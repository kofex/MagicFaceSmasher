using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerManager))]
public class NetPlayer : Photon.MonoBehaviour 
{
    private PlayerManager player;
    public virtual void Start()
    {
        player = GetComponent<PlayerManager>();
    }
	
    //Должны установть ready если игрок уже нажал говтов, до того как мы подключились к комнате
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (!player)
            return;

        bool isReady = false;

        if (stream.isWriting)
        {            
             isReady = player.isReady;
            stream.Serialize(ref isReady);
        }
        else
        {
            //Мы не отжимаем ready поэтому не имеет смысла чиать данные если и так уже стоит ready
            if (player.isReady)
                return;

            stream.Serialize(ref isReady);
            if (isReady)
                player.OnReceiveReady();
        }
    }
}
