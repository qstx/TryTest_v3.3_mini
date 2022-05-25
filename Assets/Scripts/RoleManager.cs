using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoleManager : MonoBehaviour
{
    private GameObject Role = null;
    [Header("RoleList")]
    [SerializeField]
    private List<GameObject> RoleList = null;
    [Header("ServerManager")]
    [SerializeField]
    private ServerManager serverManager = null;

    private MotionDriver motionDriver = null;
    // Start is called before the first frame update
    void Start()
    {
        Role = Instantiate<GameObject>(RoleList[0]);
        motionDriver = new MotionDriver(Role.GetComponent<RoleBoneList>());
    }

    // Update is called once per frame
    void Update()
    {
        motionDriver.RoleDriver(serverManager.GetRunQuas());
    }
    public int GetBoneNum()
    {
        return Role.GetComponent<RoleBoneList>().boneList.Length;
    }
    public void SetRoleIni()
    {
        motionDriver.SetIni();
    }
    /*UI»Øµ÷*/
    public void OnNCaliBtnClick()
    {
        motionDriver.NCaliDataGet((Quaternion[])serverManager.GetRunQuas());
        //motionDriver.NCaliQua = (Quaternion[])serverManager.GetRunQuas().Clone();
        //Debug.LogError(motionDriver.NCaliQua[0].ToString("F4"));
        motionDriver.Calibrate();
    }
    /********/
}
