//#define CACHE_DEBUG

using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
namespace Cache
{
    public enum CacheType { Qua,Acc,Gyr,Mag};
    public class DataCache// : MonoBehaviour
    {
        private int boneNum = 0;
        private Queue<byte[]> quaCache;
        private Queue<byte[]> accCache;
        private Queue<byte[]> gyrCache;
        private Queue<byte[]> magCache;
        public Quaternion[] frameQuas;
        public Vector3[] frameAccs;
        public Vector3[] frameGyrs;

        public DataCache(int boneNum)
        {
            this.boneNum = boneNum;
            quaCache = new Queue<byte[]>();
            accCache = new Queue<byte[]>();
            gyrCache = new Queue<byte[]>();
            magCache = new Queue<byte[]>();
            frameQuas = new Quaternion[boneNum];
            frameAccs = new Vector3[boneNum];
            frameGyrs = new Vector3[boneNum];
            for (int i = 0; i < boneNum; ++i)
            {
                frameQuas[i] = Quaternion.identity;
                frameAccs[i] = Vector3.zero;
                frameGyrs[i] = Vector3.zero;
            }
        }
        public bool Get(CacheType type, out byte[] outData)
        {
            switch(type)
            {
                case CacheType.Acc:
                    if (accCache.Count>0)
                    {
                        outData = accCache.Dequeue();
                        return true;
                    }
                    else
                    {
                        outData = null;
                        return false;
                    }
                case CacheType.Gyr:
                    if (gyrCache.Count > 0)
                    {
                        outData = gyrCache.Dequeue();
                        return true;
                    }
                    else
                    {
                        outData = null;
                        return false;
                    }
                case CacheType.Mag:
                    if (magCache.Count > 0)
                    {
                        outData = magCache.Dequeue();
                        return true;
                    }
                    else
                    {
                        outData = null;
                        return false;
                    }
                default:
                    if (quaCache.Count > 0)
                    {
                        outData = quaCache.Dequeue();
                        return true;
                    }
                    else
                    {
                        outData = null;
                        return false;
                    }
            }
        }
        public void Put(CacheType type, byte[] newData)
        {
            Array.Resize(ref newData, newData[1] + 2);
#if CACHE_DEBUG
            string msg = "";
            for (int i = 0; i < newData.Length; ++i)
            {
                msg += newData[i].ToString("X2") + "|";
            }
            Debug.Log(msg);
#endif
            switch (type)
            {
                case CacheType.Acc:
                    accCache.Enqueue(newData);
                    break;
                case CacheType.Gyr:
                    gyrCache.Enqueue(newData);
                    break;
                case CacheType.Mag:
                    magCache.Enqueue(newData);
                    break;
                case CacheType.Qua:
                    //for (int i = 0; i < boneNum; ++i)
                    //{
                    //    if (newData[i * 9 + 2] == 0)
                    //    {
                    //        IMUStatusManager.SetIMUStatus(i, IMU.IMUSTATUS.OffLine);
                    //        continue;
                    //    }
                    //    IMUStatusManager.SetIMUStatus(i, IMU.IMUSTATUS.OnLine);
                    //}
                    if (quaCache.Count > 15)
                        quaCache.Clear();
                    quaCache.Enqueue(newData);
                    //Debug.Log("quaCache.Enqueue" + quaCache.Count);
                    break;
                default:
                    break;
            }
        }
        public Quaternion[] GetQuasFromCache()
        {
            //Debug.Log("quaCache.Count:" + quaCache.Count);
            if (quaCache.Count > 0)
            {
                byte[] outFrame = quaCache.Dequeue();
                //string msg = "";
                //for(int i = 0; i < outFrame.Length; ++i)
                //{
                //    msg += outFrame[i].ToString("X2");
                //}
                //Debug.Log(msg);
                for (int i = 0; i < boneNum; ++i)
                {
                    if(outFrame[i * 9 + 2]==0)
                    {
                        frameQuas[i].w = 1;
                        frameQuas[i].x = 0;
                        frameQuas[i].y = 0;
                        frameQuas[i].z = 0;
                        continue;
                    }
                    frameQuas[i].w = (short)(outFrame[i * 9 + 3] | outFrame[i * 9 + 4] << 8) * 0.0001f;
                    frameQuas[i].x = (short)(outFrame[i * 9 + 5] | outFrame[i * 9 + 6] << 8) * 0.0001f;
                    frameQuas[i].y = (short)(outFrame[i * 9 + 7] | outFrame[i * 9 + 8] << 8) * 0.0001f;
                    frameQuas[i].z = (short)(outFrame[i * 9 + 9] | outFrame[i * 9 + 10] << 8) * 0.0001f;
                    //Debug.Log(i.ToString()+frameQuas[i].ToString("F4"));
                }
            }
            else
            {
                //Debug.Log("quaCache.Count nocache");
            }
            return frameQuas;
        }
        public Quaternion[] GetCopyQuasFromCache()
        {
            return frameQuas;
        }
        public Vector3[] GetAccsFromCache()
        {
            if (accCache.Count > 0)
            {
                byte[] outFrame = accCache.Dequeue();
                for (int i = 0; i < boneNum; ++i)
                {
                    frameAccs[i].x = (short)(outFrame[i * 9 + 3] | outFrame[i * 9 + 4] << 8) * 0.01f;
                    frameAccs[i].y = (short)(outFrame[i * 9 + 5] | outFrame[i * 9 + 6] << 8) * 0.01f;
                    frameAccs[i].z = (short)(outFrame[i * 9 + 7] | outFrame[i * 9 + 8] << 8) * 0.01f;
                    //Debug.Log(frameAccs[i].ToString("F2"));
                }
            }
            else
            {
                // Debug.Log("nocache");
            }
            return frameAccs;
        }
        public Vector3[] GetGyrsFromCache()
        {
            if (gyrCache.Count > 0)
            {
                byte[] outFrame = gyrCache.Dequeue();
                for (int i = 0; i < boneNum; ++i)
                {
                    frameGyrs[i].x = (short)(outFrame[i * 9 + 3] | outFrame[i * 9 + 4] << 8) * 0.01f;
                    frameGyrs[i].y = (short)(outFrame[i * 9 + 5] | outFrame[i * 9 + 6] << 8) * 0.01f;
                    frameGyrs[i].z = (short)(outFrame[i * 9 + 7] | outFrame[i * 9 + 8] << 8) * 0.01f;
                    //Debug.Log(frameGyrs[i].ToString("F2"));
                }
            }
            else
            {
                // Debug.Log("nocache");
            }
            return frameGyrs;
        }
    }
}
