using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class InventoryTabMapping
{
	public GameObject tabPage;
	public TabDataSource dataSource;
}

public enum TabDataSource
{
	Equipment,
	Consumable,
	Companion
}

public class InventoryUI : MonoBehaviour
{

    [SerializeField] private GameObject _inventoryPanel;
	[SerializeField] private TabController _tabController;


	[SerializeField] private List<TabEntry> _tabs;
    
	//[SerializeField] private Transform _content;
 //   [SerializeField] private GameObject _companioanSlot;
	//[SerializeField] private InfiniteScrollView _scrollView;

	[SerializeField] private GameObject _detailPopup;

	private TabEntry _currentTab;

	
	[SerializeField] private List<InventoryTabMapping> _tabMappings;
	private void Awake()
	{
        Hide();
	}

	public void Show()
    {
		//인벤 열기
		_inventoryPanel.SetActive(true);

		//탭 변경 이벤트
		_tabController.OnTabChanged += OnTabChanged;

		//인벤 아이템 변경 이벤트
		InventoryManager.Instance.OnInventoryChanged += RefreshCurrentTab;
		InventoryManager.Instance.OnEquipChanged += RefreshCurrentTab;

		//디폴트 탭
		_tabController.SetDefaultTab();
	}

	public void Hide()
	{
		//탭 변경 이벤트 제거
		_tabController.OnTabChanged -= OnTabChanged;

		//인벤 아이템 변경 이벤트 제거
		if (InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnInventoryChanged -= RefreshCurrentTab;
			InventoryManager.Instance.OnEquipChanged -= RefreshCurrentTab;
		}

		_currentTab = null;
		//인벤 닫기
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

	private void ChangeTab(TabEntry selectedTab)
	{
		foreach(var tab in _tabs) {
			tab.tabPage.SetActive(tab == selectedTab);
		}
	}

	private void OnTabChanged(TabEntry tab)
	{
		_currentTab = tab;
		//_detailPopup.Hide();
		RefreshTab(tab);
	}

	private void RefreshCurrentTab()
	{
		if (_currentTab != null)
			RefreshTab(_currentTab);
	}

	private void RefreshTab(TabEntry tab)
	{
		var tabPage = tab.tabPage.GetComponent<TabPage>();
		if (tabPage == null)
			return;

		var mapping = _tabMappings.Find(m => m.tabPage == tab.tabPage);
		if (mapping == null)
			return;

		switch (mapping.dataSource)
		{
			case TabDataSource.Equipment:
				tabPage.SetData(BuildItemData(ItemType.Equipment));
				break;
			case TabDataSource.Consumable:
				tabPage.SetData(BuildItemData(ItemType.Consumable));
				break;
			case TabDataSource.Companion:
				tabPage.SetData(BuildCompanionData());
				break;
		}
	}

	//아이템 리스트 생성
	private List<InfiniteScrollData> BuildItemData(ItemType type)
	{
		List<InventorySlot> items = InventoryManager.Instance.GetItemsByType(type);
		string equippedId = InventoryManager.Instance.GetEquippedWeaponId();

		List<InfiniteScrollData> dataList = new List<InfiniteScrollData>();
		for (int i = 0; i < items.Count; i++)
		{
			dataList.Add(new ItemSlotData
			{
				_index = i,
				_inventorySlot = items[i],
				_isEquipped = items[i].itemId == equippedId
			});
		}
		return dataList;
	}

	//탭페이지 리스트뷰 데이터에 따른 데이터리스트 생성 - 동료 리스트 생성
	private List<InfiniteScrollData> BuildCompanionData()
	{
		List<InfiniteScrollData> dataList = new List<InfiniteScrollData>();
		int index = 0;
		foreach (var companion in CompanionManager.Instance.OwnedCompanions)
		{
			dataList.Add(new CompanionSlotData
			{
				_index = index++,
				_companionData = companion
			});
		}
		return dataList;
	}

	//to do : 여기다 할지 말지
	// --- 상세 팝업 ---

	public void ShowDetailPopup(InventorySlot slot, bool isEquipped)
	{
		//_detailPopup.Show(slot, isEquipped);
	}
}
