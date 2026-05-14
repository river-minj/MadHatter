using System.Collections.Generic;
using UnityEngine;

public class ShopItemData
{
	public string uniqueId;
	public string shopId;
	public ItemData itemData;
	public int price;
	public int initialStock; // -1 = 무한
}

public class ShopDatabase
{
	public static ShopDatabase Instance { get; private set; }

	private Dictionary<string, List<ShopItemData>> _dicShop = new Dictionary<string, List<ShopItemData>>();
	private Dictionary<string, ShopItemData> _dicShopItem = new Dictionary<string, ShopItemData>();

	public static void CreateInstance() { Instance = new ShopDatabase(); }

	public void ApplyData(List<ShopTableData> rowList)
	{
		_dicShop.Clear();
		_dicShopItem.Clear();

		foreach (var row in rowList)
		{
			if (string.IsNullOrEmpty(row.uniqueId)) continue;

			ShopItemData data = new ShopItemData
			{
				uniqueId = row.uniqueId,
				shopId = row.shopId,
				itemData = ItemDatabase.Instance.GetItemById(row.itemId),
				price = row.price,
				initialStock = row.sellCount
			};

			if (!_dicShop.ContainsKey(row.shopId))
				_dicShop[row.shopId] = new List<ShopItemData>();

			_dicShop[row.shopId].Add(data);
			_dicShopItem[row.uniqueId] = data;
		}

		Debug.Log($"[ShopDatabase] {_dicShopItem.Count}개 상품 로드 완료");
	}

	public List<ShopItemData> GetShopItems(string shopId)
	{
		if (_dicShop.TryGetValue(shopId, out var list))
			return list;

		Debug.LogWarning($"[ShopDatabase] 상점을 찾을 수 없습니다: {shopId}");
		return null;
	}

	public ShopItemData GetShopItemByUniqueId(string uniqueId)
	{
		if (_dicShopItem.TryGetValue(uniqueId, out var data))
			return data;

		Debug.LogWarning($"[ShopDatabase] 상품을 찾을 수 없습니다: {uniqueId}");
		return null;
	}
}
