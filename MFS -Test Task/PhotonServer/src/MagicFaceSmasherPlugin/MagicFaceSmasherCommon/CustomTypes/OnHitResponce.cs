using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MagicFaceSmasherCommon.CustomTypes
{
    public class OnHitResponce
    {
        public byte injuredID;
        public byte newHP;

        public OnHitResponce()
        {
           
        }
        public OnHitResponce(byte injured, byte hp)
        {
            injuredID = injured;
            newHP = hp;
        }
        

        /// <summary>
        /// Метод только для Unity
        /// </summary>
        /// <param name="outStream">Вместо Stream нужно использовать наследника StreamBuffer</param>
        /// <param name="customObject"></param>
        /// <returns></returns>
        public static short UnitySerializer(Stream outStream, object customObject)
        {
            OnHitResponce resp = (OnHitResponce) customObject;

            short size;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(resp.injuredID);
                    bw.Write(resp.newHP);

                    size = (short) stream.Length;
                    outStream.Write(stream.ToArray(), 0, size);
                } 
            }

            return size;
        }

        /// <summary>
        /// Метод только для Unity
        /// </summary>
        /// <param name="inStream"> Вместо Stream нужно использовать наследника StreamBuffer</param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static object UnityDeserializer(Stream inStream, short length)
        {
             OnHitResponce resp = new OnHitResponce();

            using (BinaryReader br = new BinaryReader(inStream))
            {
                resp.injuredID = br.ReadByte();
                resp.newHP = br.ReadByte(); 
            }

            return resp;
        }

        public static byte[] Serializer(object customObject)
        {
            OnHitResponce resp = (OnHitResponce)customObject;

            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    bw.Write(resp.injuredID);
                    bw.Write(resp.newHP);                    

                    return stream.ToArray();
                }
            }
        }

        public static object Deserializer(byte[] bytes)
        {
            OnHitResponce resp = new OnHitResponce();

            using (MemoryStream stream = new MemoryStream(bytes))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    resp.injuredID = br.ReadByte();
                    resp.newHP = br.ReadByte();                    
                }
            }

            return resp;
        }

    }
}
