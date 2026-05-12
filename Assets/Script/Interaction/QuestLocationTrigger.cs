using UnityEngine;

/// <summary>
/// 맵 프리팹에 배치하는 도달 퀘스트 트리거.
/// Collider2D(isTrigger=true)와 함께 사용. Inspector에서 _locationId 입력.
/// </summary>
public class QuestLocationTrigger : MonoBehaviour
{
    [SerializeField] private string _locationId;

    private bool _triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_triggered) return;
        if (!other.CompareTag("Player")) return;
        if (string.IsNullOrEmpty(_locationId)) return;

        _triggered = true;
        QuestManager.Instance.ReportReach(_locationId);
    }
}
