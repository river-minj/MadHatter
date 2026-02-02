using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DialogueDatabase : MonoBehaviour
{
	public static DialogueDatabase Instance { get; private set; }

	public List<DialogueData> _dialogueList;
	private Dictionary<string, DialogueData> _dicDialogue;

	private void Awake()
	{
		Instance = this;
	}

	public DialogueData GetDialogueByID(string dialogueID)
	{
		return _dialogueList.FirstOrDefault(d => d._dialogueID == dialogueID);
	}
}
