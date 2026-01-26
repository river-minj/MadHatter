using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//독립적인 데이터 구조이며 여러 시스템에서 참조될 가능성이 높음
public class QuestState
{
	public string questId;
	public int currentStep;
	public bool isCompleted;

	public QuestState(string id)
	{
		questId = id;
		currentStep = 0;
		isCompleted = false;
	}
}

/// <summary>
///	퀘스트 매니저는 전역 상태이기 때문에 싱글톤 패턴으로 구현
///	맵 전환, 대사 변경 등의 상태에서도 퀘스트 상태가 유지되어야 하기 때문
///	여러 시스템이 접근 해야하기 때문에 싱글톤 패턴이 적합
/// </summary>
public class QuestManager : MonoBehaviour
{

	public static QuestManager Instance { get; private set; }	

	private Dictionary<string, QuestState> _dicActiveQuest = new Dictionary<string, QuestState>(); //<퀘스트ID, 퀘스트상태>
	private HashSet<string> _setCompletedQuest = new HashSet<string>(); //완료된 퀘스트ID

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

	//퀘스트 수락
	public void StartQuest(QuestData questData )
	{
		if(questData == null)
		{
			return;
		}

		if(_setCompletedQuest.Contains(questData._questId))
		{
			Debug.LogWarning($"[QuestManager] 이미 완료된 퀘스트입니다. {questData._questId}");
			return;
		}

		if(_dicActiveQuest.ContainsKey(questData._questId))
		{
			Debug.LogWarning($"[QuestManager] 이미 진행중인 퀘스트입니다. {questData._questId}");
			return;
		}

		QuestState newQuest = new QuestState(questData._questId);
		_dicActiveQuest.Add(newQuest.questId, newQuest);

		Debug.Log("[QuestManager] 퀘스트 시작");
	}

	public QuestState GetQuestState(string questId)
	{
		_dicActiveQuest.TryGetValue(questId, out QuestState questState);
		return questState;
	}

	//퀘스트 진행도 업데이트
	public void UpdqteQuestProgress(QuestData questData)
	{
		if(_dicActiveQuest.TryGetValue(questData._questId, out QuestState questState) == false)
		{
			Debug.LogWarning($"[QuestManager] 진행중인 퀘스트가 아닙니다. {questData._questId}");
			return;
		}

		if(questState.currentStep >= questData.steps.Length - 1)
		{
			CompleteQuset(questData._questId);
			return;
		}

		questState.currentStep++;
		
		Debug.Log($"[QuestManager] 퀘스트 {questData._questId} 진행도 {questState.currentStep} 업데이트");
	}

	//퀘스트 완료 처리
	public void CompleteQuset(QuestData questData )
	{
		if(questData == null)
		{
			return;
		}

		if (!_dicActiveQuest.TryGetValue(questData._questId, out QuestState state) == false)
		{
			Debug.LogWarning($"[QuestManager] 진행중인 퀘스트가 아닙니다. {questData._questId}");
			return;
		}

		_dicActiveQuest.Remove(state.questId);
		_setCompletedQuest.Add(state.questId);

		Debug.Log("[QuestManager] 퀘스트 완료");
	}

	public bool IsQuestCompleted(string questId)
	{
		return _setCompletedQuest.Contains(questId);
	}

	public bool IsQuestActive(string questId)
	{
		return _dicActiveQuest.ContainsKey(questId);
	}
}
