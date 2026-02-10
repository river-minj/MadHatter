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
	
	private List<CompanionController> _followCompanions = new List<CompanionController>(); // 따라오는 동료 리스트

	private PlayerController _player; //플레이어가 가지고 있는 동료 위치를 얻기 위한 참조
	[SerializeField] int _followDistance = 2; //동료 사이의 거리
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

		//테스트용, 나중에 삭제
		CompanionData data = CompanionDatabase.Instance.GetCompanionByID("C_WorkMan");
		AddCompanion(data);

		//2
		 data = CompanionDatabase.Instance.GetCompanionByID("C_002");
		AddCompanion(data);

		//3
		 data = CompanionDatabase.Instance.GetCompanionByID("C_003");
		AddCompanion(data);
		//4
		 data = CompanionDatabase.Instance.GetCompanionByID("C_004");
		AddCompanion(data);
		//5
		 data = CompanionDatabase.Instance.GetCompanionByID("C_005");
		AddCompanion(data);



	}

	//동료 획득
	public void AddCompanion(CompanionData data)
	{
		if (_ownedCompanions.Contains(data))
		{
			Debug.LogWarningFormat("Companion already unlocked: {0}", data._companionID);
			return;
		}

		_ownedCompanions.Add(data);
		Debug.LogFormat("Companion 획득: {0}", data._companionID);

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
		GameObject companionObj = Instantiate(data._companionPrefab, spawnPos, Quaternion.identity, _followerParent);
		CompanionController cc = companionObj.GetComponent<CompanionController>();
        if (cc!= null)
        {
            _followCompanions.Add(cc);
			int followIndex = _followCompanions.IndexOf(cc);
			cc.SetData(_player, data,followIndex);
        }
    }

	internal void SetFacingDirection(bool isRight)
	{
		foreach(var cc in _followCompanions)
		{
			if (cc == null)
				continue;

			//필요시 companion의 방향 전환 처리
			cc.SetFacingDirection(isRight);			
		}
	}
}
