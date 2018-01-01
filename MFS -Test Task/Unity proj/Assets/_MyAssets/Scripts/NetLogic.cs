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

    private static NetLogic _instance;
    public static NetLogic instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }

    private Coroutine reconnCor;


    #region Unity Methods
    public virtual void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        DontDestroyOnLoad(this);
    }

	public virtual void Start () 
    {
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

        if (reconnCor == null)
            reconnCor = StartCoroutine(ReconnCor());
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

        if (reconnCor == null)
            reconnCor = StartCoroutine(ReconnCor());

        
    }   
    #endregion

    #region Coroutines
    IEnumerator ReconnCor()
    {
        yield return new WaitForSeconds(reconnectDelay);
        Connect();

        reconnCor = null;

    }

    #endregion
}