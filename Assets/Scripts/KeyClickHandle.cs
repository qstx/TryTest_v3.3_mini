using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class KeyClickHandle<TKey> : MonoBehaviour
{
    protected Dictionary<TKey, GameObject> keyMap = null;
    [SerializeField]
    protected List<TKey> keys = null;
    [SerializeField]
    protected List<GameObject> values = null;
    public void Initial()
    {
        keyMap = new Dictionary<TKey, GameObject>();
        for (int i = 0; i < keys.Count; ++i)
            keyMap.Add(keys[i], values[i]);
    }
    public virtual void OnKeyClick(TKey key)
    {
        keyMap[key].SetActive(true);
    }
    public virtual void OnObjExitBtnClick(TKey key)
    {
        keyMap[key].SetActive(false);
    }
}
