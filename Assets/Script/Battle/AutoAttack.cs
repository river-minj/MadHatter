using UnityEngine;

public class AutoAttack : MonoBehaviour
{
	[SerializeField] private float _attackRange = 1.5f;
	[Tooltip("실제로 공격이 발동되는 근접 거리. AttackRange보다 작아야 하며, 이 거리 안까지 다가와야 실제로 공격함")]
	[SerializeField] private float _meleeRange = 0.6f;
	[SerializeField] private int _attackDamage = 3;
	[SerializeField] private float _attackCooldown = 1.0f;
	[SerializeField] private LayerMask _enemyLayer;

	[Header("Debug")]
	[SerializeField, ReadOnly] private int _finalAttackDamage;

	private float _lastAttackTime; //마지막 공격 시간 - 쿨타임 계산용
	private IAnimator _spineAnimator;
	private PlayerController _playerController; // Player 태그일 때만 존재, 공격 방향이 이동 입력에 덮어써지지 않도록 처리


	private void Awake()
	{
		_spineAnimator = GetComponent<IAnimator>();
		_playerController = GetComponent<PlayerController>();
	}

	private void Update()
	{
		//움직임이 잠겨있으면 공격 불가
		if (GameManager.Instance != null && GameManager.Instance.IsInputLock)
			return;

		//쿨타임 체크
		if (Time.time < _lastAttackTime + _attackCooldown)
			return;

		EnemyController target = FindClosestEnemy();
		if (target == null)
			return;

		// 근접 거리 밖이면 아직 공격하지 않음 (허공에 헛스윙 방지, 플레이어가 더 다가와야 함)
		float distance = Vector2.Distance(transform.position, target.transform.position);
		if (distance > _meleeRange)
			return;

		Attack(target);
	}

	private EnemyController FindClosestEnemy()
	{
		//자신 위치 중심으로 반경 원형 탐색 + 마스크로 enemy 레이어만 탐색 + 범위 내의 모든 적 collider을 반환
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

		int finalDamage = GetFinalDamage();
		_finalAttackDamage = finalDamage;
		target.TakeDamage(finalDamage);

		Debug.Log($"[AutoAttack] {gameObject.name} → {target.EnemyId} 공격 ({_attackDamage} dmg)");

		if (_spineAnimator != null)
		{
			// 공격 발동 직전 타겟 방향으로 시선 갱신
			Vector2 facingDirection = (target.transform.position - transform.position).normalized;

			if (_playerController != null)
				_playerController.FaceDirection(facingDirection); // PlayerController의 이동 기반 facing에 덮어써지지 않도록 _lastDir까지 갱신
			else
				_spineAnimator.SetFacing(facingDirection);

			_spineAnimator.PlayAnimation("attack", false);
		}
	}

	private int GetFinalDamage()
	{
		if(CompareTag("Player"))
		{
			//플레이엉의	공력은 레벨과 장비에 따라 달라질 수 있으므로 PlayerInfoManager에서 가져옴
			return PlayerInfoManager.Instance.Atk;
		}

		//기타 다른 모든 공격가능 객체의 기본 공격력
		return _attackDamage;
	}

#if UNITY_EDITOR
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, _attackRange);

		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, _meleeRange);
	}
#endif
}