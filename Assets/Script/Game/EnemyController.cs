using System;
using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
	[SerializeField] private string _enemyId;
	[SerializeField] private int _maxHp = 10;
	[SerializeField] private int _currentHp;

	private SpineAnimator _spineAnimator;
	public SpineAnimator Anim => _spineAnimator;

	private EnemyFSM _fsm;
	private Rigidbody2D _rb;

	public string EnemyId => _enemyId;
	public bool IsDead => _currentHp <= 0;

	public event Action<EnemyController> OnDeath;

	private void Awake()
	{
		_currentHp = _maxHp;

		_rb = GetComponent<Rigidbody2D>();
		_fsm = GetComponent<EnemyFSM>();
		_spineAnimator = GetComponent<SpineAnimator>();
	}

	private void Start()
	{
		if (_fsm != null)
			_fsm.Init(this);
	}

	// ============================================================
	// IDamageable - HP 감소만, 행동 판단은 FSM에 위임
	// ============================================================
	public void TakeDamage(int damage)
	{
		if (IsDead) return;

		_currentHp -= damage;
		Debug.Log($"[Enemy] {_enemyId} 피격: {damage} (HP: {_currentHp}/{_maxHp})");

		if (IsDead)
		{
			Die();
		}
		else
		{
			_fsm.OnDamaged(_currentHp);
		}
	}

	// ============================================================
	// 실행 메서드 - State에서 호출
	// ============================================================

	/// <summary>
	/// 지정 방향으로 이동
	/// </summary>
	public void MoveTo(Vector2 direction)
	{
		if (_rb == null) return;
		_rb.velocity = direction * _fsm.MoveSpeed;

		// TODO: 이동 애니메이션 파라미터 설정
		// _animator.SetFloat("MoveX", direction.x);
		// _animator.SetFloat("MoveY", direction.y);
	}

	/// <summary>
	/// 이동 정지
	/// </summary>
	public void StopMove()
	{
		if (_rb == null) return;
		_rb.velocity = Vector2.zero;

		_spineAnimator.PlayAnimation("idle");
	}

	/// <summary>
	/// 넉백 적용
	/// </summary>
	public void ApplyKnockback(Vector2 direction, float force)
	{
		if (_rb == null) return;
		_rb.velocity = Vector2.zero;
		_rb.AddForce(direction * force, ForceMode2D.Impulse);
	}

	/// <summary>
	/// 공격 실행
	/// </summary>
	public void Attack(Transform target)
	{
		if (target == null) return;

		// TODO: 공격 애니메이션 트리거
		// _animator.SetTrigger("Attack");

		//대상에게 데미지 적용 (IDamageable 구현 후)
		var damageable = _fsm.TargetDamageable;
		if (damageable != null && !damageable.IsDead)
		{
			damageable?.TakeDamage(_fsm.AttackDamage);
		}
	}

	// ============================================================
	// 사망 처리 - Controller가 직접 실행
	// ============================================================
	private void Die()
	{
		Debug.Log($"[Enemy] {_enemyId} 사망");

		_fsm.ChangeState(_fsm.DieState);
		_spineAnimator.DisableAutoIdle();

		OnDeath?.Invoke(this);
		QuestManager.Instance.ReportKill(_enemyId);

		string droppedItemId = DropDatabase.Instance.RollDrop(_enemyId);
		Debug.Log($"[Drop] enemyId: {_enemyId}, droppedItemId: {droppedItemId ?? "없음"}");
		if (!string.IsNullOrEmpty(droppedItemId))
		{
			GameObject prefab = Resources.Load<GameObject>("Prefab/DroppedItem/DroppedItem");
			if (prefab != null)
			{
				GameObject dropObj = Instantiate(prefab, transform.position, Quaternion.identity);
				DroppedItemController dic = dropObj.GetComponent<DroppedItemController>();
				if (dic != null)
				{
					dic.Init(droppedItemId);
				}
			}
		}

		Destroy(gameObject, 1.0f);
	}
}
