
using System;
using System.Collections.Generic;
using UnityEngine;


public enum SpawnPointId
{
	Default,
	Left,
	Right,
	Up,
	Down

}

[Serializable]
public class SpawnPointEntry
{
	public SpawnPointId id;
	public Transform point;
}
/// <summary>
/// 현재 활성화 된 맵을 관리
/// 카메라, 플레이어, 전환 흐름을 조율하는 컨트롤러
/// </summary>
public class MapController : MonoBehaviour
{
	[SerializeField] private MapController _nextMapMc;
	[SerializeField] private MapBounds _mapBounds;
	[SerializeField] private Vector2 _playerScale = new Vector2(0.4f, 0.4f);
	[SerializeField] private float _playerSpeed = 0.5f;


	[SerializeField] private List<SpawnPointEntry> _listSpawnPoint;
	private Dictionary<SpawnPointId, Transform> _dicSpawnPoints = new Dictionary<SpawnPointId, Transform>();

	public MapBounds MapBounds => _mapBounds;
	public Vector2 PlayerScale => _playerScale;
	public float PlayerSpeed => _playerSpeed;

	private void Awake()
	{
		if(_mapBounds == null)
		{
			_mapBounds = GetComponentInChildren<MapBounds>();
		}

		_dicSpawnPoints = new Dictionary<SpawnPointId, Transform>();
		foreach (var entry in _listSpawnPoint)
		{
			if (!_dicSpawnPoints.ContainsKey(entry.id))
			{
				_dicSpawnPoints.Add(entry.id, entry.point);
			}
		}	

	}
	private void Start()
	{
		InitializeMap();
	}

	private void InitializeMap()
	{
		//맵 안에서만 일어나는 것
		//NPC 스폰, 오브젝트 활성화 등
		//BGM 재생 등

		OnMapEnter();
	}

	protected virtual void OnMapEnter()
	{
		//맵 진입 시 처리 (적 스폰, 오브젝트 활성화 등)
		Debug.LogFormat("[Map Controller] Entered {0}", gameObject.name);
	}

	protected virtual void OnMapExit()
	{
		//맵 진입 시 처리 (적 스폰, 오브젝트 활성화 등)
		Debug.LogFormat("[Map Controller] Exiting {0}", gameObject.name);
	}

	//맵 전환 요청
	public void RequestMapTransition(SpawnPointId id)
	{
		if(_nextMapMc == null)
		{
			Debug.LogError("New MapBounds is null.");
			return;
		}

		GameManager.Instance?.RequestMapTransition(_nextMapMc, id);
	}

	//for debugging
	public Bounds GetCurrentMapBounds()
	{
		if(_mapBounds != null)
		{
			return _mapBounds._mapBound;
		}
		else
		{
			Debug.LogWarning("Current MapBounds is not assigned.");
			return default;
		}
	}

	public Transform GetSpawnPoint(SpawnPointId id)
	{
		if (_dicSpawnPoints.TryGetValue(id, out Transform spawnPoint))
		{
			return spawnPoint;
		}
		
		Debug.LogWarningFormat("[MapController] Spawn point with ID {0} not found. => Default", id);

		if(_dicSpawnPoints.TryGetValue(SpawnPointId.Default, out Transform fallback))
		{
			return fallback;
		}

		Debug.LogError("[MapController] Default spawn point not found.");
		return null;
	}
}
