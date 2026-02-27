using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData",	menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
	public string _dialogueID;
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