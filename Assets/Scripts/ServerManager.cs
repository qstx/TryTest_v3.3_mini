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
    /*UI���*/
    [Header("UI���")]
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
            //����
            return false;
        }
        if (!int.TryParse(PortInputField.text, out int localPortNum))
        {
            Debug.LogError("Parse localPort failed!----" + PortInputField.text);
            //����
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
        client.ReceiveBufferSize = 1500;//�Ż�
        ClientInfoPanel.SetActive(true);
        ClientInfoPanel.GetComponent<Text>().text = "�ͻ��ˣ�" + client.Client.RemoteEndPoint.ToString();
    }
    private void StartReceiveFromClient()
    {
        netStream = client.GetStream();
        cache = new DataCache(roleManager.GetBoneNum());
        revThread = new Thread(RevMsg);//�Ż�,100000
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
            byte[] bytes = new byte[1024];//�Ż�
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
                }//��һ��Ѱ��֡ͷ
                if (i == bytesRead)
                {
                    continue;//������β������һ�ζ�ȡ
                }
                if (isFound)
                {
                    while (i < bytesRead)
                    {
                        if (lowerFrameLen >= 149)//��ֹ��֡��������Խ�磬��ʱ������֡ͷ
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
                                if (lowerFrameLen == revFrame[1] + 3)//�ж�֡����
                                {
                                    //ת��һ֡����
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
                                else//֡������������ǰ֡
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
    /*UI�ص�*/
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
            toggle.GetComponentInChildren<Text>().text = "�ر�";
        else
            toggle.GetComponentInChildren<Text>().text = "����";

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
            toggle.GetComponentInChildren<Text>().text = "�Ͽ�";
        else
            toggle.GetComponentInChildren<Text>().text = "����";
    }
    /********/
    /*UI����*/

    /********/
    /*�����˳�*/
    private void OnApplicationQuit()
    {
        CloseServer();
    }
    /**********/
}
