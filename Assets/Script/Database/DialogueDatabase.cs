using System.Collections.Generic;
using UnityEngine;

public class DialogueDatabase
{
	public static DialogueDatabase Instance { get; private set; }

	private Dictionary<string, DialogueData> _dicDialogue = new Dictionary<string, DialogueData>();
	
	public static void CreateInstance()
	{
		Instance = new DialogueDatabase();
	}

	public void ApplyData(List<DialogueTableData> tableDataList)
	{
		_dicDialogue.Clear();

		foreach (var row in tableDataList)
		{
			if (string.IsNullOrEmpty(row.uniqueId) || string.IsNullOrEmpty(row.dialogueId))
				continue;

			if (!_dicDialogue.TryGetValue(row.dialogueId, out var dialogueData))
			{
				dialogueData = new DialogueData
				{
					_dialogueId = row.dialogueId
				};
				_dicDialogue.Add(row.dialogueId, dialogueData);
			}

			dialogueData._lines.Add(new DialogueLine
			{
				_speakerName = row.speakerName,
				_dialogueType = row.dialogueType,
				_line = row.line
			});
		}
	}

	public DialogueData GetDialogueById(string dialogueId)
	{
		if (string.IsNullOrEmpty(dialogueId))
			return null;

		if (_dicDialogue.TryGetValue(dialogueId, out var data))
			return data;

		Debug.LogWarning($"[DialogueDatabase] ID [{dialogueId}]를 찾을 수 없습니다.");
		return null;
	}
}