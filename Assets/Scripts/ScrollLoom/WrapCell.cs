/// <summary>
/// Author      Liuxun
/// Date        2018/10/12 14:01:16
/// Description:IWrapCell
/// </summary>

using UnityEngine;
/// <summary>
/// IWrapCell
/// </summary>

public abstract class WrapCell : MonoBehaviour
{
    private object dataObject;
    private int dataIndex;
  
   

    public int DataIndex
    {
        get { return dataIndex; }
    }

    public object DataObject
    {
        get
        {
            return dataObject;
        }
    }

    public void SetInfo(object data, int dataIndex)
    {
        gameObject.SetActive(true);
        this.dataIndex = dataIndex;
        this.dataObject = data;
        RefreshUi(data, dataIndex);
    }

    public void Hidden()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <param name="dataIndex">数据在集合里的index</param>
    protected abstract void RefreshUi(object data, int dataIndex);
}

