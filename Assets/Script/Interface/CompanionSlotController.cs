using Spine.Unity;
using TMPro;
using UnityEngine;


public class CompanionSlotData : InfiniteScrollData
{
	public CompanionData _companionData;
}

public class CompanionSlotController : InfiniteScrollItem
{
	[Header("UI")]
	[SerializeField]  private TextMeshProUGUI _nameText;
	[SerializeField] private SkeletonGraphic _spine;

	[Header("Animation Setting")]
	[SerializeField] private string _idleAnimationName = "Idle";
	[SerializeField] private bool _pauseInvisible = true;//안보일때 애니메이션 정지여부

	private bool _isVisible = false;

	protected override void RefreshUI()
	{
		if(_data is CompanionSlotData slotData)
		{
			if(slotData._companionData == null)
			{
				Debug.LogError("CompanionSlotData의 CompanionData가 null입니다.");
				return;
			}
			
			if (_nameText != null)
			{
				_nameText.text = slotData._companionData._companionName;
			}
			
			if(_spine != null)
			{
				SetUpSpine(slotData._companionData);
			}
		}
	}

	private void SetUpSpine(CompanionData data)
	{
		//if(_spine == null || data == null || data._skeletonDataAsset == null)
		{
			Debug.LogError("스파인 설정에 필요한 데이터가 누락되었습니다.");
			return;
		}

		//_spine.skeletonDataAsset = data._skeletonDataAsset;
		_spine.Initialize(true);
		_spine.AnimationState.SetAnimation(0, _idleAnimationName, true);
	}
}
