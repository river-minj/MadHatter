using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 드래그 이벤트 처리, 방향값 계산, PlayerController에 방향값 전달
/// </summary>
public class JoystickUI : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
	[SerializeField] private RectTransform _joystickRoot;
	[SerializeField] private RectTransform _handle;
	[SerializeField] private float _handleRange = 60f;

	private PlayerController _playerController;
	
	private void Awake()
	{
		_joystickRoot.gameObject.SetActive(false);

	}

	private void Start()
	{
#if !UNITY_WEBGL && !UNITY_ANDROID && !UNITY_IOS && !UNITY_EDITOR
		gameObject.SetActive(false);
		return;
#endif
		_playerController = GameManager.Instance.GetPlayerController();
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		//터치한 위치로 조이스틱 루트 이동
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_joystickRoot,
			eventData.position,
			eventData.pressEventCamera,
			out Vector2 localPoint
			);

		_joystickRoot.anchoredPosition = localPoint;
		_joystickRoot.gameObject.SetActive(true);

		OnDrag(eventData);
	}

	public void OnDrag(PointerEventData eventData)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
				transform as RectTransform,
				eventData.position,
				eventData.pressEventCamera,
				out Vector2 localPoint
				);

		//핸들 이동 범위 제한
		Vector2 clamped = Vector2.ClampMagnitude(localPoint, _handleRange);
		_handle.anchoredPosition = clamped;

		//방향 값 계산 후 player controller에 전달
		Vector2 direction = clamped / _handleRange; // -1 ~ 1 범위로 정규화
		Debug.Log($"[JoystickUI] Handle Position: {clamped}, Direction: {direction}");

		if (_playerController != null)
		{
			Debug.Log($"[JoystickUI] Before SetJoystickInput: {direction}");
			_playerController?.SetJoystickInput(direction);
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (_joystickRoot != null)
		{
			_joystickRoot.gameObject.SetActive(false);
		}

		if (_handle != null)
		{
			_handle.anchoredPosition = Vector2.zero;
		}

		if (_playerController != null)
		{
			_playerController.SetJoystickInput(Vector2.zero);
		}
	}
}
