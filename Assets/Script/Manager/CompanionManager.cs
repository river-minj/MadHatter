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
		for (int i = 0; i < 5; i++)
		{
			_ownedCompanions.Add(data);
			SpawnCompanion(data);
		}



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
		//생성 위치 설정
		UpdateFollowPosition();

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
		GameObject companionObj = Instantiate(data._companionPrefab, spawnPos, Quaternion.identity);
		CompanionController cc = companionObj.GetComponent<CompanionController>();
        if (cc!= null)
        {
            _followCompanions.Add(cc);
        }
    }

	private Vector3 GetFollowPosition(int index)
	{
		Transform baseAnchor = (index%2 == 0) ? _player._companionAnchorA : _player._companionAnchorB; //배치해야하는 동료의 기준 앵커 선택

		int row = index / 2; //배치해야하는 동료가 몇번째 줄에 있는지 계산

		Vector3 offset = new Vector3(0, -3f * row, 0); //각 줄마다 y축으로 0.5씩 떨어지게 오프셋 계산

		return baseAnchor.position + offset;
	}

	private void UpdateFollowPosition()
	{
		for(int i = 0; i < _followCompanions.Count; i++)
		{
			CompanionController cc = _followCompanions[i];
			if (cc == null)
				continue;

			Vector3 pos = GetFollowPosition(i);
			cc.SetFollowPosition(pos);
			
		}
	}

	//to do : companion의 이동을 이렇게 콜하는 것이 최선인지 검토 필요
	public void RefreshFollowPosition()
	{
		UpdateFollowPosition();
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
