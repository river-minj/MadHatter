using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 언락된 동료 관리
/// </summary>
public class CompanionManager : MonoBehaviour
{
	public static CompanionManager Instance;
	private List<CompanionData> _ownedCompanions = new(); // 언락된 동료 ID
	public IEnumerable<CompanionData> OwnedCompanions => _ownedCompanions;
	
	private List<CompanionController> _lineA = new List<CompanionController>();
	private List<CompanionController> _lineB = new List<CompanionController>();

	private PlayerController _player; //플레이어가 가지고 있는 동료 위치를 얻기 위한 참조
	[SerializeField] Transform _followerParent;

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

	}

	private void Start()
	{
		if(_player == null)
		{
			_player = FindObjectOfType<PlayerController>();
		}
	}

	//동료 획득
	public void AddCompanion(CompanionData data)
	{
		if (_ownedCompanions.Contains(data))
		{
			Debug.LogWarningFormat("Companion already unlocked: {0}", data._companionId);
			return;
		}

		_ownedCompanions.Add(data);
		Debug.LogFormat("Companion 획득: {0}", data._companionId);

		//생성
		SpawnCompanion(data);

	}

	//동료 생성
	public void SpawnCompanion(CompanionData data)
	{
		if(_player == null)
		{
			Debug.LogError("PlayerController reference is missing.");
			return;
		}

		Vector3 spawnPos = _player.transform.position; //일단 플레이어 위치에 생성

		GameObject prefab = Resources.Load<GameObject>(data._companionPrefabPath);
		if (prefab == null)
		{
			Debug.LogError($"[CompanionManager] 프리팹을 찾을 수 없습니다: {data._companionPrefabPath}");
			return;
		}

		GameObject companionObj = Instantiate(prefab, spawnPos, Quaternion.identity, _followerParent);

		CompanionController cc = companionObj.GetComponent<CompanionController>();
		if(cc == null)
		{
			Debug.LogErrorFormat("Companion prefab missing CompanionController: {0}", data._companionId);
			return;
		}

		int totlaCount = _lineA.Count + _lineB.Count;
		bool addToLineA = _lineA.Count <= _lineB.Count;
		int indexInLine = addToLineA? _lineA.Count : _lineB.Count;

		cc.Initialize(_player, data, totlaCount, addToLineA, indexInLine);

        if (addToLineA)
        {
			_lineA.Add(cc);
		}
		else
		{
			_lineB.Add(cc);
		}

		Debug.Log($"동료 생성: Index={cc._followIndex}, LineA? = {addToLineA}, Pos={indexInLine}");
        
    }

	public void SetFacingDirection(bool isRight)
	{
		foreach(var cc in _lineA)
		{
			if (cc == null)
				continue;
			cc.SetFacingDirection(isRight);			
		}

		foreach (var cc in _lineB)
		{
			if (cc == null)
				continue;
			cc.SetFacingDirection(isRight);
		}
	}
	public int GetTotalCompanionCount()
	{
		return _lineA.Count + _lineB.Count;
	}

	// 디버그: 현재 상태 출력
	[ContextMenu("Print Companion Status")]
	public void PrintCompanionStatus()
	{
		Debug.Log($"=== Companion Status ===");
		Debug.Log($"Total: {GetTotalCompanionCount()}");
		Debug.Log($"Line A: {_lineA.Count}");
		Debug.Log($"Line B: {_lineB.Count}");

		Debug.Log("--- Line A ---");
		for (int i = 0; i < _lineA.Count; i++)
		{
			Debug.Log($"  [{i}] {_lineA[i].name}");
		}

		Debug.Log("--- Line B ---");
		for (int i = 0; i < _lineB.Count; i++)
		{
			Debug.Log($"  [{i}] {_lineB[i].name}");
		}
	}
}
