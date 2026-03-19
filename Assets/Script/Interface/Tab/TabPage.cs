using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabPage : MonoBehaviour
{
    [SerializeField] private InfiniteScrollView _scrollView;
    public void SetData(List<InfiniteScrollData> dataList)
    {
        _scrollView.SetData(dataList);
    }
}
