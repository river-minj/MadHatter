using System.Collections;
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
	}

	private void OnDestroy()
	{
		if(QuestManager.Instance == null)
			return;

		QuestManager.Instance.OnQuestListChanged -= RefreshQuestList;
		QuestManager.Instance.OnQuestProgressUpdate -= OnQuestProgressUpdate;
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
			controller.SetQuest(quest.Value._data, quest.Value);
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
	}
}
