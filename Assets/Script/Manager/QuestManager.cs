using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//독립적인 데이터 구조 이며 여러 시스템에서 참조될 가능성이 높음
public class QuestState
{
	public int stage = 0;
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
	public void StartQuest(string questId)
	{
		if(_setCompletedQuest.Contains(questId))
		{
			return;
		}

		if(_dicActiveQuest.ContainsKey(questId))
		{
			return;
		}

		_dicActiveQuest.Add(questId, new QuestState());
		Debug.Log("[QuestManager] 퀘스트 시작");
	}

	//퀘스트 진행도 업데이트
	public void UpdqteQuestProgress(string questId, int stage)
	{
		if(_dicActiveQuest.ContainsKey(questId) == false)
		{
			Debug.LogWarning($"[QuestManager] 진행중인 퀘스트가 아닙니다. {questId}");
			return;
		}

		_dicActiveQuest[questId].stage = stage;
		Debug.Log($"[QuestManager] 퀘스트 {questId} 진행도 {stage} 업데이트");
	}

	//퀘스트 완료 처리
	public void CompleteQuset(string questId)
	{
		if (_dicActiveQuest.ContainsKey(questId) == false)
		{
			Debug.LogWarning($"[QuestManager] 진행중인 퀘스트가 아닙니다. {questId}");
			return;
		}

		_dicActiveQuest.Remove(questId);
		_setCompletedQuest.Add(questId);

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
