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

	protected override void OnPlayerExit()
	{
		base.OnPlayerExit();
		if (_npcData != null && !string.IsNullOrEmpty(_npcData._shopId))
			UIManager.Instance.HideShop();
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

#if UNITY_EDITOR || DEVELOPMENT_BUILD
		Debug.Log($"[NPCController] OnInteract | npcId={_npcId} | name={_npcData._npcName} | questId={_npcData._questId} | shopId={_npcData._shopId} | defaultDialogueId={_npcData._defaultDialogueId}");
#endif

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

			while (!string.IsNullOrEmpty(questId))
			{
				QuestData questData = QuestDatabase.Instance.GetQuestById(questId);
				if (questData == null) break;

				if (questData._questGiverNpcId != _npcId) break;

				if (QuestManager.Instance.IsQuestCompleted(questId))
				{
					questId = questData._nextQuestId;
					continue;
				}

				// 선행 퀘스트 미완료 시 체인 중단 → 기본 대화 fallthrough
				if (!string.IsNullOrEmpty(questData._preQuestId) &&
					!QuestManager.Instance.IsQuestCompleted(questData._preQuestId))
				{
					break;
				}

				QuestManager.Instance.TryQuestStart(questId);
				return;
			}
		}

		// 상점 NPC
		if (!string.IsNullOrEmpty(_npcData._shopId))
		{
			var shopDialogue = DialogueDatabase.Instance.GetDialogueById(_npcData._defaultDialogueId);
			if (shopDialogue != null)
			{
				GameManager.Instance.StartDialogue(shopDialogue, () =>
				{
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
		if (dialogue != null)
		{
			GameManager.Instance.StartDialogue(dialogue);
		}
	}
}
