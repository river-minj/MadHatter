using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestSlotController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _questTitle;
	[SerializeField] private TextMeshProUGUI _questDesc;
	[SerializeField] private TextMeshProUGUI _questProgress;

	private QuestData _questData;
	public QuestData QuestData => _questData;
	public void SetQuest(QuestData data, QuestState state)
	{
		_questData = data;
		_questTitle.text = data._title;
		_questDesc.text = data._description;
		_questProgress.text = $"{state._currentProgress} / {data._goalCount}";
	}
	public void UpdateProgress(QuestState state)
	{
		if (_questData == null)
			return;

		_questProgress.text = $"{state._currentProgress} / {_questData._goalCount}";
	}

	public void OnGetReward()
	{
	}

	public void OnCancel()
	{

	}
}
