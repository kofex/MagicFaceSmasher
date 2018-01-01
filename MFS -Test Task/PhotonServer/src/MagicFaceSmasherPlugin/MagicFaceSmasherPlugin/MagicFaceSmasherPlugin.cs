using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Photon.Hive.Plugin;
using MagicFaceSmasherCommon;
using MagicFaceSmasherCommon.CustomTypes;

namespace MagicFaceSmasherPlugin
{
    class MagicFaceSmasherPlugin : PluginBase
    {        
        private Dictionary<string, Game> games;        
        public override string Name
        {
            get
            {
                return this.GetType().Name;
            }
        }

        private byte damageToPlayer = 10;

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            games = new Dictionary<string, Game>();

            if (!host.TryRegisterType(typeof(OnHitResponce), (byte)'R', OnHitResponce.Serializer, OnHitResponce.Deserializer))
            {
                PluginHost.LogInfo("Cant register custom type");
            }
            
            return base.SetupInstance(host, config, out errorMsg);
        }       


        private void AddGame(string roomName, byte maxPlayers)        
        {
            
            Game game = new Game(maxPlayers);
            game.StartFightCallback = OnFightStart;
            games.Add(roomName, game);
            PluginHost.LogInfo(string.Format("Game added to dic {0} mplrs {1} ID {2}", roomName, maxPlayers, roomName));
        }

        private void AddPlayer(byte id, string roomName)
        {
            Actor actor = new Actor(id, 100, OnPlyerDead, OnCanHit);
            PluginHost.LogInfo("Adding actor " + actor);            
            games[roomName].AddPlayer(id, actor);
        }

        #region General callbacks
        public override void OnCreateGame(ICreateGameCallInfo info)
        {
            base.OnCreateGame(info);

            //255 - имя комнаты (см. LoadbalancingPeer.cs)
            byte maxPlayers = 2;
            string roomName = PluginHost.GameId;

            PluginHost.LogInfo(string.Format("PLUGIN:: OnCreateGame! Max players {0}, Room name: {1}", maxPlayers, roomName));            

            AddGame(roomName, maxPlayers);
            
            //Actor witch create game allways has id 1
            AddPlayer(1, roomName);

            string message = string.Format("PLUGIN:: Game Created! Max plrs {0}, first actor added to plugin game logic!", maxPlayers);
            PluginHost.LogInfo(message);
            
        }

        public override void OnJoin(IJoinGameCallInfo info)
        {
            base.OnJoin(info);

            string roomName = PluginHost.GameId;

            AddPlayer((byte)info.ActorNr, roomName);

            string message = string.Format("PLUGIN:: PLayer {0} Joind to {1}!", info.ActorNr, roomName);
            PluginHost.LogInfo(message);
        }

        public override void OnLeave(ILeaveGameCallInfo info)
        {
            base.OnLeave(info);

            string roomName = PluginHost.GameId;

            PluginHost.LogInfo(string.Format("PLUGIN:: Player leaving room name {0}!", roomName));

            games[roomName].RemovePlayer(info.ActorNr);

            string message = string.Format("PLUGIN:: PLayer {0} Leave!", info.ActorNr);
            PluginHost.LogInfo(message);            
            
        }

        public override void OnCloseGame(ICloseGameCallInfo info)
        {
            
            base.OnCloseGame(info);

            string roomName = PluginHost.GameId;

            PluginHost.LogInfo(string.Format("PLUGIN:: Game Closing room name {0}!", roomName));

            games[roomName].ClearPlayerList();
            games.Remove(roomName);
            string message = string.Format("PLUGIN:: Game Closed!");
            PluginHost.LogInfo(message);
        }

        public override void OnRaiseEvent(IRaiseEventCallInfo info)
        {
            EventCodes evCode = (EventCodes)info.Request.EvCode;

            string roomName = PluginHost.GameId;
            int actorNr = info.ActorNr;                        

            if ((byte)evCode > 199)
            {

                base.OnRaiseEvent(info);
                return;                
            }           

            string message = string.Format("PLUGIN:: Catch Event code {0}", evCode);

            switch (evCode)
            {
                case EventCodes.HIT:
                    {
                        int injuredID = (int) info.Request.Data;
                        if (games[roomName].HitPlayer(injuredID, damageToPlayer))
                            message = string.Format("PLUGIN:: EVENT! Hit player {0}! with {1} damage", injuredID, damageToPlayer);
                        else
                            message = "PLUGIN:: Can't hit!!! Actor is null or not in list!";
                        
                        break;
                    }
                case EventCodes.READY:
                    {                        
                        int id = (int)info.Request.Data;
                        
                        if(games[roomName].SetReady(id))
                            message = string.Format("PLUGIN:: EVENT! Player id {0} is ready!", id);
                        else
                            message = string.Format("PLUGIN:: EVENT! Player id {0} is Already ready!", id);

                        break;
                    }
                case EventCodes.RESET_STATS:
                    {
                        message = string.Format("PLUGIN:: EVENT! Reset stats!");
                        break;
                    }
               
            }
            
            PluginHost.LogInfo(message);
            base.OnRaiseEvent(info);
        }

        #endregion 

        #region Custom Messages

        private void OnPlyerDead(byte injuredID)
        {
            PluginHost.LogInfo(string.Format("player {0} is dead!", injuredID));
            Dictionary<byte, object> msg = new Dictionary<byte, object>();
            msg.Add(245, injuredID);
            PluginHost.BroadcastEvent(ReciverGroup.All, injuredID, 0, (byte) EventCodes.DEAD, msg, CacheOperations.AddToRoomCache);
        }

       private void OnCanHit(byte injuredID, byte hp)
        {
            OnHitResponce resp = new OnHitResponce(injuredID, hp);
            
            Dictionary<byte, object> sendCommand = new Dictionary<byte,object>();
            //245 - так как это команда для для передачи эвента (см. доки по фотону)
            sendCommand.Add(245, resp);

            PluginHost.BroadcastEvent(ReciverGroup.All, injuredID, 0, (byte) EventCodes.HIT, sendCommand, CacheOperations.AddToRoomCache);
        }

        private void OnResetStatistic()
        {
            PluginHost.BroadcastEvent(ReciverGroup.All, 0, 0, (byte) EventCodes.RESET_STATS, null, CacheOperations.AddToRoomCache);
        }

        private void OnFightStart(Game game)
        {            
            PluginHost.BroadcastEvent(ReciverGroup.All, game.GetFirstPlayerInDict().actorNr, 0, (byte)EventCodes.START_FIGHT, null, CacheOperations.AddToRoomCache);
        }
       
        #endregion

    }
}
