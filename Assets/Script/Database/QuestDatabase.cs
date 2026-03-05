using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase
{
	public static QuestDatabase Instance { get; private set; }

	private Dictionary<string, QuestData> _questMap = new Dictionary<string, QuestData>();

	public static void CreateInstance()
	{
		Instance = new QuestDatabase();
	}

	public void ApplyData(List<QuestTableData> questList, List<RewardTableData> rewardList)
	{
		_questMap.Clear();

		// 1) 보상을 rewardGroupId로 그룹핑
		var rewardGroupMap = new Dictionary<string, List<QuestReward>>();

		foreach (var row in rewardList)
		{
			if (string.IsNullOrEmpty(row.uniqueId) || string.IsNullOrEmpty(row.rewardGroupId))
				continue;

			if (!rewardGroupMap.TryGetValue(row.rewardGroupId, out var rewardGroup))
			{
				rewardGroup = new List<QuestReward>();
				rewardGroupMap.Add(row.rewardGroupId, rewardGroup);
			}

			rewardGroup.Add(new QuestReward
			{
				_gold = row.gold,
				_exp = row.exp,
				_companionId = row.companionId,
				_itemId = row.itemId,
				_itemCount = row.itemCount
			});
		}

		// 2) 퀘스트 데이터 생성 + 보상 매칭
		foreach (var row in questList)
		{
			if (string.IsNullOrEmpty(row.uniqueId))
				continue;

			if (_questMap.ContainsKey(row.uniqueId))
			{
				Debug.LogWarning($"[QuestDatabase] 중복 Quest ID: {row.uniqueId}");
				continue;
			}

			var questData = new QuestData
			{
				_questId = row.uniqueId,
				_questGiverNpcId = row.questGiverNpcId,
				_title = row.title,
				_description = row.description,
				_startDialogueId = row.startDialogueId,
				_progressDialogueId = row.progressDialogueId,
				_completedDialogueId = row.completedDialogueId,
				_questCompleterNpcId = row.questCompleterNpcId,
				_goalType = row.goalType,
				_goalCount = row.goalCount,
				_rewardGroupId = row.rewardGroupId,
				_nextQuestId = row.nextQuestId
			};

			// 보상 매칭
			if (!string.IsNullOrEmpty(row.rewardGroupId) &&
				rewardGroupMap.TryGetValue(row.rewardGroupId, out var rewards))
			{
				questData._rewards = rewards;
			}

			_questMap.Add(row.uniqueId, questData);
		}
	}

	public QuestData GetQuestById(string questId)
	{
		if (string.IsNullOrEmpty(questId))
			return null;

		if (_questMap.TryGetValue(questId, out var questData))
			return questData;

		return null;
	}

	public IEnumerable<QuestData> GetAllQuests()
	{
		return _questMap.Values;
	}
}