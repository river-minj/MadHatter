using UnityEngine;

public class EnemyRespawner : MonoBehaviour
{
    [SerializeField] private string _enemyPrefabPath;
    [SerializeField] private float _respawnDelay = 5f;
    [SerializeField] private string _linkedQuestId;

    private Vector3 _spawnPosition;

    private void Awake()
    {
        _spawnPosition = transform.position;
        var enemy = GetComponent<EnemyController>();
        if (enemy != null)
            enemy.OnDeath += HandleDeath;
    }

    private void HandleDeath(EnemyController _)
    {
        if (!string.IsNullOrEmpty(_linkedQuestId) &&
            QuestManager.Instance != null &&
            QuestManager.Instance.IsQuestCompleted(_linkedQuestId))
            return;

        var map = GameManager.Instance?.CurrentMapController;
        if (map != null)
            map.RequestRespawn(_enemyPrefabPath, _spawnPosition, _respawnDelay);
    }
}
