using UnityEngine;

// 맵 전환 트리거 (문, 포탈 등)
// 맵 변경 플로우: MapTransistor → GameManager
// _nextMapMc: 이 문이 연결하는 목적지 맵 프리팹 (Inspector에서 직접 설정)
// _spawnPointId: 목적지 맵의 스폰 위치
public class MapTransistor : MonoBehaviour
{
	[SerializeField] private MapController _nextMapMc;
	[SerializeField] private SpawnPointId _spawnPointId;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (!other.CompareTag("Player")) return;

		if (_nextMapMc == null)
		{
			Debug.LogWarning("[MapTransistor] _nextMapMc이 할당되지 않았습니다.");
			return;
		}

		GameManager.Instance?.RequestMapTransition(_nextMapMc, _spawnPointId);
	}
}
