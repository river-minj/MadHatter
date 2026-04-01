using Spine;
using System;
using System.Collections.Generic;
using UnityEngine;

public enum TabType
{
	Equipment,
	Consumable,
	Companion
}

[Serializable]
public class InventoryTab
{
	public Tab tab;
	public TabType dataSource;
	public InfiniteScrollView scrollView;
}

public class InventoryUI : MonoBehaviour
{

    [SerializeField] private GameObject _inventoryPanel;
	[SerializeField] private TabController _tabController;
	[SerializeField] private List<InventoryTab> _tabs;
	
	private Tab _currentTab;

	
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

	private void OnTabChanged(Tab tab)
	{
		_currentTab = tab;
		RefreshTab(tab);
	}

	private void RefreshCurrentTab()
	{
		if (_currentTab != null)
			RefreshTab(_currentTab);
	}

	private void RefreshTab(Tab tab)
	{
		var mapping = _tabs.Find(m => m.tab == tab);
		if (mapping == null)
			return;

		switch (mapping.dataSource)
		{
			case TabType.Equipment:
				var equipData = BuildItemData(ItemType.Equipment);
				Debug.Log($"[InventoryUI] 장비 데이터 수: {equipData.Count}");
				mapping.scrollView.SetData(equipData);
				break;
			case TabType.Consumable:
				var consumeData = BuildItemData(ItemType.Consumable);
				Debug.Log($"[InventoryUI] 소비 데이터 수: {consumeData.Count}");
				mapping.scrollView.SetData(consumeData);
				break;
			case TabType.Companion:
				var companionData = BuildCompanionData();
				Debug.Log($"[InventoryUI] 동료 데이터 수: {companionData.Count}");
				mapping.scrollView.SetData(companionData);
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
				_isEquipped = items[i].itemId == equippedId,
				_onClicked = (slot, equipped) => { ShowDetailPopup(slot, equipped); }
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

	public void ShowDetailPopup(InventorySlot slot, bool isEquipped)
	{

		var popup = UIManager.Instance.CreateItemDetailPopup();
		if (popup == null) return;

		Action actionCallback = () =>
		{
			if (slot.data._itemType == ItemType.Equipment)
			{
				if (isEquipped)
					InventoryManager.Instance.UnequipWeapon();
				else
					InventoryManager.Instance.EquipWeapon(slot.itemId);
			}
			else if (slot.data._itemType == ItemType.Consumable)
			{
				InventoryManager.Instance.UseItem(slot.itemId);
			}
		};

		popup.SetPopup(slot, isEquipped, actionCallback);
	}
}
