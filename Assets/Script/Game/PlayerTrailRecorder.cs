using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTrailRecorder : MonoBehaviour
{
	public static PlayerTrailRecorder Instance { get; private set; }
	public Queue<Vector3> TrailQue { get; private set; } = new Queue<Vector3>();

	[SerializeField] float recordInterval = 0.5f;
	private float _timer = 0f;
	private Transform _player_;

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
		_player_ = GetComponent<PlayerController>().transform;
	}

	private void Update()
	{
		_timer += Time.deltaTime;
		if (_timer >= recordInterval)
		{
			_timer = 0f;

			TrailQue.Enqueue(_player_.position); // 현재 플레이어 위치를 기록

			while (TrailQue.Count > 50) // 최대 50개의 위치만 유지 (너무 오래된 것은 삭제)
			{
				TrailQue.Dequeue();
			}
		}

	}

}
