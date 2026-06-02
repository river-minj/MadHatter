using System.Collections.Generic;
using UnityEngine;

public class CompanionDatabase
{
	public static CompanionDatabase Instance { get; private set; }

	[SerializeField] private List<CompanionData> _companionList = new List<CompanionData>();

	private Dictionary<string, CompanionData> _dicCompanion = new Dictionary<string, CompanionData>();

	public static void CreateInstance()
	{
		Instance = new CompanionDatabase();
	}

	public void ApplyData(List<CompanionTableData> tableDataList)
	{
		_dicCompanion.Clear();

		foreach (var row in tableDataList)
		{
			if (string.IsNullOrEmpty(row.uniqueId))
				continue;

			if (_dicCompanion.ContainsKey(row.uniqueId))
			{
				Debug.LogWarning($"[CompanionDatabase] 중복 Companion ID: {row.uniqueId}");
				continue;
			}

			var data = new CompanionData
			{
				_companionId = row.uniqueId,
				_companionName = row.companionName,
				_skinName = row.skinName,
				_companionPrefabPath = row.companionPrefabPath,
				_followSpeed = row.followSpeed,
				_followDistance = row.followDistance
			};

			_dicCompanion.Add(row.uniqueId, data);
		}
	}

	public CompanionData GetCompanionById(string companionID)
	{
		if (_dicCompanion.TryGetValue(companionID, out CompanionData companionData))
		{
			return companionData;
		}

		Debug.LogWarningFormat("[CompanionDatabase] Companion ID not found: {0}", companionID);
		return null;
	}

	public IEnumerable<CompanionData> GetAllCompanions()
	{
		return _dicCompanion.Values;
	}
}
