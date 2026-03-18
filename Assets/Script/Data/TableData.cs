using System;

[Serializable]
public class DialogueTableData
{
	public string uniqueId;
	public string dialogueId;
	public string speakerName;
	public DialogueType dialogueType;
	public string line;
}

[Serializable]
public class QuestTableData
{
	public string uniqueId;
	public string title;
	public string description;
	public QuestGoalType goalType;
	public int goalCount;
	public string questGiverNpcId;
	public string questCompleterNpcId;
	public string targetId;
	public string rewardGroupId;
	public string startDialogueId;
	public string progressDialogueId;
	public string completedDialogueId;
	public string nextQuestId;
}

[Serializable]
public class RewardTableData
{
	public string uniqueId;
	public string rewardGroupId;
	public int gold;
	public int exp;
	public string companionId;
	public string itemId;
	public int itemCount;
}

[Serializable]
public class NpcTableData
{
	public string uniqueId;
	public string npcName;
	public string defaultDialogueId;
	public string questId;
}

[Serializable]
public class CompanionTableData
{
	public string uniqueId;
	public string companionName;
	public string skinName;
	public string companionPrefabPath;
	public float followSpeed;
	public float followDistance;
}

[Serializable]
public class ItemTableData
{
	public string itemId;
	public string itemName;
	public ItemType itemType;
	public int effectValue;
	public string description;
	public string iconPath;
}