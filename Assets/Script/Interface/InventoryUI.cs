using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{

    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _companioanSlot;

	[SerializeField] private InfiniteScrollView _scrollView;

	private void Awake()
	{
        Hide();
	}

    public void Show()
    {
		_inventoryPanel.SetActive(true);
		RefreshUI();
	}

	public void Hide()
	{
		_inventoryPanel.SetActive(false);
	}

	public void Toggle()
	{
		if(_inventoryPanel.activeSelf)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void RefreshUI()
	{
		//무한 스크롤 뷰에 데이터 설정
		List<InfiniteScrollData> list = new List<InfiniteScrollData>();
		foreach(var companion in CompanionManager.Instance.OwnedCompanions)
		{
			CompanionSlotData data = new CompanionSlotData();
			data._companionData = companion;
			list.Add(data);
		}

		_scrollView.SetData(list);
		Debug.Log($"인벤토리 UI 새로고침: {list.Count}개의 동료 슬롯 생성");

	}
}
