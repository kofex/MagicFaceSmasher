using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicFaceSmasherPlugin
{
    public class Actor 
    {
        public byte actorNr;
        public bool isReady;

        private byte _hp;
        public byte hp
        {
            get { return _hp; }
            set 
            {
                if (dead)
                    return;

                if (value <= 0)
                {
                    _hp = 0;
                    PlayerDead();
                }
                else
                {
                    _hp = value;
                    PlayerHited();
                }
            }
        }        

        public bool dead
        {
            get;
            private set;
        }

        public delegate void OnDeadCallback(byte injuredID);
        private OnDeadCallback OnDead;

        public delegate void OnHitCallback(byte injuredID, byte hp);
        private OnHitCallback OnHit;

        public Actor(byte actorNbr, byte health, OnDeadCallback deadCallback, OnHitCallback hitCallback)
        {
            actorNr = actorNbr;
            _hp = health;
            dead = false;
            isReady = false;

            OnDead = deadCallback;
            OnHit = hitCallback;

        }

        private void PlayerDead()
        {
            dead = true;
            // Отправляем игроку сообщение о проигрыше         
            OnDead(actorNr);
        }

        private void PlayerHited()
        {
            // Отправляем разрешение на удра игрока и на апдейт хп
            OnHit(actorNr, hp);
        }
    }
}
