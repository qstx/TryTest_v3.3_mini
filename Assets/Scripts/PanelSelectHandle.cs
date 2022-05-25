using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PanelSelectHandle : KeyClickHandle<Toggle>
{
    // Start is called before the first frame update
    void Start()
    {
        Initial();
    }
    public override void OnKeyClick(Toggle key)
    {
        keyMap[key].SetActive(key.isOn);
    }
}
