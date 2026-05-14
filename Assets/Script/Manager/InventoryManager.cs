using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySlot
{
	public string itemId;
	public ItemData data;
	public int count;

	public InventorySlot(string itemId, int count)
	{
		this.itemId = itemId;
		this.data = ItemDatabase.Instance.GetItemById(itemId);
		this.count = count;
	}
}

public class InventoryManager : MonoBehaviour
{
	public static InventoryManager Instance {  get; private set; }

	private Dictionary<string, InventorySlot> _dicInventory = new Dictionary<string, InventorySlot>();

	private string _equippedWeaponId;

	private bool _isLoading = false;

	public event Action OnInventoryChanged;
	public event Action OnEquipChanged;

	public void Awake()
	{
		if(Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	//아이템 추가 / 제거
	public void AddItem(string itemId, int count = 1)
	{
		if (string.IsNullOrEmpty(itemId) || count <= 0) return;

		if (_dicInventory.TryGetValue(itemId, out var slot))
		{
			slot.count += count;
		}
		else
		{
			_dicInventory[itemId] = new InventorySlot(itemId, count);
		}
		Debug.Log($"[Inventory] 아이템 획득: {itemId} x{count}, 보유: {_dicInventory[itemId].count}개");

		if (!_isLoading)
			QuestManager.Instance.ReportCollect(itemId, count);

		OnInventoryChanged?.Invoke();
	}

	public void RemoveItem(string itemId, int count = 1)
	{
		if (!_dicInventory.TryGetValue(itemId, out var slot)) return;

		slot.count -= count;
		if (slot.count <= 0)
		{
			_dicInventory.Remove(itemId);

			// 장착 중인 장비가 제거되면 장착 해제
			if (_equippedWeaponId == itemId)
				UnequipWeapon();
		}

		OnInventoryChanged?.Invoke();
	}

	public void UseItem(string itemId)
	{
		if (!_dicInventory.TryGetValue(itemId, out var slot)) return;
		if (slot.data == null || slot.data._itemType != ItemType.Consumable) return;

		PlayerInfoManager.Instance.AddHp(slot.data._effectValue);
		RemoveItem(itemId, 1);
	}


	// 장비 착용 / 해제
	public void EquipWeapon(string itemId)
	{
		if (!_dicInventory.TryGetValue(itemId, out var slot)) return;
		if (slot.data == null || slot.data._itemType != ItemType.Equipment) return;

		_equippedWeaponId = itemId;
		OnEquipChanged?.Invoke();
	}

	public void UnequipWeapon()
	{
		_equippedWeaponId = null;
		OnEquipChanged?.Invoke();
	}

	public string GetEquippedWeaponId()
	{
		return _equippedWeaponId;
	}

	public ItemData GetEquippedWeaponData()
	{
		if (string.IsNullOrEmpty(_equippedWeaponId))
			return null;

		return ItemDatabase.Instance.GetItemById(_equippedWeaponId);
	}

	// UI용 조회
	public Dictionary<string, InventorySlot> GetAllItems()
	{
		return _dicInventory;
	}

	public List<InventorySlot> GetItemsByType(ItemType type)
	{
		List<InventorySlot> result = new List<InventorySlot>();
		foreach (var slot in _dicInventory.Values)
		{
			if (slot.data != null && slot.data._itemType == type)
				result.Add(slot);
		}
		return result;
	}

	public bool HasItem(string itemId)
	{
		return _dicInventory.ContainsKey(itemId);
	}

	// --- 세이브/로드 ---

	public InventorySaveData GetSaveData()
	{
		var saveData = new InventorySaveData();
		saveData.equippedWeaponId = _equippedWeaponId;
		saveData.items = new List<InventoryItemEntry>();

		foreach (var pair in _dicInventory)
		{
			saveData.items.Add(new InventoryItemEntry
			{
				itemId = pair.Key,
				count = pair.Value.count
			});
		}

		return saveData;
	}

	public void ApplyData(InventorySaveData data)
	{
		_isLoading = true;

		_dicInventory.Clear();
		_equippedWeaponId = null;

		if (data == null)
		{
			_isLoading = false;
			return;
		}

		if (data.items != null)
		{
			foreach (var entry in data.items)
			{
				if (!string.IsNullOrEmpty(entry.itemId) && entry.count > 0)
					_dicInventory[entry.itemId] = new InventorySlot(entry.itemId, entry.count);
			}
		}

		_equippedWeaponId = data.equippedWeaponId;

		_isLoading = false;

		OnInventoryChanged?.Invoke();
		OnEquipChanged?.Invoke();
	}
}
