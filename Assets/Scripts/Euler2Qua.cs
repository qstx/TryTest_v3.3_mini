using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Euler2Qua : MonoBehaviour
{
    public enum Mode { L_Eulers,L_Quaternion,R_Quaternion,R_revBytes}
    public Mode mode;
    [Header("L Eulers")]
    [SerializeField]
    Vector3 euler;

    [Header("L Quaternion")]
    [SerializeField]
    float x;
    [SerializeField]
    float y;
    [SerializeField]
    float z;
    [SerializeField]
    float w;

    [Header("R Quaternion")]
    [SerializeField]
    float rqx;
    [SerializeField]
    float rqy;
    [SerializeField]
    float rqz;
    [SerializeField]
    float rqw;

    [Header("R revBytes")]
    [SerializeField]
    string rbw;
    [SerializeField]
    string rbx;
    [SerializeField]
    string rby;
    [SerializeField]
    string rbz;

    Quaternion qua;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        switch (mode)
        {
            case Mode.L_Eulers:
                {
                    qua = Quaternion.Euler(euler);
                    x = qua.x;
                    y = qua.y;
                    z = qua.z;
                    w = qua.w;

                    rqx = x;
                    rqy = z;
                    rqz = -y;
                    rqw = w;

                    rbw = ((int)(rqw * 10000)).ToString("X4");
                    rbx = ((int)(rqx * 10000)).ToString("X4");
                    rby = ((int)(rqy * 10000)).ToString("X4");
                    rbz = ((int)(rqz * 10000)).ToString("X4");
                }
                break;
        }
    }
}
