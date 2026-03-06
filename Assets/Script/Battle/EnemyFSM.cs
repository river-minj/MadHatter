using UnityEngine;
using System.Collections.Generic;

public class EnemyFSM : MonoBehaviour
{
	[Header("감지/공격 범위")]
	[SerializeField] private float _detectRange = 7f;
	[SerializeField] private float _attackRange = 1.5f;

	[Header("이동")]
	[SerializeField] private float _moveSpeed = 2f;

	[Header("공격")]
	[SerializeField] private int _attackDamage = 3;
	[SerializeField] private float _attackCooldown = 1.0f;

	[Header("넉백/경직")]
	[SerializeField] private float _knockbackForce = 3f;
	[SerializeField] private float _hitStunDuration = 0.3f;

	[Header("순찰 (선택)")]
	[SerializeField] private List<Transform> _patrolPoints;

	// 공유 데이터 (State에서 읽기용)
	public float DetectRange => _detectRange;
	public float AttackRange => _attackRange;
	public float MoveSpeed => _moveSpeed;
	public int AttackDamage => _attackDamage;
	public float AttackCooldown => _attackCooldown;
	public float KnockbackForce => _knockbackForce;
	public float HitStunDuration => _hitStunDuration;
	public List<Transform> PatrolPoints => _patrolPoints;
	public Vector3 OriginPosition { get; private set; }
	public Transform Target { get; private set; }

	// 상태 인스턴스 (State에서 전환용)
	public IEnemyState IdleState { get; private set; }
	public IEnemyState ChaseState { get; private set; }
	public IEnemyState AttackState { get; private set; }
	public IEnemyState HitState { get; private set; }
	public IEnemyState ReturnState { get; private set; }

	private IEnemyState _currentState;

	private void Awake()
	{
		OriginPosition = transform.position;
	}

	/// <summary>
	/// EnemyController에서 호출. State 생성 시 Controller 참조가 필요하므로 외부 초기화.
	/// </summary>
	public void Init(EnemyController controller)
	{
		IdleState = new EnemyIdleState(this, controller);
		ChaseState = new EnemyChaseState(this, controller);
		AttackState = new EnemyAttackState(this, controller);
		HitState = new EnemyHitState(this, controller);
		ReturnState = new EnemyReturnState(this, controller);

		// 플레이어를 타겟으로 설정
		GameObject player = GameObject.FindGameObjectWithTag("Player");
		if (player != null)
			Target = player.transform;

		ChangeState(IdleState);
	}

	private void Update()
	{
		_currentState?.Update();
	}

	public void ChangeState(IEnemyState newState)
	{
		_currentState?.Exit();
		_currentState = newState;
		_currentState?.Enter();
	}

	/// <summary>
	/// EnemyController.TakeDamage에서 호출.
	/// HP 정보를 받아 사망/피격 상태를 결정.
	/// </summary>
	public void OnDamaged(int currentHp)
	{
		if (currentHp <= 0)
		{
			// 사망은 FSM 상태 전환 없이 Controller가 직접 처리
			return;
		}

		ChangeState(HitState);
	}

	/// <summary>
	/// 타겟과의 거리 반환 (State에서 공용 사용)
	/// </summary>
	public float GetDistanceToTarget()
	{
		if (Target == null) return float.MaxValue;
		return Vector2.Distance(transform.position, Target.position);
	}
}