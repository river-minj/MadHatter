using System.Collections.Generic;

public enum QuestGoalType
{
	None,
	Kill,
	Collect,
	Talk,
	Explore
}


public class QuestData
{
	public string _questId;
	public string _questGiverNpcId;

	public string _title;
	public string _description;

	public string _startDialogueId;
	public string _progressDialogueId;
	public string _completedDialogueId;

	public string _targetDialogueId;

	public string _targetId; //몬스터 or 파밍 아이템 or 대화 npc
	public QuestGoalType _goalType;
	public int _goalCount;

	public string _rewardGroupId;
	public List<QuestReward> _rewards = new List<QuestReward>();

	public string _nextQuestId;
	public string _preQuestId;
}