using System;
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

[System.Serializable]
public class QuestReward
{
	public string _companionId; //무조건 1명만 보상에 넣는다
	
	public int _gold;

	public int _exp;

	public string _itemId;
	public int _itemCount;
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

	public Dictionary<string, QuestState> DicActiveQuest => _dicActiveQuest;

	private const int MaxQuestCount = 5;

	//이벤트
	public Action OnQuestListChanged;
	public Action<QuestState> OnQuestProgressUpdate;
	public Action<string> OnQuestRewardClaimed;

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

	public QuestSaveData GetSaveData()
	{
		QuestSaveData data = new QuestSaveData
		{
			startedQuests = new List<string>(_setStartedQuest),
			completedQuests = new List<string>(_setCompletedQuest),
		};

		foreach (var kv in _dicActiveQuest)
		{
			QuestState qs = kv.Value;

			data.activeQuests.Add(new ActiveQuestEntry
			{
				questID = kv.Key,
				currentProgress = qs._currentProgress,
				isCompleted = qs._isCompleted
			});
		}

		return data;
	}

	public void ApplyData(QuestSaveData data)
	{
		if (data == null) return;

		_setStartedQuest = data.startedQuests != null
			? new HashSet<string>(data.startedQuests)
			: new HashSet<string>();
		_setCompletedQuest = data.completedQuests != null
			? new HashSet<string>(data.completedQuests)
			: new HashSet<string>();
		_dicActiveQuest.Clear();
		foreach (var entry in data.activeQuests)
		{
			QuestData questData = QuestDatabase.Instance.GetQuestById(entry.questID);
			if (questData == null)
				continue;

			QuestState qs = new QuestState(questData)
			{
				_currentProgress = entry.currentProgress,
				_isCompleted = entry.isCompleted
			};

			_dicActiveQuest.Add(entry.questID, qs);
		}

		//적용 
		OnQuestListChanged?.Invoke();
	}

	//npc와 상호작용 처리
	public void TryQuestStart(string questID)
	{
		var questData = QuestDatabase.Instance.GetQuestById(questID);
		if(questData == null)
		{
			Debug.LogWarning($"[QuestManager] 유효하지 않은 퀘스트ID: {questID}");
			return;
		}

		//완료된 퀘스트
		if(_setCompletedQuest.Contains(questID))
		{
			DialogueData completedD = DialogueDatabase.Instance.GetDialogueById(questData._completedDialogueId);
			if (completedD != null)
			{
				GameManager.Instance.StartDialogue(completedD);
			}
			return;
		}

		//진행중인 퀘스트
		if(_dicActiveQuest.TryGetValue(questID, out QuestState qs))
		{
			if (qs._isCompleted)
			{
				// 목표 달성 → completedDialogue 재생 후 보상 지급
				DialogueData completedD = DialogueDatabase.Instance.GetDialogueById(questData._completedDialogueId);
				if (completedD != null)
				{
					GameManager.Instance.StartDialogue(completedD, () =>
					{
						ClaimReward(questID);
						GameManager.Instance.EndDialogue();
					});
				}
				else
				{
					ClaimReward(questID);
				}
			}
			else
			{
				DialogueData progressD = DialogueDatabase.Instance.GetDialogueById(questData._progressDialogueId);
				if (progressD != null)
				{
					GameManager.Instance.StartDialogue(progressD);
				}
			}
			return;
		}

		//새 퀘스트
		if (_dicActiveQuest.Count >= MaxQuestCount)
		{
			Debug.LogWarning("[QuestManager] 최대 퀘스트 수에 도달했습니다.");
			return;
		}

		//새로 퀘스트 시작
		DialogueData startD = DialogueDatabase.Instance.GetDialogueById(questData._startDialogueId);
		if (startD != null)
		{
			GameManager.Instance.StartDialogue(startD, () =>
			{
				ShowQuestAcceptPopup(questData);
			});
		}
		else
		{
			ShowQuestAcceptPopup(questData);
		}
			
    }

	private void ShowQuestAcceptPopup(QuestData questData)
	{
		UIManager.Instance.ShowConfirmPopup("CommonConfirmPopup", questData._description, "수락", "거절", CommonConfirmPopup.ConfirmType.OKCancel, () =>
		{
			//수락 선택시
			StartQuest(questData);
			GameManager.Instance.EndDialogue();
		}, () =>
		{
			//거절 선택시
			GameManager.Instance.EndDialogue();
			Debug.Log($"[QuestManager] 퀘스트 거절: {questData._questId}");
		});
	}	

	private void StartQuest(QuestData questData)
	{
		if (questData == null)
			return;

		if (_setStartedQuest.Contains(questData._questId))
			return;

		_setStartedQuest.Add(questData._questId);
		_dicActiveQuest.Add(questData._questId, new QuestState(questData));

		Debug.Log($"[QuestManager] 퀘스트 시작: {questData._questId} - {questData._title}");

		OnQuestListChanged?.Invoke();
		GameManager.Instance.SaveGame();
	}	


	public bool IsQuestCompleted(string questId)
	{
		return _setCompletedQuest.Contains(questId);
	}

	public void ReportTalktoNPC(string npcId)
	{
		foreach(var quest in _dicActiveQuest)
		{
			QuestState qs = quest.Value;
			if (qs == null)
				continue;

			if (qs._isCompleted == true)
				continue;

			if (qs._data._targetId != npcId)
				continue;

			if (qs._data._goalType != QuestGoalType.Talk)
				continue;

			bool completed = qs.AddProgress();

			OnQuestProgressUpdate?.Invoke(qs);
	
			Debug.Log($"[QuestManager] ReportTalktoNPC: {npcId}, Progress: {qs._currentProgress}/{qs._data._goalCount}");

			if (completed)
			{
				OnQuestListChanged?.Invoke();
			}
		}
	}

	public void ReportKill(string enemyId)
	{

		foreach (var quest in _dicActiveQuest)
		{
			QuestState qs = quest.Value;
			if (qs == null || qs._isCompleted)
				continue;

			if (qs._data._goalType != QuestGoalType.Kill)
				continue;

			if (qs._data._targetId != enemyId)
				continue;

			bool completed = qs.AddProgress();
			OnQuestProgressUpdate?.Invoke(qs);

			Debug.Log($"[QuestManager] ReportKill: {enemyId}, Progress: {qs._currentProgress}/{qs._data._goalCount}");

			if (completed)
			{
				OnQuestListChanged?.Invoke();
			}
		}

	}

	public void ReportCollect(string itemId, int count = 1)
	{
		foreach (var quest in _dicActiveQuest)
		{
			QuestState qs = quest.Value;
			if (qs == null || qs._isCompleted)
				continue;

			if (qs._data._goalType != QuestGoalType.Collect)
				continue;

			if (qs._data._targetId != itemId)
				continue;

			bool completed = qs.AddProgress(count);
			OnQuestProgressUpdate?.Invoke(qs);

			Debug.Log($"[QuestManager] ReportCollect: {itemId} x{count}, Progress: {qs._currentProgress}/{qs._data._goalCount}");

			if (completed)
			{
				OnQuestListChanged?.Invoke();
			}
		}
	}

	public void ReportReach(string locationId)
	{
		var autoComplete = new System.Collections.Generic.List<string>();

		foreach (var quest in _dicActiveQuest)
		{
			QuestState qs = quest.Value;
			if (qs == null || qs._isCompleted)
				continue;
			if (qs._data._targetId != locationId)
				continue;
			if (qs._data._goalType != QuestGoalType.Explore)
				continue;

			bool completed = qs.AddProgress();
			OnQuestProgressUpdate?.Invoke(qs);

			Debug.Log($"[QuestManager] ReportReach: {locationId}, Progress: {qs._currentProgress}/{qs._data._goalCount}");

			if (completed)
			{
				OnQuestListChanged?.Invoke();
				autoComplete.Add(quest.Key);
			}
		}

		// Explore 퀘스트는 도달 즉시 자동 완료 — NPC 귀환 불필요
		foreach (var questId in autoComplete)
			ClaimReward(questId);
	}

	public void ClaimReward(string questId)
	{
		//진행 중인 퀘스트 여부
		if (_dicActiveQuest.TryGetValue(questId, out QuestState qs) == false)
			return;

		//퀘스트 달성 여부
		if (qs._isCompleted == false)
		{
			Debug.LogWarning($"[QuestManager] 퀘스트가 완료되지 않았습니다: {questId}");
			return;
		}

		var questData = qs._data;
		if (questData == null)
		{
			Debug.LogWarning($"[QuestManager] 유효하지 않은 퀘스트ID: {questId}");
			return;
		}

		GetReward(questData);

		// 보상 요약 토스트
		int totalGold = 0, totalExp = 0;
		foreach (var r in questData._rewards) { totalGold += r._gold; totalExp += r._exp; }
		var parts = new System.Collections.Generic.List<string>();
		if (totalGold > 0) parts.Add($"골드 +{totalGold}");
		if (totalExp  > 0) parts.Add($"EXP +{totalExp}");
		string summary = parts.Count > 0 ? $" ({string.Join(" / ", parts)})" : "";
		UIManager.Instance?.ShowToast($"[퀘스트 완료] {questData._title}{summary}");

		//퀘스트 리스트에서 제거
		_dicActiveQuest.Remove(questId);
		_setCompletedQuest.Add(questId);

		qs._isCompleted = true;

		OnQuestRewardClaimed?.Invoke(questId);

		// 모든 퀘스트 완료 시 엔딩 팝업
		bool allCleared = true;
		foreach (var q in QuestDatabase.Instance.GetAllQuests())
		{
			if (!_setCompletedQuest.Contains(q._questId)) { allCleared = false; break; }
		}
		if (allCleared)
		{
			UIManager.Instance?.ShowConfirmPopup(
				"CommonConfirmPopup",
				"퀘스트를 전부 달성하였습니다!",
				"확인", "",
				CommonConfirmPopup.ConfirmType.OK,
				null);
		}
	}

	private void GetReward(QuestData qd)
	{
		if (qd._rewards == null || qd._rewards.Count == 0)
		{
			Debug.LogWarning("[QuestManager] 보상이 없습니다.");
			return;
		}

		foreach (var reward in qd._rewards)
		{
			Debug.Log($"[QuestManager] 보상 처리: gold={reward._gold}, exp={reward._exp}, companionId={reward._companionId}, itemId={reward._itemId}, itemCount={reward._itemCount}");
			if (reward._gold > 0)
			{
				PlayerInfoManager.Instance.AddGold(reward._gold);
			}

			if (reward._exp > 0)
			{
				PlayerInfoManager.Instance.AddExp(reward._exp);
			}

			if (!string.IsNullOrEmpty(reward._companionId))
			{
				CompanionData companionData = CompanionDatabase.Instance.GetCompanionById(reward._companionId);
				if (companionData != null)
				{
					CompanionManager.Instance.AddCompanion(companionData);
				}
				else
				{
					Debug.LogWarning($"[QuestManager] 보상 동료ID가 유효하지 않습니다: {reward._companionId}");
				}
			}

			if(!string.IsNullOrEmpty(reward._itemId))
			{
				int count = reward._itemCount > 0 ? reward._itemCount : 1;
				InventoryManager.Instance.AddItem(reward._itemId, count);
			}

		}
	}

	public void CancelQuest(string questId)
	{
		if (_dicActiveQuest.ContainsKey(questId) == false)
			return;

		_dicActiveQuest.Remove(questId);
		_setStartedQuest.Remove(questId);

		OnQuestListChanged?.Invoke();
		Debug.Log($"[QuestManager] 퀘스트 포기: {questId}");
		GameManager.Instance.SaveGame();
	}

	public void ResetQuest(string questId)
	{
		_setStartedQuest.Remove(questId);
		_setCompletedQuest.Remove(questId);
		_dicActiveQuest.Remove(questId);

		OnQuestListChanged?.Invoke();
		Debug.Log($"[QuestManager] 퀘스트 초기화: {questId}");
		GameManager.Instance.SaveGame();
	}

	public string GetQuestStatus(string questId)
	{
		if (_setCompletedQuest.Contains(questId)) return "완료";
		if (_dicActiveQuest.ContainsKey(questId))
			return _dicActiveQuest[questId]._isCompleted ? "보상대기" : "진행중";
		if (_setStartedQuest.Contains(questId)) return "진행중";
		return "미시작";
	}

	//talk 타입 퀘스트 완료 대화
	public string GetTalkQuestTargetDialogue(string npcId)
	{
		foreach (var pair in _dicActiveQuest)
		{
			QuestState state = pair.Value;
			QuestData data = state._data;

			if (data._goalType == QuestGoalType.Talk
				&& data._targetId == npcId
				&& !state._isCompleted)
			{
				return data._targetDialogueId;
			}
		}
		return null;
	}
}
