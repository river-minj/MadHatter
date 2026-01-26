using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/Quest Data")]
public class QuestData : ScriptableObject
{
	[Header("Basic Info")]
	public string _questId;
	public string _title;
	[TextArea]
	public string _description;

	[Header("Dialogue Steps")]
	[TextArea]
	public string[] steps;

	[Header("Next Quest (optional)")]
	public int nextQuestId = -1;
	[Header("Reward(optional")]
	public string rewardId;
}
