using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData",	menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
	public string _dialogueID;
	public string _speakerName;
	public List<string> _lines = new List<string>();

	public IEnumerable<string> GetLines()
	{
		return _lines;
	}	
}