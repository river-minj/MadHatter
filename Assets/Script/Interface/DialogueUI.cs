using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///	대화 UI 컨트롤러
/// </summary>
public class DialogueUI : MonoBehaviour
{
	[SerializeField] private GameObject _dialoguePanel;
	[SerializeField] private TextMeshProUGUI _dialogueText;
	[SerializeField] private TextMeshProUGUI _name;
	[SerializeField] private GameObject _nameRoot;

	//전체화면 터치 버튼, 대	화창이 켜질 때 활성화, 대화창이 꺼질 때 비활성화된다.
	[SerializeField] private Button _touchButton;
	[SerializeField] private float _typingSpeed = 0.05f;

	private Queue<DialogueLine> _lines = new Queue<DialogueLine>();
	private Action _onDialogueComplete;
	private Coroutine _typingCoroutine;
	private string _fullText;
	private bool _isTyping;

	public bool IsDialogueRunning => _isDialogueRunning;
	private bool _isDialogueRunning = false;
	public bool IsVisible { get; private set; }


	private void Awake()
	{
		if (_touchButton != null)
		{
			_touchButton.onClick.AddListener(OnDialogueClicked);
		}
		Hide();
	}

	public void AdvanceDialogue()
	{
		if (!_isDialogueRunning)
			return;

		HandleAdvance();
	}
	public void StartDialogue(IEnumerable<DialogueLine> lines, Action onComplete = null)
	{
		if (_isDialogueRunning)
			return;

		_isDialogueRunning = true;
		_onDialogueComplete = onComplete;
		_lines.Clear();

		foreach (var line in lines)
		{
			if (line == null || string.IsNullOrEmpty(line._line))
				continue;
			_lines.Enqueue(line);
		}

		ShowNextLine();
	}

	private void ShowNextLine()
	{
		if (!_isDialogueRunning)
			return;

		if (_lines == null || _lines.Count == 0)
		{
			EndDialogue();
			return;
		}

		DialogueLine line = _lines.Dequeue();
		Show(line._speakerName, line._line, line._dialogueType);

		if (_typingCoroutine != null)
			StopCoroutine(_typingCoroutine);

		_typingCoroutine = StartCoroutine(TypeLine(line._line));
	}

	private void Show(string name, string line, DialogueType dialogueType)
	{
		_fullText = line;

		if (_dialogueText != null)
		{
			_dialogueText.text = "";
		}

		bool showName = dialogueType != DialogueType.System;
		GameObject nameObj = _nameRoot != null ? _nameRoot : _name?.gameObject;

		if (nameObj != null)
		{
			nameObj.SetActive(showName);
		}

		if (showName && _name != null)
		{
			_name.text = name;
		}

		if (_dialoguePanel != null)
		{
			_dialoguePanel.SetActive(true);
			IsVisible = true;
		}

		if (_touchButton != null)
		{
			_touchButton.gameObject.SetActive(true);
		}
	}

	private IEnumerator TypeLine(string fullText)
	{
		_isTyping = true;

		for (int i = 0; i < fullText.Length; i++)
		{
			if (_dialogueText != null)
			{
				_dialogueText.text = fullText.Substring(0, i + 1);
			}
			yield return new WaitForSeconds(_typingSpeed);
		}

		_isTyping = false;
		_typingCoroutine = null;
	}

	public void OnDialogueClicked()
	{
		AdvanceDialogue();
	}

	private void HandleAdvance()
	{
		if (_isTyping)
		{
			CompleteTyping();
		}
		else
		{
			ShowNextLine();
		}
	}

	private void CompleteTyping()
	{
		if (_typingCoroutine != null)
		{
			StopCoroutine(_typingCoroutine);
			_typingCoroutine = null;
		}

		_isTyping = false;

		if (_dialogueText != null)
		{
			_dialogueText.text = _fullText;
		}
	}

	private void EndDialogue()
	{
		_isDialogueRunning = false;

		if (_typingCoroutine != null)
		{
			StopCoroutine(_typingCoroutine);
			_typingCoroutine = null;
		}

		_isTyping = false;
		Hide();

		_onDialogueComplete?.Invoke();
		_onDialogueComplete = null;
	}

	private void Hide()
	{
		if (_dialoguePanel != null)
		{
			_dialoguePanel.SetActive(false);
		}

		if (_touchButton != null)
		{
			_touchButton.gameObject.SetActive(false);
		}

		IsVisible = false;
	}

	public void Close()
	{
		Hide();
	}
}
