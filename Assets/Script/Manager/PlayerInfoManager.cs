using System;
using UnityEngine;

public struct PlayerInfo
{
	public int _level; //레벨에 따라서 뒤에 매달고 다니는 동료 숫자에 제한이 있음
	public int _exp;
	public int _gold;


	public PlayerInfo(int level, int experience, int gold)
	{
		_level = level;
		_exp = experience;
		_gold = gold;
	}

}

public class PlayerInfoManager : MonoBehaviour
{
    public static PlayerInfoManager Instance { get; private set; }

	public Action<int> OnGoldChanged;
	public Action<int> OnLevelChanged;
	public Action<int> OnExpChanged;

	private PlayerInfo _playerInfo = new PlayerInfo(1, 0, 0);


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

	public int GetMaxCompanionCount()
	{
		//레벨에 따른 최대 동료 수 계산
		return _playerInfo._level / 2; //레벨 2마다 동료 1명 추가
	}

	public void AddGold(int amount)
	{
		_playerInfo._gold += amount;
		OnGoldChanged?.Invoke(_playerInfo._gold);
	}

	public void AddExp(int amount)
	{
		_playerInfo._exp += amount;
		OnExpChanged?.Invoke(_playerInfo._exp);

		CheckLevelUp();
	}

	public void AddLevel(int amount)
	{
		_playerInfo._level += amount;
		OnLevelChanged?.Invoke(_playerInfo._level);
	}

	public void CheckLevelUp() { }
}
