using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudUI : MonoBehaviour
{
	[Header("HP")]
	[SerializeField] private Image _hpSlider;
	[SerializeField] private TextMeshProUGUI _hpText;

	[Header("EXP")]
	[SerializeField] private Image _expSlider;
	[SerializeField] private TextMeshProUGUI _expText;

	[Header("Info")]
	[SerializeField] private TextMeshProUGUI _levelText;
	[SerializeField] private TextMeshProUGUI _goldText;
	[SerializeField] private TextMeshProUGUI _atkText;

	private void Start()
	{
		Subscribe();
		RefreshUI();
	}

	private void OnDestroy()
	{
		Unsubscribe();
	}

	private void Subscribe()
	{
		PlayerInfoManager.Instance.OnHpChanged += UpdateHp;
		PlayerInfoManager.Instance.OnExpChanged += UpdateExp;
		PlayerInfoManager.Instance.OnLevelChanged += UpdateLevel;
		PlayerInfoManager.Instance.OnGoldChanged += UpdateGold;
		PlayerInfoManager.Instance.OnAtkChanged += UpdateAtk;

	}

	private void Unsubscribe()
	{
		if (PlayerInfoManager.Instance == null)
			return;

		PlayerInfoManager.Instance.OnHpChanged -= UpdateHp;
		PlayerInfoManager.Instance.OnExpChanged -= UpdateExp;
		PlayerInfoManager.Instance.OnLevelChanged -= UpdateLevel;
		PlayerInfoManager.Instance.OnGoldChanged -= UpdateGold;
		PlayerInfoManager.Instance.OnAtkChanged -= UpdateAtk;
	}

	private void RefreshUI()
	{
		var info = PlayerInfoManager.Instance.PlayerInfo;
		UpdateHp(info._hp);
		UpdateExp(info._exp);
		UpdateLevel(info._level);
		UpdateGold(info._gold);
		UpdateAtk(PlayerInfoManager.Instance.Atk);

	}

	private void UpdateHp(int hp)
	{
		int maxHp = PlayerInfoManager.Instance.MaxHp;
		_hpSlider.fillAmount = (float)hp / maxHp;
		_hpText.text = $"{hp} / {maxHp}";

	}

	private void UpdateExp(int exp)
	{
		int requiredExp = PlayerInfoManager.Instance.RequestExp;
		_expSlider.fillAmount = (float)exp / requiredExp;
		_expText.text = $"{exp} / {requiredExp}";
	}

	private void UpdateLevel(int level)
	{
		_levelText.text = $"{level}";
	}

	private void UpdateGold(int gold)
	{
		_goldText.text = $"{gold}";
	}

	private void UpdateAtk(int atk)
	{
		_atkText.text = $"{atk}";
	}
}
