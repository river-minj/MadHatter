using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[Serializable]
public class  SaveData
{
	public PlayerInfoSaveData playerInfo;
	public QuestSaveData questInfo;
}

[Serializable]
public class PlayerInfoSaveData
{
	public string name;
	public int level;
	public int exp;
	public int gold;
	public int hp;
}

[Serializable]
public class QuestSaveData
{
	public List<string> startedQuests = new List<string>();
	public List<string> completedQuests = new List<string>();
	public List<ActiveQuestEntry> activeQuests = new List<ActiveQuestEntry>();
}

[Serializable]
public class ActiveQuestEntry
{
	public string questID;
	public int currentProgress;
	public bool isCompleted;
}

public class GameSystem : MonoBehaviour
{
	private static string SavePath => Path.Combine(Application.persistentDataPath, "saveData.json");

	public static bool Exists() => File.Exists(SavePath);

	public static void Save(SaveData data)
	{
		string json = JsonUtility.ToJson(data, true);
		File.WriteAllText(SavePath, json);
		Debug.Log($"Game saved to {SavePath}");
	}

	public static SaveData Load()
	{
		if (!File.Exists(SavePath))
		{
			Debug.LogWarning($"No save file found at {SavePath}");
			return null;
		}

		string json = File.ReadAllText(SavePath);
		SaveData data = JsonUtility.FromJson<SaveData>(json);
		Debug.Log($"Game loaded from {SavePath}");
		return data;
	}

	public static void Delete()
	{
		if (File.Exists(SavePath))
		{
			File.Delete(SavePath);
			Debug.Log($"Save file deleted at {SavePath}");
		}
		else
		{
			Debug.LogWarning($"No save file to delete at {SavePath}");
		}
	}
}
