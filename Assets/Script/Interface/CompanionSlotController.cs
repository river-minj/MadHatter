using TMPro;
using UnityEngine;


public class CompanionSlotData : InfiniteScrollData
{
	public CompanionData _companionData;
}

public class CompanionSlotController : InfiniteScrollItem
{
	[SerializeField]  private TextMeshProUGUI _nameText;

	protected override void RefreshUI()
	{
		if(_data is CompanionSlotData companionData)
		{
			if(companionData._companionData == null)
			{
				Debug.LogError("CompanionSlotData의 CompanionData가 null입니다.");
				return;
			}
			
			if (_nameText != null)
			{
				_nameText.text = companionData._companionData._companionName;
			}

		}
	}
}
