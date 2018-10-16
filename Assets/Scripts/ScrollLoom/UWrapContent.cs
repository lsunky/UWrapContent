using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Author      Liuxun
/// Date        2018/10/12 15:33:08
/// Description:UWrapContent
/// </summary>

public class UWrapContent : MonoBehaviour {

    enum AdapationType
    {
        /// <summary>
        /// 计算列数
        /// </summary>
        ModifyColumns,

        /// <summary>
        /// 固定列数
        /// </summary>
        FixedColumns,
    }

    [SerializeField]
    private ScrollRect scrollRect;
    [SerializeField]
    private WrapCell cellPrefab;
    [SerializeField]
    private AdapationType adapationType;
    [SerializeField]
    private Vector2 cellSize = new Vector2(50, 50);
    [SerializeField]
    private Vector2 cellOffset;
    [SerializeField]
    private bool isFullLoop;
    [SerializeField]
    private int numberOfColumns = 1;

    private LinkedList<WrapCell> localCellsPool = new LinkedList<WrapCell>();
    public LinkedList<WrapCell> cellsInUse = new LinkedList<WrapCell>();

    private bool initFinish;
    private int visibleCellsRowCount;
    private int visibleCellsTotalCount;
    private IList cellDataList;
    private int preFirstVisibleIndex;

    private bool vertical
    {
        get { return scrollRect.vertical; }
    }
    
    private void Awake()
    {
        scrollRect.onValueChanged.AddListener(OnValueChangedHandle);
    }

    private void OnValueChangedHandle(Vector2 arg0)
    {
        CalculateCurrentIndex();
    }

    /// <summary>
    /// 刷新某一个值
    /// </summary>
    /// <param name="data"></param>
    /// <param name="index"></param>
    public void UpdateCellData<T>(T data, int index = -1)
    {
        if (index == -1)
        {
            index = cellDataList.IndexOf(data);
        }
        if (index < 0)
        {
            return;
        }

        foreach (WrapCell cell in cellsInUse)
        {
            if (cell.DataIndex == index)
            {
                cell.SetInfo(data, index);
            }
        }
    }

    /// <summary>
    /// 重新计算并绘制界面
    /// 此方法公开，有时候外部需要调用
    /// </summary>
    public void CalculateCurrentIndex()
    {
        if (cellDataList == null)
            return;
        int firstVisibleIndex;
        if (vertical)
        {
            firstVisibleIndex = Mathf.FloorToInt((scrollRect.content.anchoredPosition.y) / (cellSize.y + cellOffset.y));
        }
        else
        {
            firstVisibleIndex = Mathf.FloorToInt((- scrollRect.content.anchoredPosition.x) / (cellSize.x+cellOffset.x));
        }

        if (!isFullLoop)
        {
            int limit = Mathf.CeilToInt((float)cellDataList.Count / (float)numberOfColumns) - visibleCellsRowCount;
            if (firstVisibleIndex < 0 || limit <= 0)
                firstVisibleIndex = 0;
            else if (firstVisibleIndex >= limit)
                firstVisibleIndex = limit - 1;
        }
        
        if (preFirstVisibleIndex != firstVisibleIndex)
        {
            bool scrollingPositive = preFirstVisibleIndex < firstVisibleIndex;
            int indexDelta = Mathf.Abs(preFirstVisibleIndex - firstVisibleIndex);

            int deltaSign = scrollingPositive ? +1 : -1;

            for (int i = 1; i <= indexDelta; i++)
                UpdateContent(preFirstVisibleIndex + i * deltaSign, scrollingPositive);

            preFirstVisibleIndex = firstVisibleIndex;
        }
    }

    void UpdateContent(int cellIndex, bool scrollingPositive)
    {
        int index = scrollingPositive ? ((cellIndex - 1) * numberOfColumns) + (visibleCellsTotalCount) : (cellIndex * numberOfColumns);
        for (int i = 0; i < numberOfColumns; i++)
        {
            FreeCell(scrollingPositive);
            ShowCell(index + i, scrollingPositive);
        }
    }

    void FreeCell(bool scrollingPositive)
    {
        LinkedListNode<WrapCell> cell = null;
        if (scrollingPositive)
        {
            cell = cellsInUse.First;
            cellsInUse.RemoveFirst();
            localCellsPool.AddLast(cell);
        }
        else
        {
            cell = cellsInUse.Last;
            cellsInUse.RemoveLast();
            localCellsPool.AddFirst(cell);
        }
    }

    public void InitWithData(IList list,bool resetPos = true)
    {
        if (!initFinish)
        {
            InitData();
            SetCellsPool();
            initFinish = true;
        }

        if (resetPos)
        {
            if (vertical)
                scrollRect.verticalScrollbar.value = 1f;
            else
                scrollRect.horizontalScrollbar.value = 1f;
        }

        cellDataList = list;
        ResetContent();
    }

    void ShowCell(int cellIndex, bool scrollingPositive)
    {
        WrapCell tempCell = GetCellFromPool(scrollingPositive);
        if (!isFullLoop)
        {
            if (cellIndex < cellDataList.Count)
                tempCell.SetInfo(cellDataList[cellIndex], cellIndex);
            else
                tempCell.Hidden();
        }
        else
        {
            int realIndexInList = cellIndex % cellDataList.Count;
            if (realIndexInList < 0)
                realIndexInList = cellDataList.Count + realIndexInList;
            tempCell.SetInfo(cellDataList[realIndexInList], realIndexInList);
        }
        
        PositionCell(tempCell.gameObject, cellIndex);
    }

    void PositionCell(GameObject go, int index)
    {
        int rowMod = index % numberOfColumns;
        int tmepIndex = index >= 0 ? index : index + 1;
        rowMod = rowMod >= 0 ? rowMod : rowMod + numberOfColumns;
        int addValue = index >= 0 ? 0 : 1;
        Vector2 anchoredPos;
        if(vertical)
            anchoredPos = new Vector2((cellSize.x + cellOffset.x) * rowMod , (addValue - (tmepIndex / numberOfColumns)) * (cellSize.y + cellOffset.y));
        else
            anchoredPos = new Vector2((cellSize.x + cellOffset.x )* (tmepIndex / numberOfColumns - addValue) , -rowMod * (cellSize.y + cellOffset.y));
        go.GetComponent<RectTransform>().anchoredPosition = anchoredPos;
    }

    WrapCell GetCellFromPool(bool scrollingPositive)
    {
        if (localCellsPool.Count == 0)
            return null;
        LinkedListNode<WrapCell> cell = localCellsPool.First;
        localCellsPool.RemoveFirst();

        if (scrollingPositive)
            cellsInUse.AddLast(cell);
        else
            cellsInUse.AddFirst(cell);
        return cell.Value;
    }

    void ResetContent()
    {
        preFirstVisibleIndex = 0;
        int outSideCount = cellsInUse.Count;
        while (outSideCount > 0)
        {
            outSideCount--;
            cellsInUse.Last.Value.Hidden();
            localCellsPool.AddLast(cellsInUse.Last.Value);
            cellsInUse.RemoveLast();
        }

        SetContentSize();
        int showCount;
        if (!isFullLoop)
            showCount = (visibleCellsTotalCount > cellDataList.Count) ? cellDataList.Count : visibleCellsTotalCount;
        else
            showCount = visibleCellsTotalCount;
        for (int i = 0; i < showCount; i++)
        {
            ShowCell(i, true);
        }
    }

    void SetContentSize()
    {
        int cellOneWayCount = Mathf.CeilToInt((float)cellDataList.Count / (float)numberOfColumns);
        float addOffset;
        if (vertical)
        {
            addOffset = cellOneWayCount > 1 ? cellOffset.y * (cellOneWayCount - 1) : 0;
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, cellOneWayCount * cellSize.y + addOffset);
        }
        else
        {
            addOffset = cellOneWayCount > 1 ? cellOffset.x * (cellOneWayCount - 1) : 0;
            scrollRect.content.sizeDelta = new Vector2(cellOneWayCount * cellSize.x + addOffset, scrollRect.content.sizeDelta.y);
        }
    }

    void InitData()
    {
        if (vertical)
        {
            visibleCellsRowCount = Mathf.CeilToInt(scrollRect.viewport.rect.height / (cellSize.y + cellOffset.y));
            if (adapationType == AdapationType.ModifyColumns)
                numberOfColumns = Mathf.FloorToInt(scrollRect.viewport.rect.width / (cellSize.x + cellOffset.x));
        }
        else
        {
            visibleCellsRowCount = Mathf.CeilToInt(scrollRect.viewport.rect.width / (cellSize.x + cellOffset.x));
            if (adapationType == AdapationType.ModifyColumns)
                numberOfColumns = Mathf.FloorToInt(scrollRect.viewport.rect.height / (cellSize.y + cellOffset.y));
        }
       
        visibleCellsTotalCount = (visibleCellsRowCount + 1) * numberOfColumns;
    }

    void SetCellsPool()
    {
        int outSideCount = localCellsPool.Count + cellsInUse.Count - visibleCellsTotalCount;
        if (outSideCount > 0)
        {
            while (outSideCount > 0)
            {
                outSideCount--;
                LinkedListNode<WrapCell> cell = localCellsPool.Last;
                localCellsPool.RemoveLast();
                Destroy(cell.Value.gameObject);
            }
        }
        else if (outSideCount < 0)
        {
            for (int i = 0; i < -outSideCount; i++)
            {
                WrapCell cell = Instantiate<WrapCell>(cellPrefab);
                localCellsPool.AddLast(cell);
                cell.transform.SetParent(scrollRect.content.transform, false);
                cell.Hidden();
            }
        }
    }
}
