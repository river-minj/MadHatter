using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
	[SerializeField] private GameObject _root;
	[SerializeField] private Transform _listParent;
	[SerializeField] private GameObject _questSlotPrefab;
	[SerializeField] private int _maxQuestCount = 10;

	private List<QuestSlotController> _slotPool = new List<QuestSlotController>();

	private void Start()
	{
		if(QuestManager.Instance == null)
			return;

		QuestManager.Instance.OnQuestListChanged += RefreshQuestList;
		QuestManager.Instance.OnQuestProgressUpdate += OnQuestProgressUpdate;
		QuestManager.Instance.OnQuestRewardClaimed += OnQuestRewardClaimed;
	}

	private void OnDestroy()
	{
		if(QuestManager.Instance == null)
			return;

		QuestManager.Instance.OnQuestListChanged -= RefreshQuestList;
		QuestManager.Instance.OnQuestProgressUpdate -= OnQuestProgressUpdate;
		QuestManager.Instance.OnQuestRewardClaimed -= OnQuestRewardClaimed;
	}

	public void Toggle()
	{
		bool isActive = _root.activeSelf;
		_root.SetActive(!isActive);

		if(!isActive)
		{
			RefreshQuestList();
		}
	}

	private void RefreshQuestList()
	{
		var dicQuest = QuestManager.Instance?.DicActiveQuest;
		if (dicQuest == null) return;

		var quests = new List<QuestState>(dicQuest.Values);

		// 풀이 부족하면 필요한 만큼 추가 (최대 _maxQuestCount까지만 생성)
		int targetSize = Mathf.Min(quests.Count, _maxQuestCount);
		while (_slotPool.Count < targetSize)
		{
			var go = Instantiate(_questSlotPrefab, _listParent);
			go.SetActive(false);
			_slotPool.Add(go.GetComponent<QuestSlotController>());
		}

		for (int i = 0; i < _slotPool.Count; i++)
		{
			if (i < quests.Count)
			{
				_slotPool[i].SetQuest(quests[i].Data, quests[i], OnSlotClaimClicked, OnSlotCancelClicked);
				_slotPool[i].gameObject.SetActive(true);
			}
			else
			{
				_slotPool[i].gameObject.SetActive(false);
			}
		}
	}

	private void OnQuestProgressUpdate(QuestState state)
	{
		foreach(var slot in _slotPool)
		{
			if(slot.gameObject.activeSelf && slot.QuestData._questId == state.Data._questId)
			{
				slot.UpdateProgress(state);
				break;
			}
		}

		GameManager.Instance.SaveGame();
	}

	private void OnSlotClaimClicked(string questID)
	{
		QuestManager.Instance.ClaimReward(questID);
	}

	private void OnSlotCancelClicked(string questID)
	{
		QuestManager.Instance.CancelQuest(questID);
	}

	private void OnQuestRewardClaimed(string questID)
	{
		RefreshQuestList();
	}
}
