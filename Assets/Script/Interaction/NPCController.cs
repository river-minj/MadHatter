using UnityEngine;

public class NPCController: InteractionController
{
	[SerializeField]private string _npcId;
	private NpcData _npcData;
	private string _npcName;

	public void Init()
	{
		_npcData = NpcDatabase.Instance.GetNpcById(_npcId);

		if (_npcData != null)
		{
			_npcName = _npcData._npcName;
		}
		else
		{
			Debug.LogError($"[NPCController] NPC 데이터를 찾을 수 없습니다: {_npcId}");
		}
	}

	protected override void OnInteract()
	{
		
		if(_npcData == null)
		{
			Init();
		}

		if (_npcName == null)
		{
			Debug.LogError($"[NPCController] NPC Data가 없습니다: {gameObject.name}");
			return;
		}
		if (!string.IsNullOrEmpty(_npcData._questId))
		{
			QuestData questData = QuestDatabase.Instance.GetQuestById(_npcData._questId);
			if (questData != null)
			{
				QuestManager.Instance.TryQuestStart(_npcData._questId);
			}
		}

		//현재 대화하고 있는 NPC가 퀘스트의 타겟일 경우
		QuestManager.Instance.ReportTalktoNPC(_npcId);

		//to do : Quest 진행 상황에 따른 대화 불러오기


		//var dialogue = DialogueDatabase.Instance.GetDialogueByID(_npcData._defaultDialogueID);
		//if (dialogue != null)
		//{
		//	//show dialogue
		//	GameManager.Instance?.StartDialogue(dialogue);
		//}


	}
}
