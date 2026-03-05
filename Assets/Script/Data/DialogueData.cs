using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueData
{
	public string _dialogueId;
	public List<DialogueLine> _lines = new List<DialogueLine>();
}

[Serializable]
public class DialogueLine
{
	public string _speakerName;
	public DialogueType _dialogueType;
	[TextArea] public string _line;
}

public enum DialogueType
{
	NPC,
	Monologue,
	System
}