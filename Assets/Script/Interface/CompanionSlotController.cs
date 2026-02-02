using TMPro;
using UnityEngine;


public class CompanionSlotData : InfiniteScrollData
{
	public CompanionData companionData;
}

public class CompanionSlotController : InfiniteScrollItem
{
	[SerializeField]  private TextMeshProUGUI _nameText;

	private CompanionSlotData _companion;
	public void SetData(CompanionData companion)
	{
		if(companion == null)
		{
			Debug.LogWarning("Companion data is null.");
			return;
		}

		_companion = _data as CompanionSlotData;

		RefreshUI();
	}

	protected override void RefreshUI()
	{
		if (_nameText != null)
		{
			_nameText.text = _companion.companionData._companionName;
		}
	}
}
