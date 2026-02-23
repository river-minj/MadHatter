using System;
using TMPro;
using UnityEngine;

public class QuestSlotController : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _questTitle;
	[SerializeField] private TextMeshProUGUI _questDesc;
	[SerializeField] private TextMeshProUGUI _questProgress;
	[SerializeField] private GameObject _rewardButton;
	[SerializeField] private GameObject _cancelButton;

	private QuestData _questData;
	public QuestData QuestData => _questData;

	private Action<string> _onClaimClicked;
	private Action<string> _onCancelClicked;

	public void SetQuest(QuestData data, QuestState state, Action<string> onClaim, Action<string> onCancel)
	{
		_questData = data;
		_onClaimClicked = onClaim;
		_onCancelClicked = onCancel;

		_questTitle.text = data._title;
		_questDesc.text = data._description;
		_questProgress.text = $"{state._currentProgress} / {data._goalCount}";

		RefreshRewardButton(state);
	}

	private void RefreshRewardButton(QuestState state)
	{
		if(state == null)
			return;

		bool isCompleted = state._isCompleted;

		_rewardButton.SetActive(isCompleted);
		_cancelButton.SetActive(!isCompleted);

	}
	public void UpdateProgress(QuestState state)
	{
		if (_questData == null)
			return;

		_questProgress.text = $"{state._currentProgress} / {_questData._goalCount}";
	}

	// 메서드 작성
	public void OnGetReward()
	{
		if (_questData == null)
			return;

		_onClaimClicked?.Invoke(_questData._questId);
	}

	public void OnCancel()
	{
		if (_questData == null)
			return;

		_onCancelClicked?.Invoke(_questData._questId);
	}
}
