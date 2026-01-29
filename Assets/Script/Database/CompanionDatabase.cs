using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEngine;

public class CompanionDatabase : MonoBehaviour
{
	public static CompanionDatabase instance;

	[SerializeField] private List<CompanionData> _companionList = new List<CompanionData>();

	private Dictionary<string, CompanionData> _dicCompanion = new Dictionary<string, CompanionData>();

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}

		BuildMap();
	
	}

	private void BuildMap()
	{
		foreach (CompanionData companion in _companionList)
		{
			_dicCompanion.Add(companion._companionID, companion);
		}
	}

	public CompanionData GetCompanionByID(string companionID)
	{
		if (_dicCompanion.TryGetValue(companionID, out CompanionData companionData))
		{
			return companionData;
		}

		Debug.LogWarningFormat("[CompanionDatabase] Companion ID not found: {0}", companionID);
		return null;
	}
}
