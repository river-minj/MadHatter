using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/Quest Data")]
public class QuestData : ScriptableObject
{
	[Header("Basic Info")]
	public string _questID;
	public string _npcID;
	
	public string _title;
	[TextArea]
	public string _description;

	[Header("QuestDialogue")]
	public string _startDialogueID;
	public string _progressDialogueID;
	public string _completedDialogueID;

	public string _targetID;
	public QuestGoalType _goalType;
	public int _goalCount;

	[Header("Next Quest (optional)")]
	public string _nextQuestID;
	[Header("Reward(optional")]
	public QuestReward _reward;
}
