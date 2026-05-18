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
			Debug.LogError($"[NPCController] NPC Data가 없습니다: {gameObject.name} / npcId={_npcId}");
			return;
		}

		Debug.Log($"[NPCController] OnInteract | npcId={_npcId} | name={_npcData._npcName} | questId={_npcData._questId} | shopId={_npcData._shopId} | defaultDialogueId={_npcData._defaultDialogueId}");

		// 이 NPC가 Talk 퀘스트 타겟인 경우
		string targetDialogueId = QuestManager.Instance.GetTalkQuestTargetDialogue(_npcId);
		Debug.Log($"[NPCController] Talk 퀘스트 타겟 dialogueId={targetDialogueId}");
		if (!string.IsNullOrEmpty(targetDialogueId))
		{
			QuestManager.Instance.ReportTalktoNPC(_npcId);
			var targetDialogue = DialogueDatabase.Instance.GetDialogueById(targetDialogueId);
			Debug.Log($"[NPCController] targetDialogue={(targetDialogue != null ? "found" : "null")}");
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
			Debug.Log($"[NPCController] 퀘스트 분기 진입 | questId={questId}");

			while (!string.IsNullOrEmpty(questId))
			{
				QuestData questData = QuestDatabase.Instance.GetQuestById(questId);
				if (questData == null) { Debug.Log($"[NPCController] questData null: {questId}"); break; }

				if (questData._questGiverNpcId != _npcId)
				{
					Debug.Log($"[NPCController] giverNpcId 불일치: {questData._questGiverNpcId} != {_npcId}");
					break;
				}

				if (QuestManager.Instance.IsQuestCompleted(questId))
				{
					Debug.Log($"[NPCController] 퀘스트 완료됨, 다음 체인: {questData._nextQuestId}");
					questId = questData._nextQuestId;
					continue;
				}

				Debug.Log($"[NPCController] TryQuestStart: {questId}");
				QuestManager.Instance.TryQuestStart(questId);
				return;
			}
		}

		// 상점 NPC
		if (!string.IsNullOrEmpty(_npcData._shopId))
		{
			var shopDialogue = DialogueDatabase.Instance.GetDialogueById(_npcData._defaultDialogueId);
			Debug.Log($"[NPCController] 상점 분기 | shopId={_npcData._shopId} | shopDialogue={(shopDialogue != null ? "found" : "null")}");
			if (shopDialogue != null)
			{
				GameManager.Instance.StartDialogue(shopDialogue, () =>
				{
					Debug.Log($"[NPCController] 상점 대화 완료 콜백 | shopId={_npcData._shopId}");
					GameManager.Instance.EndDialogue();
					UIManager.Instance.ShowShop(_npcData._shopId);
				});
			}
			else
			{
				UIManager.Instance.ShowShop(_npcData._shopId);
			}
			return;
		}

		// 기본 대화
		var dialogue = DialogueDatabase.Instance.GetDialogueById(_npcData._defaultDialogueId);
		Debug.Log($"[NPCController] 기본 대화 분기 | defaultDialogueId={_npcData._defaultDialogueId} | dialogue={(dialogue != null ? "found" : "null")}");
		if (dialogue != null)
		{
			GameManager.Instance.StartDialogue(dialogue);
		}
	}
}
