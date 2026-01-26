using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NpcData", menuName = "Game/Npc Data")]
public class NpcData : ScriptableObject
{
	public string _npcID;
	public string _npcName;
	public string _defaultDialogueID;
	public string _questID;
}
