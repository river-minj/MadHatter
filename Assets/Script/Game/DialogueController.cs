using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueController : MonoBehaviour
{
    [SerializeField] private DialogueUI _dialogueUI;

    private Queue<DialogueLine> _lines = new Queue<DialogueLine>();    
    private Action _onDialogueComplete;


    public bool IsDialogueRunning => _isDialogueRunning;
    bool _isDialogueRunning = false;

    private void Awake()
    {
		if(_dialogueUI == null)
        {
			if(_dialogueUI == null)
            {
				Debug.LogError("DialogueUI component is required for DialogueController.");
			}
		}
	}
	

	// Update is called once per frame
	void Update()
    {
        if (!_isDialogueRunning) //대화창이 열린 상태가 아니면 입력을 받지 않도록 한다.
            return;

        //to do : 키 입력의 판단이 이 위치에 있는 것이 맞는지 고민해볼 것
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.E))
        {
            ShowNextLine();
        }
    }

    /// <summary>
    /// 대화 시작
    /// </summary>
    public void StartDialogue(IEnumerable<DialogueLine> lines, Action onComplete = null)
    {
        if (IsDialogueRunning == true)
            return;


        _isDialogueRunning = true;
        _onDialogueComplete = onComplete;

        _lines.Clear();
        foreach (var line in lines)
        {
            if (line == null || string.IsNullOrEmpty(line._line) == true)
                continue;

			_lines.Enqueue(line); //큐에 대화 줄들을 추가
		}


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

		DialogueLine line = _lines.Dequeue(); //큐 안에서 다음 대화를 꺼내오기
		_dialogueUI.Show(line._speakerName, line._line, line._dialogueType);
	}

    /// <summary>
    /// 대화 종료 처리
    /// </summary>
	private void EndDialogue()
	{
        _isDialogueRunning = false;
        
        if(_dialogueUI != null && _dialogueUI.IsVisible)
        {
    		_dialogueUI.Close();
        }

		_onDialogueComplete?.Invoke();
        _onDialogueComplete = null;
	}
}
