using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogueUI _dialogueUI;

    private Queue<string> _lines;
    private Action _onDialogueComplete;

    bool _isDialogueRunning = false;

    public bool IsDialogueRunning => _isDialogueRunning;

    private void Awake()
    {
		if(_dialogueUI == null)
        {
			_dialogueUI = GetComponentInChildren<DialogueUI>();
			if(_dialogueUI == null)
            {
				Debug.LogError("DialogueUI component is required for DialogueController.");
			}
		}

		_lines = new Queue<string>();
        _isDialogueRunning = false;
	}
	

	// Update is called once per frame
	void Update()
    {
        if (!_isDialogueRunning) //대화창이 열린 상태가 아니면 입력을 받지 않도록 한다.
            return;

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            ShowNextLine();
        }
    }

    public void StartDialogue(IEnumerable<string> lines, Action onComplete = null)
    {
        if (_dialogueUI== null)
        {
            onComplete?.Invoke();
            return;
        }

        if(lines == null)
        {
            onComplete?.Invoke();
            return;
        }

        _lines.Clear();
        foreach (var line in lines)
        {
            if (string.IsNullOrEmpty(line) == true)
                continue;

			_lines.Enqueue(line); //큐에 대화 줄들을 추가
		}

		_onDialogueComplete = onComplete;
		_isDialogueRunning = true;

		ShowNextLine();

    }

    private void ShowNextLine()
    {
        if (_isDialogueRunning == false)
            return;

        if (_lines == null || _lines.Count == 0)
        {
			EndDialogue();
			return;
		}

		string line = _lines.Dequeue(); //큐 안에서 다음 대화를 꺼내오기
		_dialogueUI.Show(line);
	}

	private void EndDialogue()
	{
        _isDialogueRunning = false;
        
        if(_dialogueUI != null && _dialogueUI.IsVisible  )
        {
    		_dialogueUI.Close();
        }

		_onDialogueComplete?.Invoke();
        _onDialogueComplete = null;
	}
}
