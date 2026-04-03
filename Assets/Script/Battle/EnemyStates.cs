using UnityEngine;

// ============================================================
// Idle 상태 (대기 + 순찰 통합)
// - 순찰 포인트가 있으면 순찰, 없으면 제자리 대기
// - 감지 범위에 플레이어 진입 시 Chase로 전환
// ============================================================
public class EnemyIdleState : IEnemyState
{
	private readonly EnemyFSM _fsm;
	private readonly EnemyController _controller;

	private int _currentPatrolIndex;
	private float _patrolWaitTimer;
	private readonly float _patrolWaitDuration = 1f;
	private bool _isWaiting;

	public EnemyIdleState(EnemyFSM fsm, EnemyController controller)
	{
		_fsm = fsm;
		_controller = controller;
	}

	public void Enter()
	{
		_controller.StopMove();
		_controller.Anim.PlayAnimation("idle");
		_isWaiting = false;
		_patrolWaitTimer = 0f;
	}

	public void Update()
	{
		// 감지 범위 체크
		if (_fsm.GetDistanceToTarget() <= _fsm.DetectRange)
		{
			_fsm.ChangeState(_fsm.ChaseState);
			return;
		}

		// 순찰 포인트가 있으면 순찰
		if (_fsm.PatrolPoints != null && _fsm.PatrolPoints.Count > 0)
		{
			UpdatePatrol();
		}
	}

	public void Exit() { }

	private void UpdatePatrol()
	{
		if (_isWaiting)
		{
			_patrolWaitTimer += Time.deltaTime;
			if (_patrolWaitTimer >= _patrolWaitDuration)
			{
				_isWaiting = false;
				_currentPatrolIndex = (_currentPatrolIndex + 1) % _fsm.PatrolPoints.Count;
			}
			return;
		}

		Transform target = _fsm.PatrolPoints[_currentPatrolIndex];
		Vector2 direction = (target.position - _controller.transform.position).normalized;
		_controller.MoveTo(direction);
		_controller.Anim.PlayAnimation("run");
		_controller.Anim.SetFacing(direction);

		float distance = Vector2.Distance(_controller.transform.position, target.position);

		if (distance < 0.2f)
		{
			_controller.StopMove();
			_controller.Anim.PlayAnimation("idle");
			_isWaiting = true;
			_patrolWaitTimer = 0f;
		}
	}
}

// ============================================================
// Chase 상태 (추적)
// - 플레이어를 향해 이동
// - 공격 범위 도달 시 Attack으로 전환
// - 감지 범위 이탈 시 Return으로 전환
// ============================================================
public class EnemyChaseState : IEnemyState
{
	private readonly EnemyFSM _fsm;
	private readonly EnemyController _controller;

	public EnemyChaseState(EnemyFSM fsm, EnemyController controller)
	{
		_fsm = fsm;
		_controller = controller;
	}

	public void Enter() { }

	public void Update()
	{
		//Debug.Log($"[ChaseState] dist: {_fsm.GetDistanceToTarget()}");

		// 대상 사망 시 복귀
		var targetDamageable = _fsm.TargetDamageable;
		if (targetDamageable == null || targetDamageable.IsDead)
		{
			_fsm.ChangeState(_fsm.ReturnState);
			return;
		}

		float distance = _fsm.GetDistanceToTarget();

		// 공격 범위 도달
		if (distance <= _fsm.AttackRange)
		{
			_fsm.ChangeState(_fsm.AttackState);
			return;
		}

		// 감지 범위 이탈
		if (distance > _fsm.DetectRange)
		{
			_fsm.ChangeState(_fsm.ReturnState);
			return;
		}

		// 플레이어 방향으로 이동
		Vector2 direction = (_fsm.Target.position - _controller.transform.position).normalized;
		_controller.MoveTo(direction);
		_controller.Anim.PlayAnimation("run");
		_controller.Anim.SetFacing(direction);
	}

	public void Exit()
	{
		_controller.StopMove();
	}
}

// ============================================================
// Attack 상태 (공격)
// - 쿨타임 기반 공격 실행
// - 대상이 공격 범위 밖으로 나가면 Chase로 전환
// - 대상이 감지 범위 밖이면 Return으로 전환
// ============================================================
public class EnemyAttackState : IEnemyState
{
	private readonly EnemyFSM _fsm;
	private readonly EnemyController _controller;
	private float _lastAttackTime;

	public EnemyAttackState(EnemyFSM fsm, EnemyController controller)
	{
		_fsm = fsm;
		_controller = controller;
	}

	public void Enter()
	{
		_controller.StopMove();
		// 진입 즉시 공격 가능하도록 쿨타임 초기화
		_lastAttackTime = -_fsm.AttackCooldown;
	}

	public void Update()
	{
		//Debug.Log($"[AttackState] dist: {_fsm.GetDistanceToTarget()}, detectRange: {_fsm.DetectRange}");
		float distance = _fsm.GetDistanceToTarget();

		//대상 사망 시 복귀
		var targetDamageable = _fsm.TargetDamageable;
		if(targetDamageable == null || targetDamageable.IsDead)
		{
			_fsm.ChangeState(_fsm.ReturnState);
			return;
		}

		// 감지 범위 이탈
		if (distance > _fsm.DetectRange)
		{
			_fsm.ChangeState(_fsm.ReturnState);
			return;
		}

		// 공격 범위 이탈 → 다시 추적
		if (distance > _fsm.AttackRange)
		{
			_fsm.ChangeState(_fsm.ChaseState);
			return;
		}

		// 쿨타임 체크 → 공격
		if (Time.time - _lastAttackTime >= _fsm.AttackCooldown)
		{
			_lastAttackTime = Time.time;

			_controller.Anim.PlayAnimation("attack", false);
			_controller.Attack(_fsm.Target);
		}
	}

	public void Exit() { }
}

// ============================================================
// Hit 상태 (피격 경직)
// - 넉백 + 일정 시간 경직
// - 경직 해제 후 Chase 또는 Idle로 복귀
// ============================================================
public class EnemyHitState : IEnemyState
{
	private readonly EnemyFSM _fsm;
	private readonly EnemyController _controller;
	private float _stunTimer;

	public EnemyHitState(EnemyFSM fsm, EnemyController controller)
	{
		_fsm = fsm;
		_controller = controller;
	}

	public void Enter()
	{
		_stunTimer = 0f;
		_controller.StopMove();
		_controller.Anim.PlayAnimation("hit", false);

		// 타겟 반대 방향으로 넉백
		if (_fsm.Target != null)
		{
			Vector2 knockbackDir = (_controller.transform.position - _fsm.Target.position).normalized;
			_controller.ApplyKnockback(knockbackDir, _fsm.KnockbackForce);
		}
	}

	public void Update()
	{
		_stunTimer += Time.deltaTime;
		//Debug.Log($"[HitState] stunTimer: {_stunTimer} / {_fsm.HitStunDuration}");
		if (_stunTimer >= _fsm.HitStunDuration)
		{
			// 경직 해제 → 감지 범위 내면 Chase, 밖이면 Return
			if (_fsm.GetDistanceToTarget() <= _fsm.DetectRange)
				_fsm.ChangeState(_fsm.ChaseState);
			else
				_fsm.ChangeState(_fsm.ReturnState);
		}
	}

	public void Exit() { }
}

// ============================================================
// Return 상태 (원위치 복귀)
// - 원래 위치로 이동
// - 도착하면 Idle로 전환
// - 복귀 중 플레이어 재감지 시 Chase로 전환
// ============================================================
public class EnemyReturnState : IEnemyState
{
	private readonly EnemyFSM _fsm;
	private readonly EnemyController _controller;

	public EnemyReturnState(EnemyFSM fsm, EnemyController controller)
	{
		_fsm = fsm;
		_controller = controller;
	}

	public void Enter() { }

	public void Update()
	{
		// 복귀 중 플레이어 재감지
		if (_fsm.GetDistanceToTarget() <= _fsm.DetectRange)
		{
			_fsm.ChangeState(_fsm.ChaseState);
			return;
		}

		// 원래 위치로 이동
		Vector2 direction = (_fsm.OriginPosition - _controller.transform.position).normalized;
		_controller.MoveTo(direction);
		_controller.Anim.PlayAnimation("run");
		_controller.Anim.SetFacing(direction);

		// 도착 체크
		float distance = Vector2.Distance(_controller.transform.position, _fsm.OriginPosition);
		if (distance < 0.2f)
		{
			_fsm.ChangeState(_fsm.IdleState);
		}
	}

	public void Exit()
	{
		_controller.StopMove();
	}

	// ============================================================
	// Die 상태 (사망처리)
	// ============================================================
	public class EnemyDieState : IEnemyState
	{
		private EnemyFSM _fsm;
		private EnemyController _controller;

		public EnemyDieState(EnemyFSM fsm, EnemyController controller)
		{
			_fsm = fsm;
			_controller = controller;
		}

		public void Enter() {
			_controller.Anim.PlayAnimation("die", false);
		}
		public void Update() { }  // 아무것도 안 함 — 상태 전환 없음
		public void Exit() { }
	}
}