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
		if (_npcData == null)
		{
			Init();
		}

		if (_npcData == null)
		{
			Debug.LogError($"[NPCController] NPC Data가 없습니다: {gameObject.name}");
			return;
		}

		// 이 NPC가 Talk 퀘스트 타겟인 경우
		string targetDialogueId = QuestManager.Instance.GetTalkQuestTargetDialogue(_npcId);
		if (!string.IsNullOrEmpty(targetDialogueId))
		{
			QuestManager.Instance.ReportTalktoNPC(_npcId);
			var targetDialogue = DialogueDatabase.Instance.GetDialogueById(targetDialogueId);
			if (targetDialogue != null)
			{
				GameManager.Instance.StartDialogue(targetDialogue);
				return;
			}
		}
		// 이 NPC가 줄 퀘스트가 있는 경우
		if (!string.IsNullOrEmpty(_npcData._questId))
		{
			string questId = _npcData._questId;

			// 체인 퀘스트: 현재 줄 수 있는 퀘스트 찾기
			while (!string.IsNullOrEmpty(questId))
			{
				QuestData questData = QuestDatabase.Instance.GetQuestById(questId);
				if (questData == null) break;

				if (questData._questGiverNpcId != _npcId)
					break;

				// 완료된 퀘스트면 다음 체인으로
				if (QuestManager.Instance.IsQuestCompleted(questId))
				{
					questId = questData._nextQuestId;
					continue;
				}

				// 시작 가능하거나 진행 중인 퀘스트 발견
				QuestManager.Instance.TryQuestStart(questId);
				return;
			}
		}

		// 줄 퀘스트가 없으면 기본 대화
		var dialogue = DialogueDatabase.Instance.GetDialogueById(_npcData._defaultDialogueId);
		if (dialogue != null)
		{
			GameManager.Instance.StartDialogue(dialogue);
		}
	}
}
