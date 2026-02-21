using UnityEngine;

[CreateAssetMenu(fileName = "QuestData", menuName = "Game/Quest/Quest Data")]
public class QuestData : ScriptableObject
{
	[Header("Basic Info")]
	public string _questId;
	public string _questGiverNpcId;
	
	public string _title;
	[TextArea]
	public string _description;

	[Header("Quest Dialogue")]
	public string _startDialogueId;
	public string _progressDialogueId;
	public string _completedDialogueId;

	[Header("Quest Clear")]
	public string _questCompleterNpcId;
	public QuestGoalType _goalType;
	public int _goalCount;

	[Header("Reward")]
	public QuestReward _reward;

	[Header("Next Quest (optional)")]
	public string _nextQuestId;

	private void OnValidate()
	{
		// 0) null 방어 (Reward는 class라 null 가능)
		if (_reward == null)
		{
			_reward = new QuestReward();
			Debug.LogWarning($"[{name}] Reward가 null이라 기본 객체를 생성했습니다.", this);
		}

		// 1) 문자열 trim (선택: 안전한 자동보정)
		_questId = TrimOrEmpty(_questId);
		_questGiverNpcId = TrimOrEmpty(_questGiverNpcId);
		_title = TrimOrEmpty(_title);
		_description = TrimOrEmpty(_description);

		_startDialogueId = TrimOrEmpty(_startDialogueId);
		_progressDialogueId = TrimOrEmpty(_progressDialogueId);
		_completedDialogueId = TrimOrEmpty(_completedDialogueId);

		_questCompleterNpcId = TrimOrEmpty(_questCompleterNpcId);
		_nextQuestId = TrimOrEmpty(_nextQuestId);

		_reward._companionId = TrimOrEmpty(_reward._companionId);
		_reward._itemId = TrimOrEmpty(_reward._itemId);

		// 2) 필수값 검사 (로그 경고/오류)
		RequireNotEmpty(_questId, nameof(_questId));
		RequireNotEmpty(_questGiverNpcId, nameof(_questGiverNpcId));
		RequireNotEmpty(_title, nameof(_title));
		RequireNotEmpty(_questCompleterNpcId, nameof(_questCompleterNpcId));

		// 3) Goal 규칙
		if (_goalType == QuestGoalType.None)
		{
			Debug.LogError($"[{name}] {_goalType} 는 허용되지 않습니다. GoalType을 설정해주세요.", this);
		}

		if (_goalCount < 1)
		{
			// 자동보정 여부는 팀 정책에 따라 결정
			Debug.LogWarning($"[{name}] _goalCount 는 1 이상이어야 합니다. 현재값: {_goalCount}", this);
			// _goalCount = 1; // 자동보정 원하면 사용
		}

		// 4) 보상 수치 음수 방지 (자동보정은 비교적 안전)
		if (_reward._gold < 0)
		{
			Debug.LogWarning($"[{name}] Reward Gold가 음수여서 0으로 보정합니다. ({_reward._gold})", this);
			_reward._gold = 0;
		}

		if (_reward._exp < 0)
		{
			Debug.LogWarning($"[{name}] Reward Exp가 음수여서 0으로 보정합니다. ({_reward._exp})", this);
			_reward._exp = 0;
		}

		if (_reward._itemCount < 0)
		{
			Debug.LogWarning($"[{name}] Reward ItemCount가 음수여서 0으로 보정합니다. ({_reward._itemCount})", this);
			_reward._itemCount = 0;
		}

		// 5) 아이템 보상 조합 규칙 (핵심)
		bool hasItemId = !string.IsNullOrWhiteSpace(_reward._itemId);
		bool hasItemCount = _reward._itemCount > 0;

		if (!hasItemId && hasItemCount)
		{
			Debug.LogError($"[{name}] itemId 없이 itemCount만 설정되어 있습니다. itemId를 입력하거나 itemCount를 0으로 설정해주세요.", this);
		}

		if (hasItemId && !hasItemCount)
		{
			Debug.LogError($"[{name}] itemId가 있는데 itemCount가 0입니다. itemCount를 1 이상으로 설정해주세요.", this);
		}

		// 6) 다음 퀘스트 자기참조 방지
		if (!string.IsNullOrWhiteSpace(_nextQuestId) && _nextQuestId == _questId)
		{
			Debug.LogError($"[{name}] _nextQuestId가 자기 자신(_questId)과 같습니다. 자기참조는 허용하지 않습니다.", this);
		}

		// 7) 콘텐츠 품질 경고 (선택)
		if (string.IsNullOrWhiteSpace(_description))
		{
			Debug.LogWarning($"[{name}] _description 이 비어 있습니다.", this);
		}

		if (string.IsNullOrWhiteSpace(_completedDialogueId))
		{
			Debug.LogWarning($"[{name}] _completedDialogueId 가 비어 있습니다.", this);
		}
	}

	private static string TrimOrEmpty(string value)
	{
		return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
	}

	private void RequireNotEmpty(string value, string fieldName)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			Debug.LogError($"[{name}] 필수 필드 누락: {fieldName}", this);
		}
	}
}
}
