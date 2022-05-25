using System.Collections;
using System.Collections.Generic;
using System;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using Cache;

public class ServerManager : MonoBehaviour
{
    /*UI组件*/
    [Header("UI组件")]
    [SerializeField]
    private Text IPInputField = null;
    [SerializeField]
    private Text PortInputField = null;
    [SerializeField]
    private GameObject ClientInfoPanel = null;
    [SerializeField]
    private Toggle ClientHandleToggle = null;
    /*RoleManager*/
    [Header("RoleManager")]
    [SerializeField]
    private RoleManager roleManager = null;

    private NetworkStream netStream = null;
    public DataCache cache = null;
    private Thread revThread = null;
    private byte[] revFrame = new byte[150];
    private TcpClient client = null;
    private TcpListener listener = null;

    public bool isServerOn { get; private set; } = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(isServerOn&&listener.Pending())
        {
            ConnectClient();
        }
    }
    private bool StartServer()
    {
        if (!IPAddress.TryParse(IPInputField.text, out IPAddress localIPAddress))
        {
            Debug.LogError("Parse localIP failed!----" + IPInputField.text);
            //报错
            return false;
        }
        if (!int.TryParse(PortInputField.text, out int localPortNum))
        {
            Debug.LogError("Parse localPort failed!----" + PortInputField.text);
            //报错
            return false;
        }

        listener = new TcpListener(localIPAddress, localPortNum);
        listener.Start(1);
        isServerOn = true;
        return true;
    }
    private void CloseServer()
    {
        DisconnectClient();
        if (listener != null)
        {
            listener.Stop();
            listener = null;
        }
        isServerOn = false;
    }
    private void ConnectClient()
    {
        client = listener.AcceptTcpClient();
        client.NoDelay = true;
        client.ReceiveBufferSize = 1500;//优化
        ClientInfoPanel.SetActive(true);
        ClientInfoPanel.GetComponent<Text>().text = "客户端：" + client.Client.RemoteEndPoint.ToString();
    }
    private void StartReceiveFromClient()
    {
        netStream = client.GetStream();
        cache = new DataCache(roleManager.GetBoneNum());
        revThread = new Thread(RevMsg);//优化,100000
        revThread.Start();
    }
    private void StopReceiveFromClient()
    {
        if (revThread != null)
        {
            revThread.Interrupt();
            revThread.Abort();
            cache = null;
            netStream.Close();
            netStream = null;
        }
        revThread = null;
        roleManager.SetRoleIni();
    }
    private void DisconnectClient()
    {
        StopReceiveFromClient();
        if (client != null)
        {
            client.Close();
            client = null;
            ClientHandleToggle.isOn = false;
        }
        ClientInfoPanel.SetActive(false);
    }
    //ffff00900110270000000000000210270000000000000310270000000000000410270000000000000510270000000000000610270000000000000710270000000000000810270000000000000910270000000000000a10270000000000000b10270000000000000c10270000000000000d10270000000000000e10270000000000000f1027000000000000101027000000000000
    private void RevMsg()
    {
        int lowerFrameLen = -1;
        bool isFound = false;
        while (true)
        {
            byte[] bytes = new byte[1024];//优化
            int bytesRead = netStream.Read(bytes, 0, bytes.Length);
            //ns.Flush();
            if (bytesRead > 0)
            {
#if TCP_RX_DEBUG
                string msg = "";
                for (int z = 0; z < bytesRead; ++z)
                {
                    msg += bytes[z].ToString("X2") + "|";
                }
                Debug.Log(msg);
#endif
                int i = 0;
                while (!isFound && i + 1 < bytesRead)
                {
                    ++i;
                    if (bytes[i] == 255 && bytes[i - 1] == 255)
                    {
                        isFound = true;
                        ++i;
                    }
                }//第一次寻找帧头
                if (i == bytesRead)
                {
                    continue;//读到结尾进行下一次读取
                }
                if (isFound)
                {
                    while (i < bytesRead)
                    {
                        if (lowerFrameLen >= 149)//防止丢帧导致数组越界，此时重新找帧头
                        {
                            isFound = false;
                            lowerFrameLen = -1;
                            break;
                        }
                        else
                        {
                            try
                            {
                                revFrame[++lowerFrameLen] = bytes[i];
                            }
                            catch (Exception e)
                            {
                                Debug.LogError(e.Message);
                                Debug.LogError(lowerFrameLen.ToString() + "--" + i.ToString());
                            }
                            if (lowerFrameLen > 40 && revFrame[lowerFrameLen] == 255 && revFrame[lowerFrameLen - 1] == 255)
                            {
                                if (lowerFrameLen == revFrame[1] + 3)//判断帧完整
                                {
                                    //转移一帧数据
                                    switch (revFrame[0])
                                    {
                                        case 0:
                                            //Debug.Log("GetQuas");
                                            cache.Put(CacheType.Qua, revFrame);
                                            break;
                                        case 1:
                                            //GetAcc();
                                            cache.Put(CacheType.Acc, revFrame);
                                            break;
                                        case 2:
                                            //GetGyr();
                                            cache.Put(CacheType.Gyr, revFrame);
                                            break;
                                        case 3:
                                            //GetMag();
                                            cache.Put(CacheType.Mag, revFrame);
                                            break;
                                        default:
                                            break;
                                    }
                                    lowerFrameLen = -1;
                                }
                                else//帧不完整丢弃当前帧
                                {
                                    lowerFrameLen = -1;
                                }
                            }
                            ++i;
                        }
                    }
                }
            }
        }
    }
    public Quaternion[] GetRunQuas()
    {
        if (cache == null)
        {
            Quaternion[] tmp = new Quaternion[roleManager.GetBoneNum()];
            for(int i= tmp.Length-1;i>=0;--i)
            {
                tmp[i] = Quaternion.identity;
            }
            return tmp;
        }
        return cache.GetQuasFromCache();
    }
    /*UI回调*/
    public void OnServerSettingToggleChanged(Toggle toggle)
    {
        if (toggle.isOn)
        {
            if (!StartServer())
                toggle.isOn = false;
        }
        else
        {
            CloseServer();
        }
        if (toggle.isOn)
            toggle.GetComponentInChildren<Text>().text = "关闭";
        else
            toggle.GetComponentInChildren<Text>().text = "开启";

    }
    public void OnClientHandleToggleChanged(Toggle toggle)
    {
        if(toggle.isOn)
        {
            StartReceiveFromClient();
        }
        else
        {
            DisconnectClient();
        }
        if (toggle.isOn)
            toggle.GetComponentInChildren<Text>().text = "断开";
        else
            toggle.GetComponentInChildren<Text>().text = "连接";
    }
    /********/
    /*UI控制*/

    /********/
    /*程序退出*/
    private void OnApplicationQuit()
    {
        CloseServer();
    }
    /**********/
}
