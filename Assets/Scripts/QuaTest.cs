using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuaTest : MonoBehaviour
{
    [SerializeField]
    GameObject First;
    Quaternion firstPose;

    [SerializeField]
    GameObject Second;
    Quaternion caliPose;
    Quaternion secondPose;

    [SerializeField]
    GameObject Cor1Pose;

    [SerializeField]
    GameObject Cor2Pose;

    [SerializeField]
    bool calibool = false;
    [SerializeField]
    bool updata = false;
    
    // Start is called before the first frame update
    void Start()
    {
        firstPose = First.transform.rotation;
        secondPose = Second.transform.rotation;
        caliPose = secondPose;
    }

    // Update is called once per frame
    void Update()
    {
        secondPose = Second.transform.rotation;
        if(updata)
        {
            caliPose = secondPose;
            updata = false;
        }
        if (calibool)
        {
            First.transform.rotation = Quaternion.Euler(Quaternion.Inverse(caliPose).eulerAngles + secondPose.eulerAngles)*firstPose;
            //Debug.Log("calied");
            //Debug.Log(newPose.ToString("F4") + "---" + First.transform.rotation.ToString("F4"));
        }
        else
        {
            First.transform.rotation = firstPose;
            Debug.Log("uncalied");
            //Debug.Log(newPose.ToString("F4") + "---" + First.transform.rotation.ToString("F4"));
        }
    }
}
