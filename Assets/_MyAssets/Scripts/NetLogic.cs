using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicFaceSmasherCommon;
using MagicFaceSmasherCommon.CustomTypes;
using ExitGames.Client.Photon;
using System.IO;

public class NetLogic : Photon.MonoBehaviour 
{
    public float reconnectDelay = 2; //sec
    public string gameVersion;
	public int maxReconnectionCount = 3;

	public static NetLogic instance { get; private set; }

	private Coroutine _reconnCor;
	private WaitForSeconds _recconnectionWait;
	private int _reconnCount;

    #region Unity Methods
    public virtual void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
	        Destroy(gameObject);
	        return;
        }

        DontDestroyOnLoad(this);
    }

	public virtual void Start () 
    {
	    _recconnectionWait =  new WaitForSeconds(reconnectDelay);

		PhotonNetwork.autoJoinLobby = true;
        Connect();
        PhotonPeer.RegisterType(typeof(OnHitResponce), (byte)'R', OnHitResponce.UnitySerializer, OnHitResponce.UnityDeserializer);
        PhotonNetwork.OnEventCall += OnCommonEvents;        
	}
    #endregion    


    #region Custom Methods
    public void Connect()
    {
        GameManager gameManager = GameManager.instance;
        string ip = gameManager.connectionIP;
        int port = gameManager.connectionPort;

        PhotonNetwork.PhotonServerSettings.ServerAddress = ip;
        PhotonNetwork.PhotonServerSettings.ServerPort = port;

        PhotonNetwork.ConnectUsingSettings(gameVersion);
    }

    public void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    #endregion

    #region Callbacks
    /// <summary>
    /// Получение общих эвентов не зависимо от сцены
    /// </summary>
    /// <param name="eventCode"></param>
    /// <param name="content"></param>
    /// <param name="sender"></param>
    private void OnCommonEvents(byte eventCode, object content, int sender)
    {
        EventCodes evCode = (EventCodes) eventCode;
        //Debug.Log(string.Format("On Common Events come {0} ({1}) Sender {2}", eventCode, evCode, sender));

        switch (evCode)
        {
            case EventCodes.RESET_STATS:
                {
                    GameManager.instance.ResertCharactersStats();
                    break;
                }           
        }
    }

    public virtual void OnConnectedToPhoton()
    {
        if (MainMenuLogic.instance)
            MainMenuLogic.instance.OnlineState();
	    GameManager.instance.isOfflineMode = false;
	    _reconnCount = 0;
    }

    /// <summary>
    /// Если дисконнект после того как соединение было установлено
    /// </summary>
    /// <param name="cause"></param>
    public virtual void OnConnectionFail(DisconnectCause cause) 
    {
        Debug.LogError(string.Format("Connection dropped! {0}", cause.ToString()));

        if (MainMenuLogic.instance)
            MainMenuLogic.instance.OnlineState();

        if (_reconnCor == null)
            _reconnCor = StartCoroutine(ReconnCor());
    }

    /// <summary>
    /// Если не получилось подключиться
    /// </summary>
    /// <param name="cause"></param>
    public virtual void OnFailedToConnectToPhoton(DisconnectCause cause)
    {
        Debug.LogError(string.Format("Connection failed! {0}", cause.ToString()));

        if (MainMenuLogic.instance)
            MainMenuLogic.instance.OnlineState();

        if (_reconnCor == null)
            _reconnCor = StartCoroutine(ReconnCor());

        
    }   
    #endregion

    #region Coroutines
    IEnumerator ReconnCor()
    {
	    GameManager.instance.isOfflineMode = true;
	    if (_reconnCount > maxReconnectionCount)
	    {
		    _reconnCor = null;
			yield break;
	    }

	    yield return _recconnectionWait;
	    _reconnCount++;
        Connect();
	    GameManager.instance.isOfflineMode = false;

		_reconnCor = null;
    }

    #endregion
}