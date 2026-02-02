using System.Collections.Generic;
using UnityEngine;

public class CompanionDatabase : MonoBehaviour
{
	public static CompanionDatabase Instance;

	[SerializeField] private List<CompanionData> _companionList = new List<CompanionData>();

	private Dictionary<string, CompanionData> _dicCompanion = new Dictionary<string, CompanionData>();

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
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
