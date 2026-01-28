using UnityEngine;

public class NPCInteraction : InteractionController
{
	private string _npcName;
	private string _npcID;

	[SerializeField] private NpcData _npcData;

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (_npcData != null)
		{
			_npcID = _npcData._npcID;
			_npcName = _npcData._npcName;
		}
	}

	protected override void OnInteract()
	{
		
		if(_npcData == null)
		{
			Debug.LogWarningFormat("[NPCInteraction] NPC Data is null for NPC: {0}", gameObject.name);
			return;
		}


		if (string.IsNullOrEmpty(_npcData._questID) == false)
		{
			QuestData questData = QuestDatabase.Instance.GetQuestByID(_npcData._questID);
			if (questData != null)
			{
				QuestManager.Instance.TryQuestStart(_npcData._questID);
			}
		}

		//현재 대화하고 있는 NPC가 퀘스트의 타겟일 경우
		QuestManager.Instance.ReportTalktoNPC(_npcID);

		//to do : Quest 진행 상황에 따른 대화 불러오기


		//var dialogue = DialogueDatabase.Instance.GetDialogueByID(_npcData._defaultDialogueID);
		//if (dialogue != null)
		//{
		//	//show dialogue
		//	GameManager.Instance?.StartDialogue(dialogue);
		//}


	}
}
