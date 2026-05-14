using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
	public static ShopManager Instance { get; private set; }

	// uniqueId → 남은 재고 (무한 재고 아이템은 저장하지 않음)
	private Dictionary<string, int> _stockMap = new Dictionary<string, int>();

	public Action OnShopStockChanged;

	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	public int GetStock(string uniqueId)
	{
		ShopItemData item = ShopDatabase.Instance.GetShopItemByUniqueId(uniqueId);
		if (item == null) return 0;

		if (item.initialStock == -1) return -1;

		if (_stockMap.TryGetValue(uniqueId, out int stock))
			return stock;

		return item.initialStock;
	}

	// 구매 시도. 성공 시 true 반환
	public bool TryBuy(string uniqueId)
	{
		ShopItemData item = ShopDatabase.Instance.GetShopItemByUniqueId(uniqueId);
		if (item == null) return false;

		int stock = GetStock(uniqueId);
		if (stock == 0)
		{
			Debug.Log($"[ShopManager] 품절: {uniqueId}");
			return false;
		}

		if (!PlayerInfoManager.Instance.SpendGold(item.price))
		{
			Debug.Log($"[ShopManager] 골드 부족: {item.price} 필요");
			return false;
		}

		InventoryManager.Instance.AddItem(item.itemData.itemId);

		if (stock != -1)
		{
			_stockMap[uniqueId] = stock - 1;
			OnShopStockChanged?.Invoke();
		}

		Debug.Log($"[ShopManager] 구매 완료: {item.itemData.itemId} (남은 재고: {GetStock(uniqueId)})");
		GameManager.Instance.SaveGame();
		return true;
	}

	public ShopSaveData GetSaveData()
	{
		var data = new ShopSaveData();
		foreach (var kv in _stockMap)
			data.stocks.Add(new ShopStockEntry { uniqueId = kv.Key, remaining = kv.Value });
		return data;
	}

	public void ApplyData(ShopSaveData data)
	{
		_stockMap.Clear();
		if (data == null) return;
		foreach (var entry in data.stocks)
			_stockMap[entry.uniqueId] = entry.remaining;
	}
}
