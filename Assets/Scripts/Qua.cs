using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class Qua : MonoBehaviour
{
    [SerializeField]
    GameObject Object;
    [SerializeField]
    Quaternion quaternion;
    Quaternion frist= Quaternion.identity;
    [SerializeField]
    bool show = false;
    // Start is called before the first frame update
    void Start()
    {
        frist = Quaternion.Euler(0,0,30);
        Object.transform.rotation = frist;
        Debug.Log("frist:" + Quaternion.identity.ToString("F2"));
    }

    // Update is called once per frame
    void Update()
    {
        if (show)
        {
            //Debug.Log("Before:" + frist.ToString("F2"));
            //Object.transform.rotation = quaternion*frist;
            //Debug.Log("Trans:" + quaternion.ToString("F2")+ Quaternion.Inverse(quaternion).ToString("F2"));
            //Debug.Log("Then:" + Object.transform.rotation.ToString("F2"));
            Debug.Log("1" + Quaternion.Euler(0, 0, 30).ToString("F4"));
            Debug.Log("2" + Quaternion.Euler(0, 60, 0).ToString("F4")); Object.transform.rotation = Quaternion.Euler(0, 60, 0) * Quaternion.Euler(0, 0, 30);
            Debug.Log("3" + Quaternion.Euler(0, 60, 0).ToString("F4"));
        }
    }
}
