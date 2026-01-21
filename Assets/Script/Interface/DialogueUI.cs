using TMPro;
using UnityEditor.Search;
using UnityEngine;

public class DialogueUI : MonoBehaviour
{
	[SerializeField] private GameObject _dialoguePanel;
	[SerializeField] private TextMeshProUGUI _dialogueText;

	
	public bool IsVisible { get; private set; }
	
	private void Awake()
	{

		Hide();
	}

	private void Start()
	{
	}

	private void Hide()
	{
		if (_dialoguePanel != null)
		{
			_dialoguePanel.SetActive(false);
		}

		IsVisible = false;
	}

	public void Show(string message)
	{
		if(_dialoguePanel == null)
		{
			return;
		}

		if(_dialogueText != null)
		{
			_dialogueText.text = message;
		}

		_dialoguePanel.SetActive(true);
		IsVisible = true;
	}

	public void Close()
	{
		Hide();
	}
}
