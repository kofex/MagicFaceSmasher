using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicFaceSmasherPlugin
{
    public class Game
    {
        public delegate void OnFigthStart(Game game);
        public OnFigthStart StartFightCallback;

        private Dictionary<int, Actor> players;        
        private byte maxPlayers;

        private int readyPlrCount;
        private int readyPlayers
        {
            get { return readyPlrCount; }
            set
            {
                readyPlrCount = value;
                if (readyPlrCount == maxPlayers)
                {
                    FightStart();
                }
            }
        }
        

        public Game(byte maxPlrsInGame)
        {
            players = new Dictionary<int, Actor>();
            maxPlayers = maxPlrsInGame;
            readyPlrCount = 0;
        }

        public void AddPlayer(int id, Actor plr)
        {
            if (players.Count > maxPlayers)
                return;
            
            if (!players.ContainsValue(plr))
            {
                players.Add(id, plr);
            }
        }
               
        public void RemovePlayer(int plrNr)
        {
            if (players.ContainsKey(plrNr))
            {
                if (GetActorFromPlyers(plrNr).isReady)
                    readyPlayers--;
                players.Remove(plrNr);            
            }
        }

        public void ClearPlayerList()
        {
            players.Clear();
        }

        public bool HitPlayer(int plrId, byte dgm)
        {
            if (players.ContainsKey(plrId))
            {
                Actor player = players[plrId];
                player.hp -= dgm;
                return true;
            }

            return false;
           
        }

        public Actor GetFirstPlayerInDict()
        {
            Actor player = null;
            player = players.First().Value;

            return player;
        }

        public Actor GetActorFromPlyers(int plrId)
        {
            Actor player = null;
            players.TryGetValue(plrId, out player);

            return player;
        }

        /// <summary>
        /// Sets player ready
        /// </summary>
        /// <param name="id">player id</param>
        public bool SetReady(int id)
        {
            Actor player = GetActorFromPlyers(id);
            if (player.isReady)
                return false;
            
            player.isReady = true;
            readyPlayers++;

            return true;
        }

        private void FightStart()
        {
            StartFightCallback(this);
        }
    }
}
