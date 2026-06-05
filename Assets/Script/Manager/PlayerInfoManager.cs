using System;
using System.Net.Http;
using UnityEngine;

public struct PlayerInfo
{
	public string _name;
	public int _level; //레벨에 따라서 뒤에 매달고 다니는 동료 숫자에 제한이 있음
	public int _exp;
	public int _gold;
	public int _hp;


	public PlayerInfo(string name, int level, int experience, int gold, int hp)
	{
		_name = name;
		_level = level;
		_exp = experience;
		_gold = gold;
		_hp = hp;
	}

}

/// <summary>
/// 플레이어 정보 관리 싱글톤
/// </summary>
public class PlayerInfoManager : MonoBehaviour
{
    public static PlayerInfoManager Instance { get; private set; }

	//이벤트
	public Action<int> OnGoldChanged;
	public Action<int> OnLevelChanged;
	public Action<int> OnExpChanged;
	public Action<int> OnHpChanged;
	public Action OnPlayerDead;
	public Action<int> OnAtkChanged;


	private PlayerInfo _playerInfo = new PlayerInfo("Noah", 1, 0, 0,100);

	public PlayerInfo PlayerInfo => _playerInfo;

	public int RequestExp => 100 * _playerInfo._level; //레벨에 따른 필요한 경험치 계산 (임시)
	public int MaxHp => 100 + (_playerInfo._level - 1) * 10; // 레벨당 +10 (임시)
	public bool IsDead => _playerInfo._hp <= 0;

	[SerializeField] private int _baseAtk = 3;
	public int Atk
	{
		get
		{
			int bonus = 0;
			if(InventoryManager.Instance != null)
			{
				ItemData weapon = InventoryManager.Instance.GetEquippedWeaponData();
				if(weapon != null)
				{
					bonus = weapon._effectValue; //무기의 공격력
				}
			}
			return _baseAtk + bonus;
		}
	}

	
	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		DontDestroyOnLoad(gameObject);

	}

	private void Start()
	{
		//인벤토리 매니저에 장비 변경 이벤트 구독
		if(InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnEquipChanged += HandleEquipChanged;
		}
	}

	private void OnDestroy()
	{
		//인벤토리 매니저 이벤트 구독 해제
		if(InventoryManager.Instance != null)
		{
			InventoryManager.Instance.OnEquipChanged -= HandleEquipChanged;
		}

	}

	private void HandleEquipChanged()
	{
		OnAtkChanged?.Invoke(Atk);
	}

	public PlayerInfoSaveData GetSaveData()
	{
		return new PlayerInfoSaveData
		{
			name = _playerInfo._name,
			level = _playerInfo._level,
			exp = _playerInfo._exp,
			gold = _playerInfo._gold,
			hp = _playerInfo._hp
		};
	}

	public void ApplyData(PlayerInfoSaveData data)
	{
		_playerInfo._name = data.name;
		_playerInfo._level = Mathf.Max(1, data.level);
		_playerInfo._exp = data.exp;
		_playerInfo._gold = data.gold;
		_playerInfo._hp = data.hp;

		//적용
		OnGoldChanged?.Invoke(_playerInfo._gold);
		OnLevelChanged?.Invoke(_playerInfo._level);
		OnExpChanged?.Invoke(_playerInfo._exp);
		OnHpChanged?.Invoke(_playerInfo._hp);
	}

	public int GetMaxCompanionCount()
	{
		//레벨에 따른 최대 동료 수 계산
		return _playerInfo._level / 2; //레벨 2마다 동료 1명 추가
	}

	public void AddGold(int amount)
	{
		if(amount <= 0)
		{
			return;
		}

		_playerInfo._gold += amount;
		OnGoldChanged?.Invoke(_playerInfo._gold);
	}

	public bool SpendGold(int amount)
	{
		if (amount <= 0 || _playerInfo._gold < amount)
			return false;

		_playerInfo._gold -= amount;
		OnGoldChanged?.Invoke(_playerInfo._gold);
		return true;
	}

	public void AddExp(int amount)
	{
		if(amount <= 0)
		{
			return;
		}

		_playerInfo._exp += amount;
		OnExpChanged?.Invoke(_playerInfo._exp);

		CheckLevelUp();
	}

	public void AddLevel(int amount)
	{
		if(amount <= 0)
		{
			return;
		}

		_playerInfo._level += amount;
		OnLevelChanged?.Invoke(_playerInfo._level);
	}

	public void SetGold(int amount)
	{
		_playerInfo._gold = Mathf.Max(0, amount);
		OnGoldChanged?.Invoke(_playerInfo._gold);
	}

	public void SetLevel(int level)
	{
		_playerInfo._level = Mathf.Max(1, level);
		OnLevelChanged?.Invoke(_playerInfo._level);
	}

	public void SetBaseAtk(int atk)
	{
		_baseAtk = Mathf.Max(0, atk);
		OnAtkChanged?.Invoke(Atk);
	}

	public void TakeDamage(int damage)
	{
		if (damage <= 0 || IsDead)
			return;

		_playerInfo._hp -= damage;
		if(_playerInfo._hp < 0)
			_playerInfo._hp = 0;

		Debug.Log($"[Player] 피격: {damage} (HP: {_playerInfo._hp}/{MaxHp})");
		OnHpChanged?.Invoke(_playerInfo._hp);

		if(IsDead)
		{
			OnPlayerDead?.Invoke();
		}
	}

	//부활
	public void RestoreHp()
	{
		_playerInfo._hp = MaxHp;
		OnHpChanged?.Invoke(_playerInfo._hp);
	}

	public void AddHp(int amount)
	{
		if (amount <= 0) return;
		_playerInfo._hp = Mathf.Min(_playerInfo._hp + amount, MaxHp);
		OnHpChanged?.Invoke(_playerInfo._hp);
	}

	public void CheckLevelUp()
	{
		int levelUps = 0;
		while (_playerInfo._level > 0 && _playerInfo._exp >= RequestExp)
		{
			_playerInfo._exp -= RequestExp;
			_playerInfo._level++;
			levelUps++;
		}
		if (levelUps > 0)
		{
			OnLevelChanged?.Invoke(_playerInfo._level);
			OnExpChanged?.Invoke(_playerInfo._exp);
		}
	}
}
