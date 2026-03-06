using UnityEngine;
using System;

public class EnemyController : MonoBehaviour, IDamageable
{
	[SerializeField] private string _enemyId;
	[SerializeField] private int _maxHp = 10;
	[SerializeField] private int _currentHp;

	public string EnemyId => _enemyId;
	public bool IsDead => _currentHp <= 0;

	public event Action<EnemyController> OnDeath;

	private void Awake()
	{
		_currentHp = _maxHp;
	}

	public void TakeDamage(int damage)
	{
		if (IsDead) return;

		_currentHp -= damage;
		Debug.Log($"[Enemy] {_enemyId} 피격: {damage} (HP: {_currentHp}/{_maxHp})");

		if (IsDead)
		{
			Die();
		}
	}

	private void Die()
	{
		Debug.Log($"[Enemy] {_enemyId} 사망");
		OnDeath?.Invoke(this);
		QuestManager.Instance.ReportKill(_enemyId);

		// TODO: 사망 애니메이션/이펙트 후 Destroy
		Destroy(gameObject, 0.5f);
	}
}