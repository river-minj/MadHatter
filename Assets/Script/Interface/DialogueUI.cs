using TMPro;
using UnityEngine;

/// <summary>
///	대화 UI 컨트롤러
/// </summary>
public class DialogueUI : MonoBehaviour
{
	[SerializeField] private GameObject _dialoguePanel;
	[SerializeField] private TextMeshProUGUI _dialogueText;
	[SerializeField] private TextMeshProUGUI _name;
	[SerializeField] private GameObject _nameRoot;
	
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

	public void Show(string name, string lines, DialogueType dialogueType)
	{

		if(_dialogueText != null)
		{
			_dialogueText.text = lines;
		}

		//이름 영역 분기
		bool showName = dialogueType != DialogueType.System;
		if(_nameRoot!= null)
		{
			_nameRoot.SetActive(showName);
		}

		if (showName && _name != null)
		{
			_name.text = name;
		}
		
		if(_dialoguePanel != null)
		{
			_dialoguePanel.SetActive(true);
			IsVisible = true;
		}

	}

	public void Close()
	{
		Hide();
	}
}
