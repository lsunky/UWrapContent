/// <summary>
/// Author      Liuxun
/// Date        2018/10/12 15:18:36
/// Description:TestCell
/// </summary>

using System;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// TestCell
/// </summary>

public class TestWrapCell : WrapCell
{
    public Text txtValue;
    public Text txtIndex;
    protected override void RefreshUi(object data, int dataIndex)
    {
        int num = (int)data;
        txtValue.text = num.ToString();
        txtIndex.text = dataIndex.ToString();
    }
}

