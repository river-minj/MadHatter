using System.Collections.Generic;
using UnityEngine;

public class NpcDatabase
{
	public static NpcDatabase Instance { get; private set; }

	private Dictionary<string, NpcData> _dicNpc = new Dictionary<string, NpcData>();

	public static void CreateInstance()
	{
		Instance = new NpcDatabase();
	}


	public void ApplyData(List<NpcTableData> tableDataList)
	{
		_dicNpc.Clear();

		foreach (var row in tableDataList)
		{
			if (string.IsNullOrEmpty(row.uniqueId))
				continue;

			if (_dicNpc.ContainsKey(row.uniqueId))
			{
				Debug.LogWarning($"[NpcDatabase] 중복 NPC ID: {row.uniqueId}");
				continue;
			}

			var data = new NpcData
			{
				_npcId = row.uniqueId,
				_npcName = row.npcName,
				_defaultDialogueId = row.defaultDialogueId,
				_questId = row.questId
			};

			_dicNpc.Add(row.uniqueId, data);
		}
	}

	public NpcData GetNpcById(string npcId)
	{
		if (_dicNpc.TryGetValue(npcId, out NpcData npcData))
		{
			return npcData;
		}

		Debug.LogWarning($"[NpcDatabase] NPC ID not found: {npcId}");
		return null;
	}
}