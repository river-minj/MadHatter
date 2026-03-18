using System.Collections.Generic;
using UnityEngine;


public class ItemDatabase
{
	public static ItemDatabase Instance { get; private set; }
	
	private Dictionary<string, ItemData> _dicItem = new Dictionary<string, ItemData>();

	public static void CreateInstance() { Instance = new ItemDatabase(); }

	public void ApplyData(List<ItemTableData> rowList)
	{
		_dicItem.Clear();

		foreach (ItemTableData item in rowList)
		{
			if (string.IsNullOrEmpty(item.itemId))
				continue;

			if(_dicItem.ContainsKey(item.itemId))
			{
				Debug.LogWarning($"[ItemDatabase] 중복 itemId: {item.itemId}");
				continue;
			}

			_dicItem[item.itemId] = new ItemData(item);
		}

		Debug.Log($"[ItemDatabase] 아이템 {_dicItem.Count}개 로드 완료");
	}

	public ItemData GetItemById(string itemId)
	{
		if (_dicItem.TryGetValue(itemId, out var data))
			return data;

		Debug.LogWarning($"[ItemDatabase] 아이템을 찾을 수 없습니다: {itemId}");
		return null;
	}
}
