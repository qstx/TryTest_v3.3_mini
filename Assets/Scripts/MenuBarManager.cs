using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuBarManager : KeyClickHandle<Button>
{
    // Start is called before the first frame update
    void Start()
    {
        Initial();
    }
    public void OnAppExitYesBtnClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
