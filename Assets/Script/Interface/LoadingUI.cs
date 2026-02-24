using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUI : MonoBehaviour
{
	[SerializeField] private GameObject _root;
	[SerializeField] private Slider _progressBar;
	[SerializeField] private TextMeshProUGUI _progressText;
	[SerializeField] private TextMeshProUGUI _statusText;

	private void Awake()
	{
		_root.SetActive(false);
	}

	public void Show()
	{
		if (_root != null)
		{
			_root.SetActive(true);
		}

		SetProgress(0f, "");
	}

	public void Hide()
	{
		if(_root != null)
		{
			_root.SetActive(false);
		}
	}

	public void SetProgress(float progress, string message)
	{
		if (_progressBar == null)
			return;

		if (_progressText == null)
			return;

		if (_statusText == null)
			return;

		_progressBar.value = progress;
		_progressText.text = $"{(int)(progress * 100)}%";
		_statusText.text = message;
	}
}
