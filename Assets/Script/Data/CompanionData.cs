using UnityEngine;
[CreateAssetMenu(fileName = "CompanionData", menuName = "Game/Companion Data")]

[System.Serializable]
public class CompanionData : ScriptableObject
{
	[Header("Basic Info")]
	public string _companionID;
	public string _companionName;
	public string _skinName; 

	public GameObject _companionPrefab;
	public float _followSpeed = 3f;
	public float _followDistance = 1.2f;
}
