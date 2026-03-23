using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropDatabase : MonoBehaviour
{
	public static DropDatabase Instance { get; private set; }

	private Dictionary<string, List<DropData>> _dicDrop = new Dictionary<string, List<DropData>>();

	public static void CreateInstance()
	{
		Instance = new DropDatabase();
	}

	public void ApplyData(List<DropTableData> rowList)
	{
		_dicDrop.Clear();

		foreach(var raw in rowList)
		{
			if (string.IsNullOrEmpty(raw.enemyId))
				continue;

			if (!_dicDrop.ContainsKey(raw.enemyId))
			{
				_dicDrop[raw.enemyId] = new List<DropData>();
			}

			_dicDrop[raw.enemyId].Add(new DropData(raw));
		}

		Debug.Log($"[DropDatabase] {_dicDrop.Count}개 적의 드롭 테이블 로드 완료");
	}

	//drop계산
	public string RollDrop(string enemyId)
	{
		if (!_dicDrop.TryGetValue(enemyId, out var dropList)) 
			return null;
		if (dropList.Count == 0) 
			return null;

		int totalWeight = 0;
		foreach (var drop in dropList)
		{
			totalWeight += drop._weight;
		}

		int roll = Random.Range(0, totalWeight);
		int cumulative = 0;

		foreach (var drop in dropList)
		{
			cumulative += drop._weight;
			if (roll < cumulative)
			{
				return string.IsNullOrEmpty(drop._itemId) ? null : drop._itemId;
			}
		}

		return null;
	}
}
