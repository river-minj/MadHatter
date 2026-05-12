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

	[Header("Buttons")]
	[SerializeField] private Button _inventoryButton;
	[SerializeField] private Button _questButton;
	[SerializeField] private Button _interactButton;

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

		if (_inventoryButton != null)
		{
			_inventoryButton.onClick.AddListener(OnInventoryButtonClicked);
		}

		if (_questButton != null)
		{
			_questButton.onClick.AddListener(OnQuestButtonClicked);
		}

		if (_interactButton != null)
		{
			_interactButton.onClick.AddListener(OnInteractButtonClicked);
			_interactButton.gameObject.SetActive(false);
		}

		var player = GameManager.Instance.GetPlayerController();
		if (player != null)
		{
			player.OnInteractableChanged += HandleInteractableChanged;
		}

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

		if (GameManager.Instance != null)
		{
			var player = GameManager.Instance.GetPlayerController();
			if (player != null)
			{
				player.OnInteractableChanged -= HandleInteractableChanged;
			}
		}
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

	private void OnInventoryButtonClicked()
	{
		UIManager.Instance.ToggleInventory();
	}

	private void OnQuestButtonClicked()
	{
		UIManager.Instance.ToggleQuest();
	}

	private void OnInteractButtonClicked()
	{
		var player = GameManager.Instance.GetPlayerController();
		if (player != null)
		{
			player.TryInteract();
		}
	}

	private void HandleInteractableChanged(bool hasInteractable)
	{
		_interactButton.gameObject.SetActive(hasInteractable);
	}
}
