using System.Collections.Generic;
using UnityEngine;


public struct TrailPoint
{
	public Vector3 position;
	public Vector3 direction;
	public TrailPoint(Vector3 pos, Vector3 dir)
	{
		position = pos;
		direction = dir;
	}
}

//목적 : 플레이어의 이동 경로를 일정 거리마다 기록하여 동료들이 같은 경로로 따라오게 함
public class PlayerTrailRecorder : MonoBehaviour
{
	public static PlayerTrailRecorder Instance { get; private set; }

	[SerializeField] private float _recordDistance = 0.15f; //이 만큼 이동할 때마다 기록
	[SerializeField] private int _maxTrailCount = 1000; //최대 기록 개수

	private  List<TrailPoint> _listTrail = new List<TrailPoint>();
	private Vector3 _lastRecordedPos;

	private PlayerController _player_;

	public TrailPoint GetPoint(int index)
	{
		index = Mathf.Clamp(index, 0, _listTrail.Count - 1);
		return _listTrail[index];
	}
	public int Count => _listTrail.Count;

	public TrailPoint GetLatestPoint()
	{
		if (_listTrail.Count == 0)
			return new TrailPoint(transform.position, Vector3.down);
		return _listTrail[_listTrail.Count - 1];
	}

	public void ResetToPosition(Vector3 position)
	{
		_listTrail.Clear();
		var point = new TrailPoint(position, Vector3.down);
		for (int i = 0; i < _maxTrailCount; i++)
			_listTrail.Add(point);
		_lastRecordedPos = position;
	}

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
		_player_ = GetComponent<PlayerController>();

		_lastRecordedPos = _player_.transform.position;

		//초기 위치
		_listTrail.Add(new TrailPoint(_lastRecordedPos, Vector3.down));
	}

	private void Update()
	{
		if (_player_ == null)
			return;

		Vector3 currentPos = _player_.transform.position;
		float dist = Vector3.Distance(currentPos, _lastRecordedPos);
		
		if(dist >= _recordDistance)
		{
			Vector3 moveDir = (currentPos - _lastRecordedPos).normalized;
			Vector3 snappedDir = SnapToCardinal(moveDir);

			_listTrail.Add( new TrailPoint(currentPos, moveDir) );
			_lastRecordedPos = currentPos;

			//최대 개수 초과 시 오래된 기록 삭제
			if(_listTrail.Count > _maxTrailCount)
			{
				_listTrail.RemoveAt(0);
			}
		}
	}

	private Vector3 SnapToCardinal(Vector3 dir)
	{
		if(Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
		{
			return dir.x > 0 ? Vector3.right : Vector3.left; //수평 방향
		}
		else
		{
			return dir.y > 0 ? Vector3.up : Vector3.down; //수직 방향
		}
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (_listTrail == null || _listTrail.Count < 2)
			return;

		Gizmos.color = Color.cyan;
		for (int i = 1; i < _listTrail.Count; i++)
		{
			Gizmos.DrawLine(_listTrail[i - 1].position, _listTrail[i].position);
		}
	}
#endif
}
