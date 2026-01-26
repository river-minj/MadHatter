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

	private HashSet<string> _setStartedQuest = new HashSet<string>(); //시작한 퀘스트ID
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

}
