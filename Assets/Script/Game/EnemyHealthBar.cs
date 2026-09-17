using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
	[SerializeField] private GameObject _barRoot;
	[SerializeField] private Slider _slider;
	[SerializeField] private float _visibleDuration = 2f;

	private EnemyController _controller;
	private float _hideTimer;

	private void Awake()
	{
		_controller = GetComponent<EnemyController>();

		if (_barRoot != null)
			_barRoot.SetActive(false);
	}

	private void OnEnable()
	{
		if (_controller != null)
			_controller.OnHpChanged += HandleHpChanged;
	}

	private void OnDisable()
	{
		if (_controller != null)
			_controller.OnHpChanged -= HandleHpChanged;
	}

	private void Update()
	{
		if (_barRoot == null || !_barRoot.activeSelf)
			return;

		_hideTimer -= Time.deltaTime;
		if (_hideTimer <= 0f)
			_barRoot.SetActive(false);
	}

	private void HandleHpChanged(int current, int max)
	{
		if (_slider != null)
			_slider.value = max > 0 ? (float)current / max : 0f;

		if (current <= 0)
		{
			if (_barRoot != null)
				_barRoot.SetActive(false);
			return;
		}

		if (_barRoot != null)
			_barRoot.SetActive(true);

		_hideTimer = _visibleDuration;
	}
}
