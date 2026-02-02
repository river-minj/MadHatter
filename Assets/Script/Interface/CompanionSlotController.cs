using TMPro;
using UnityEngine;


public class CompanionSlotData : InfiniteScrollData
{
	public CompanionData _companionData;
}

public class CompanionSlotController : InfiniteScrollItem
{
	[SerializeField]  private TextMeshProUGUI _nameText;

	private CompanionSlotData _companion = new CompanionSlotData();
	public void SetData(CompanionData companion)
	{
		if(companion == null)
		{
			Debug.LogWarning("Companion data is null.");
			return;
		}

		_companion._companionData = companion;

		RefreshUI();
	}

	protected override void RefreshUI()
	{
		if (_nameText != null)
		{
			_nameText.text = _companion._companionData._companionName;
		}
	}
}
