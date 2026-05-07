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
	private CameraController _camCtrl;
	private Transform _playerTransform;
	private float _visibleCenterOffset;

	private void Awake()
	{
        Hide();
	}

	private void Update()
	{
		if (!_inventoryPanel.activeSelf) return;
		if (_camCtrl == null || _playerTransform == null) return;

		float clampedBaseY = _camCtrl.GetClampedTargetY();
		float worldOffsetY = (_playerTransform.position.y - _visibleCenterOffset) - clampedBaseY;
		_camCtrl.SetUIOffset(new Vector3(0f, worldOffsetY, 0f));
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

		//인벤이 가리지 않는 영역 중앙으로 카메라 이동
		AdjustCamera(true);
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

		//카메라 원상 복귀
		AdjustCamera(false);
	}

	private void AdjustCamera(bool open)
	{
		var cam = Camera.main;
		_camCtrl = cam?.GetComponent<CameraController>();
		if (_camCtrl == null) return;

		if (!open)
		{
			_camCtrl.ClearUIOffset();
			_playerTransform = null;
			return;
		}

		// 가시영역 중앙 뷰포트 → 월드 오프셋 계산 (패널 크기 기준, 한 번만)
		var canvas = _inventoryPanel.GetComponentInParent<Canvas>().rootCanvas;
		Vector3[] corners = new Vector3[4];
		_inventoryPanel.GetComponent<RectTransform>().GetWorldCorners(corners);
		float panelTopViewport = (corners[1].y * canvas.scaleFactor) / Screen.height;
		float visibleCenterViewport = (panelTopViewport + 1f) * 0.5f;
		_visibleCenterOffset = (visibleCenterViewport - 0.5f) * 2f * cam.orthographicSize;

		var playerGO = GameObject.FindGameObjectWithTag("Player");
		if (playerGO == null) return;
		_playerTransform = playerGO.transform;
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
