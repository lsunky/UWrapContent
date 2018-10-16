/// <summary>
/// Author      Liuxun
/// Date        2018/10/12 15:33:08
/// Description:TempWrap
/// </summary>

using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
/// <summary>
/// TempWrap
/// </summary>

public class TempWrap : MonoBehaviour
{
    public UWrapContent wrap;
    public UWrapContent fullLoomWrap;
    string maxCountStr = "100";
    public Button btn;

    private void Start()
    {
        btn.onClick.AddListener(BtnClickHandle);
    }

    private void BtnClickHandle()
    {
        List<int> list = new List<int>();
        int length = int.Parse(maxCountStr);
        for (int i = 0; i < length; i++)
        {
            list.Add(i);
        }
        wrap.InitWithData(list);
        fullLoomWrap.InitWithData(list);
    }

    private void OnGUI()
    {
        maxCountStr = GUI.TextField(new Rect(0, 0, 400, 80), maxCountStr);
    }


}

