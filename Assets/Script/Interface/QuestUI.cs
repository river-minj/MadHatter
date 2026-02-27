using System.Collections.Generic;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
	[SerializeField] private GameObject _root;
	[SerializeField] private Transform _listParent;
	[SerializeField] private GameObject _questSlotPrefab;

	private List<QuestSlotController> _listQuest = new List<QuestSlotController>();

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

		foreach(var slot in _listQuest)
				{
			Destroy(slot.gameObject);
		}
		_listQuest.Clear();


		foreach(var quest in dicQuest)
		{
			var slot = Instantiate(_questSlotPrefab, _listParent);
			var controller = slot.GetComponent<QuestSlotController>();
			controller.SetQuest(quest.Value._data, quest.Value, OnSlotClaimClicked, OnSlotCancelClicked);

			_listQuest.Add(controller);
		}
	}

	private void OnQuestProgressUpdate(QuestState state)
	{
		foreach(var slot in _listQuest)
		{
			if(slot.QuestData._questId == state._data._questId)
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
		var slot = _listQuest.Find(s => s.QuestData._questId == questID);
		if (slot != null)
		{
			_listQuest.Remove(slot);

			//to do : Destroy를 하지 않는 방법으로 개선 필요
			Destroy(slot.gameObject);
		}
	}
}
