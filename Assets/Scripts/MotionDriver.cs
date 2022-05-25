using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionDriver// : MonoBehaviour
{
    private Quaternion[] fristQua = null;
    public Quaternion[] NCaliQua;
    private List<Quaternion[]> TCaliQua;
    private List<Quaternion[]> FCaliQua;

    private Transform[] boneList = null;
    private int boneNum;
    public bool isCalibrated { get; set; } = false;
    public MotionDriver(RoleBoneList roleBoneList)
    {
        boneList = roleBoneList.boneList;
        boneNum = boneList.Length;
        fristQua = new Quaternion[boneNum];
        for (int i = 0; i < boneNum; ++i)
        {
            fristQua[i] = boneList[i].transform.rotation;
        }
        isCalibrated = false;
    }
    private Quaternion QuaTransR2L(Quaternion qua)
    {
        qua = new Quaternion(qua.x, -qua.z, qua.y, qua.w);
        return qua;
    }
    public void Calibrate()
    {
        isCalibrated = true;
    }
    public void NCaliDataGet(Quaternion[] NCaliData)
    {
        this.NCaliQua = (Quaternion[])NCaliData.Clone();
    }
    public void SetIni()
    {
        for(int i = 0; i < boneNum; ++i)
        {
            boneList[i].rotation = fristQua[i];
        }
        isCalibrated = false;
    }
    public void RoleDriver(Quaternion[] RunQuas)
    {
        if (isCalibrated)
        {
            //Debug.Log("isCalibrated");
            for (int i = 0; i < boneNum; ++i)
            {
                //Quaternion tmp = Quaternion.Inverse(QuaTransR2L(NCaliQua[i])) * QuaTransR2L(RunQuas[i]) * fristQua[i];
                Quaternion tmp = QuaTransR2L(RunQuas[i]) * Quaternion.Inverse(QuaTransR2L(NCaliQua[i])) * fristQua[i];
                boneList[i].transform.rotation = tmp;
                //if(i==0)
                //    Debug.Log(tmp.ToString("F4"));
            }
        }
        else
        {
            //Debug.Log(RunQuas[0]);
            //Debug.Log(QuaTransR2L(RunQuas[9]));
            for (int i = 0; i < boneNum; ++i)
                boneList[i].transform.rotation = QuaTransR2L(RunQuas[i]) * fristQua[i];
        }
    }
}
