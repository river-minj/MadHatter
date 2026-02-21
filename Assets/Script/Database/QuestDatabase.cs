using System.Collections.Generic;
using UnityEngine;

public class QuestDatabase : MonoBehaviour
{
	[SerializeField]private List<QuestData> _questList = new List<QuestData>();
	private Dictionary<string, QuestData> _questMap = new Dictionary<string, QuestData>();

	public static QuestDatabase Instance { get; private set; }

	private void Awake()
	{
		Instance = this;


		BuildMap();
	}

	private void BuildMap()
	{
		_questMap = new Dictionary<string, QuestData>();

		foreach(var q in _questList)
		{
			if(q == null || string.IsNullOrEmpty(q._questId))
			{
				continue;
			}

			if(_questMap.ContainsKey(q._questId))
			{
				Debug.LogWarningFormat("[QuestDatabase] Duplicate Quest ID: {0}", q._questId);
				continue;
			}
			
			_questMap.Add(q._questId, q);
		}
	}

	public QuestData GetQuestByID(string questId)
	{
		if(string.IsNullOrEmpty(questId))
		{
			return null;
		}

		if(_questMap.TryGetValue(questId, out var questData))
		{
			return questData;
		}

		return null;
	}

	public IEnumerable<QuestData> GetAllQuests()
	{
		return _questList;
	}

	public QuestData GetQuestData(string questID)
	{
		if (string.IsNullOrEmpty(questID))
		{
			Debug.LogWarning("[QuestDatabase] GetQuestData called with null or empty questID.");
			return null;
		}

		if (_questMap.TryGetValue(questID, out var questData))
		{
			return questData;
		}

		Debug.LogWarningFormat("[QuestDatabase] Quest ID not found: {0}", questID);
		return null;

	}
}
