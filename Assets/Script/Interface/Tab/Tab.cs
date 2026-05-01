using UnityEngine;
using UnityEngine.UI;
using static TabController;

public class Tab : MonoBehaviour
{

	[SerializeField] private Button _tabButton;
	[SerializeField] private TabPage _linkedPage;

	[Header("Visual")]
	[SerializeField] private Image _image;
	[SerializeField] private Sprite _selectedSprite;
	[SerializeField] private Sprite _normalSprite;
	[SerializeField] private Sprite _lockedSprite;

	[Header("Initial State")]
	[SerializeField] private bool _startLocked = false;

	public Button TabButton => _tabButton;
	public TabPage LinkedPage => _linkedPage;
	private TabState _state = TabState.Normal;
	public TabState State => _state;
	public bool IsLocked => _state == TabState.Locked;

	private void Awake()
	{
		if (_startLocked)
		{
			SetLocked(true);
		}
		else
		{
			ApplyState(TabState.Normal);
		}
	}
	public void SetSelected(bool selected)
	{
		// 잠긴 탭은 선택 상태로 못 바뀜
		if (_state == TabState.Locked)
		{
			return;
		}

		ApplyState(selected ? TabState.Selected : TabState.Normal);
	}

	public void SetLocked(bool locked)
	{
		if (locked)
		{
			ApplyState(TabState.Locked);
		}
		else
		{
			// 잠금 해제 시 기본은 Normal
			ApplyState(TabState.Normal);
		}
	}

	private void ApplyState(TabState newState)
	{
		_state = newState;

		// 클릭 가능 여부
		if (_tabButton != null)
		{
			_tabButton.interactable = (newState != TabState.Locked);
		}

		// 스프라이트 갱신
		if (_image == null)
		{
			return;
		}

		switch (newState)
		{
			case TabState.Selected:
				_image.sprite = _selectedSprite;
				break;
			case TabState.Normal:
				_image.sprite = _normalSprite;
				break;
			case TabState.Locked:
				_image.sprite = _lockedSprite;
				break;
		}
	}
}

