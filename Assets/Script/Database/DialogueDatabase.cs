using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
	public static DialogueDatabase Instance { get; private set; }

	[SerializeField] private List<DialogueData> _dialogueList;
	private Dictionary<string, DialogueData> _dicDialogue;

	private void Awake()
	{
		Instance = this;

		MakeDic();
	}

	private void MakeDic()
	{
		_dicDialogue = new Dictionary<string, DialogueData>();
		foreach (var dialogue in _dialogueList)
		{ 
			if (dialogue == null || string.IsNullOrEmpty(dialogue._dialogueID))
				continue;
			
			_dicDialogue[dialogue._dialogueID] = dialogue;
		}
	}

	public DialogueData GetDialogueById(string dialogueId)
	{
		if (string.IsNullOrEmpty(dialogueId))
			return null;

		if (_dicDialogue.TryGetValue(dialogueId, out DialogueData dialogue))
		{
			return dialogue;
		}

		Debug.LogWarning($"Dialogue with ID '{dialogueId}' not found in the database.");
		return null;

	}
}
