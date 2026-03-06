using UnityEngine;

public class AutoAttack : MonoBehaviour
{
	[SerializeField] private float _attackRange = 1.5f;
	[SerializeField] private int _attackDamage = 3;
	[SerializeField] private float _attackCooldown = 1.0f;
	[SerializeField] private LayerMask _enemyLayer;

	private float _lastAttackTime;

	private void Update()
	{
		if (GameManager.Instance != null && GameManager.Instance.IsInputLock)
			return;

		if (Time.time < _lastAttackTime + _attackCooldown)
			return;

		EnemyController target = FindClosestEnemy();
		if (target == null)
			return;

		Attack(target);
	}

	private EnemyController FindClosestEnemy()
	{
		Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, _attackRange, _enemyLayer);

		EnemyController closest = null;
		float closestDist = float.MaxValue;

		foreach (var hit in hits)
		{
			var enemy = hit.GetComponent<EnemyController>();
			if (enemy == null || enemy.IsDead)
				continue;

			float dist = Vector2.Distance(transform.position, hit.transform.position);
			if (dist < closestDist)
			{
				closestDist = dist;
				closest = enemy;
			}
		}

		return closest;
	}

	private void Attack(EnemyController target)
	{
		_lastAttackTime = Time.time;
		target.TakeDamage(_attackDamage);

		Debug.Log($"[AutoAttack] {gameObject.name} → {target.EnemyId} 공격 ({_attackDamage} dmg)");

		// TODO: 공격 애니메이션 트리거
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _attackRange);
	}
#endif
}