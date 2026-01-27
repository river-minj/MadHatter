using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//독립적인 데이터 구조이며 여러 시스템에서 참조될 가능성이 높음
public class QuestState
{
	public QuestData _data;
	public int _currentProgress;
	public bool _isCompleted;

	public QuestState(QuestData data)
	{
		_data = data; 
		_currentProgress = 0;
		_isCompleted = false;
	}

	public bool AddProgress(int amount =1)
	{
		if(_isCompleted)
		{
			return false;
		}

		_currentProgress += amount;
		if(_currentProgress >= _data._goalCount)
		{
			_isCompleted = true;
			_currentProgress = _data._goalCount;
			return true;
		}

		return false;
	}
}

public enum QuestGoalType
{
	None,
	Kill,
	Collect,
	Talk,
	Explore,
	AcquireItem,
}

[System.Serializable]
public class QuestReward
{
	public string _companionID;
	public int _gold;
	public int _exp;
	public string _itemID;
}

/// <summary>
///	퀘스트 매니저는 전역 상태이기 때문에 싱글톤 패턴으로 구현
///	맵 전환, 대사 변경 등의 상태에서도 퀘스트 상태가 유지되어야 하기 때문
///	여러 시스템이 접근 해야하기 때문에 싱글톤 패턴이 적합
/// </summary>
public class QuestManager : MonoBehaviour
{

	public static QuestManager Instance { get; private set; }

	private HashSet<string> _setStartedQuest = new HashSet<string>(); //시작한 퀘스트ID
	private HashSet<string> _setCompletedQuest = new HashSet<string>(); //완료된 퀘스트ID
	private Dictionary<string, QuestState> _dicActiveQuest = new Dictionary<string, QuestState>();

	private void Awake()
	{
		if(Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
	}

	//npc와 상호작용 처리
	public void TryQuestInteraction(string questID)
	{
		var questData = QuestDatabase.Instance.GetQuestByID(questID);
		if(questData == null)
		{
			Debug.LogWarning($"[QuestManager] 유효하지 않은 퀘스트ID: {questID}");
			return;
		}

		//새로 퀘스트 시작
		if(_setStartedQuest.Contains(questID) == false)
		{
			_setStartedQuest.Add(questID);
			_dicActiveQuest.Add(questID, new QuestState(questData));

			DialogueData d = DialogueDatabase.Instance.GetDialogueByID(questData._startDialogueID);
            if (d != null)
            {
				GameManager.Instance.StartDialogue(d);
            }
			return;
        }

		//퀘스트 진행 중
		if(_setCompletedQuest.Contains(questID) == false)
		{
			DialogueData d = DialogueDatabase.Instance.GetDialogueByID(questData._progressDialogueID);
			if (d != null)
			{
				GameManager.Instance.StartDialogue(d);
			}
			return;
		}

		//퀘스트 완료
		DialogueData completedDialogue = DialogueDatabase.Instance.GetDialogueByID(questData._completedDialogueID);
		if (completedDialogue != null)
		{
			GameManager.Instance.StartDialogue(completedDialogue, () => { CompleteQuset(questData); });
		}
			
    }

	//퀘스트 시작
	public void StartQuest(QuestData questData )
	{
		if(questData == null)
		{
			return;
		}

		string questID = questData._questID;
		if(_setCompletedQuest.Contains(questID))
		{
			Debug.LogWarning($"[QuestManager] 이미 완료된 퀘스트입니다. {questID}");
			return;
		}

		if(_setStartedQuest.Contains(questID))
		{
			Debug.LogWarning($"[QuestManager] 이미 진행중인 퀘스트입니다. {questID}");
			return;
		}

		_setStartedQuest.Add(questID);
		Debug.Log("[QuestManager] 퀘스트 시작");
	}

	//퀘스트 완료 처리
	public void CompleteQuset(QuestData questData )
	{
		if(questData == null)
		{
			return;
		}

		if (!_setStartedQuest.Contains(questData._questID))
		{
			Debug.LogWarning($"[QuestManager] 진행중인 퀘스트가 아닙니다. {questData._questID}");
			return;
		}

		_setCompletedQuest.Add(questData._questID);

		Debug.Log("[QuestManager] 퀘스트 완료");
	}

	public bool IsQuestCompleted(string questId)
	{
		return _setCompletedQuest.Contains(questId);
	}

	public void ReportTalktoNPC(string npcID)
	{
		foreach(var questID in _setStartedQuest)
		{
			QuestState qs = _dicActiveQuest[questID];
			if (qs != null && qs._data._goalType == QuestGoalType.Talk && qs._data._npcID == npcID)
			{
				bool completed = qs.AddProgress();
                if (completed)
                {
					OnQuestCompleted(qs);
                }
            }
		}
	}
	public void ReportKill(string monsterID)
	{
		foreach (var questID in _setStartedQuest)
		{
			QuestState qs = _dicActiveQuest[questID];
			if (qs != null && qs._data._goalType == QuestGoalType.Kill)
			{
				bool completed = qs.AddProgress();

				if (completed)
				{
					OnQuestCompleted(qs);
				}
			}
		}
	}

	public void ReportReach(string locationID)
	{
		foreach (var questID in _setStartedQuest)
		{
			QuestState qs = _dicActiveQuest[questID];
			if (qs._data._goalType == QuestGoalType.Explore && qs._data._npcID == locationID)
			{
				bool completed = qs.AddProgress();

				if (completed)
				{
					OnQuestCompleted(qs);
				}
			}
		}
	}

	private void OnQuestCompleted(QuestState questState)
	{
		_setCompletedQuest.Add(questState._data._questID);

		QuestReward reward = questState._data._reward;

		if(string.IsNullOrEmpty(reward._companionID) == false)
		{
			Debug.Log($"[QuestManager] 동료 획득: {reward._companionID}");
		}

		if(reward._gold > 0)
		{
			Debug.Log($"[QuestManager] 골드 획득: {reward._gold}");
		}

		if(reward._exp > 0)
		{
			Debug.Log($"[QuestManager] 경험치 획득: {reward._exp}");
		}

		if(string.IsNullOrEmpty(questState._data._nextQuestID) == false)
		{
			QuestData nextQuestData = QuestDatabase.Instance.GetQuestByID(questState._data._nextQuestID);
			StartQuest(nextQuestData);
		}
	}
}
