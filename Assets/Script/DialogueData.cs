using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueData",	menuName = "Game/Dialogue/Dialogue Data")]
public class DialogueData : ScriptableObject
{
	public string name;

	public List<string> lines = new List<string>();

	public IEnumerable<string> GetLines()
	{
		return lines;
	}	
}