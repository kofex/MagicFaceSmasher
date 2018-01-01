using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using MagicFaceSmasherCommon;
using MagicFaceSmasherCommon.CustomTypes;

public class NetGameScnenManager: Photon.MonoBehaviour
{
    public byte maxPlrsInRoom = 2;
    public int maxErrorTryes = 3;
    public Transform spawnPointRoot;

    private static NetGameScnenManager _instance;
    private RoomOptions _roomOptions;
    private int errorTryes = 0;
    private Coroutine connCor;    
    private Transform[] spawnPoints;
    private GameManager gameManager;
    private string _roomName = string.Empty;
    
    public string roomName
    {
        get { return _roomName; }
        private set { _roomName = value; }
    }

    public static NetGameScnenManager instance
    {
        get { return _instance; }
        private set { _instance = value; }
    }


    #region Unity Methods
    public virtual void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
            Destroy(gameObject);

        errorTryes = 0;
        gameManager = GameManager.instance;

        FillSpawnPointsFromRootObj();      
  
        _roomOptions = new RoomOptions();
        _roomOptions.MaxPlayers = maxPlrsInRoom;
        _roomOptions.Plugins = new string[] {"MagicFaceSmasherPlugin"};

        ConnectToRoom();
    }    

    public virtual void Start()
    {        
        PhotonNetwork.OnEventCall += OnSceneEvents;
    }

    public virtual void OnDestroy()
    {
        PhotonNetwork.OnEventCall -= OnSceneEvents;
    }

    #endregion

    #region Custom Methods
    /// <summary>
    /// Коннектимся в комнату или создаем если ее нет
    /// </summary>
    public void ConnectToRoom()
    {
        string sceneName = gameManager.sceneName;
        RoomInfo[] rooms = PhotonNetwork.GetRoomList();

        if (rooms.Length > 0)        
        {
            RoomInfo[] filtredRooms = (from room in rooms where 
                                           room.Name.Split(new string[] { "_" }, System.StringSplitOptions.RemoveEmptyEntries)[0].Equals(sceneName)
                                           &&
                                           room.PlayerCount < maxPlrsInRoom && room.IsOpen && room.IsVisible
                                           select room).ToArray();           

            if (filtredRooms.Length > 0)
            {
                PhotonNetwork.JoinRoom(filtredRooms[0].Name);
                return;
            }            
        }

        roomName = string.Format("{0}_{1}", sceneName, rooms.Length);
        PhotonNetwork.CreateRoom(roomName, _roomOptions, null);

    }

    /// <summary>
    /// Получаем свободное место, для спавна
    /// </summary>
    /// <returns></returns>
    private Transform GetSpawnPosition()
    {
        float radius = 1.5f;        
        foreach (Transform sp in spawnPoints)
        {    
            //первый всегда plane
            if (Physics.OverlapSphere(sp.position, radius).Length < 2)            
            {                
                return sp;
            }            
        }

        return null;
    }

    private void FillSpawnPointsFromRootObj()
    {
        spawnPoints = new Transform[spawnPointRoot.childCount];
        int inx = 0;

        foreach (Transform spawnPoint in spawnPointRoot)
        {
            spawnPoints[inx++] = spawnPoint;
        }
    }

    /// <summary>
    /// Когда игра начинается комната закрыватеся
    /// </summary>
    internal void CloseRoom()
    {
        PhotonNetwork.room.IsOpen = false;                
    }
    
    #endregion



    #region Callbacks
    /// <summary>
    /// Эвенты которы мы обрабатываем только для сцены (комнаты)
    /// </summary>
    /// <param name="eventCode"></param>
    /// <param name="content"></param>
    /// <param name="sender"></param>
    public void OnSceneEvents(byte eventCode, object content, int sender)
    {        
        EventCodes evCode = (EventCodes)eventCode;
        //Debug.Log(string.Format("On SceneEvents code {0} ({1}) Sender {2}", eventCode, evCode, sender));

        switch (evCode)
        {            
            case EventCodes.READY:
                {
                    PlayerManager player = gameManager.GetPlayerById((int)content);
                    if (player)
                        player.OnReceiveReady();
                    else
                        Debug.LogError(string.Format("Player with id {0} is null", (int)content));
                    break;
                }
            case EventCodes.START_FIGHT:
                {
                    gameManager.FightStart();
                    break;
                }
            case EventCodes.HIT:
                {
                    //-1 - эвент с плагина
                    if (sender != -1)
                        break;

                    OnHitResponce hit = (OnHitResponce)content;
                    int inhured = hit.injuredID;
                    int hp = hit.newHP;
                    gameManager.GetOtherPlayer(inhured).PlayAttack();
                    gameManager.GetPlayerById(inhured).UpdateHP(hp);

                    //Debug.Log(string.Format("id {0} hp {1}", inhured, hp));
                    break;
                }
            case EventCodes.DEAD:
                {
                    byte id = (byte)content;
                    gameManager.GetOtherPlayer(id).PlayAttack();
                    gameManager.GetPlayerById(id).OnDead();                    
                    break;
                }
        }
    }    

    public virtual void OnJoinedRoom()
    {
        Debug.Log(string.Format("Joined!"));
        //Небольшая задержка, чтобы в сцене все успело заспавниться
        StartCoroutine(OnJoinRoomCorr());
        
    }

    // codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg. 
    public virtual void OnPhotonJoinRoomFailed(object[] codeAndMsg)
    {
        ErrorCode code = (ErrorCode) codeAndMsg[0];
        string errorMasg = (string) codeAndMsg[1];

        Debug.LogError(string.Format("{0}! Code {1}", errorMasg, code));

        if (errorTryes < maxErrorTryes)
            if(connCor == null)
                StartCoroutine(ConnnectionToRoomCor());     

    }

    // codeAndMsg[0] is short ErrorCode. codeAndMsg[1] is string debug msg.  
    public virtual void OnPhotonCreateRoomFailed(object[] codeAndMsg)
    {
        short code = (short)codeAndMsg[0];        
        string errorMasg = (string)codeAndMsg[1];

        Debug.LogError(string.Format("{0}! Code {1}", errorMasg, code));

        if (errorTryes < maxErrorTryes)
            if (connCor == null)
                StartCoroutine(ConnnectionToRoomCor());            
    }

    public virtual void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
    {
        Debug.Log(string.Format("Player {0} left the room", otherPlayer.ID));
        if(gameManager.CanFight())
            gameManager.Win();
    }  
    #endregion


    #region Events to send

    public void SendReady()
    {
        PhotonNetwork.RaiseEvent((byte)EventCodes.READY, PhotonNetwork.player.ID, true, null);
    }

    public void OnHit(int hitedId)
    {
        PhotonNetwork.RaiseEvent((byte)EventCodes.HIT, hitedId, true, null);
    }

    #endregion

    #region Coroutines
    IEnumerator ConnnectionToRoomCor()
    {
        errorTryes++;
        yield return new WaitForSeconds(1.0f);

        connCor = null;
        ConnectToRoom();        
    }

    IEnumerator OnJoinRoomCorr()
    {
        yield return new WaitForSeconds(1.0f);

        Transform sp = GetSpawnPosition();
        if (sp == null)
        {
            Debug.LogError(string.Format("Error! No spawn point! {0}", this));
            yield break;
        }

        string name = gameManager.characters[gameManager.selectedIndex].name;
        PhotonNetwork.Instantiate(name, sp.position, sp.rotation, 0);        
    }
    #endregion
}
