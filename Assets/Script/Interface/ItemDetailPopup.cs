using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPopup : MonoBehaviour
{
	[SerializeField] private Image _iconImage;
	[SerializeField] private TextMeshProUGUI _nameText;
	[SerializeField] private TextMeshProUGUI _descriptionText;
	[SerializeField] private TextMeshProUGUI _effectText;
	[SerializeField] private Button _actionButton;
	[SerializeField] private TextMeshProUGUI _actionButtonText;
	[SerializeField] private Button _closeButton;
	[SerializeField] private Button _cancelButton;

	private Action _actionCallback;

	private void Awake()
	{
		if (_actionButton != null)
			_actionButton.onClick.AddListener(OnActionClicked);
		if (_closeButton != null)
			_closeButton.onClick.AddListener(ClosePopup);
		if (_cancelButton != null)
			_cancelButton.onClick.AddListener(ClosePopup);
	}

	public void SetPopup(InventorySlot slot, bool isEquipped, Action actionCallback)
	{
		_actionCallback = actionCallback;

		_nameText.text = slot.data._itemName;
		_descriptionText.text = slot.data._description;

		if (_iconImage != null && !string.IsNullOrEmpty(slot.data._iconPath))
			_iconImage.sprite = Resources.Load<Sprite>(slot.data._iconPath);

		if (slot.data._itemType == ItemType.Equipment)
		{
			_effectText.text = $"ATK +{slot.data._effectValue}";
		}
		else if (slot.data._itemType == ItemType.Consumable)
		{
			_effectText.text = $"HP +{slot.data._effectValue}";
		}

		if (slot.data._itemType == ItemType.Equipment)
		{
			_actionButtonText.text = isEquipped ? "해제" : "장착";
		}
		else if (slot.data._itemType == ItemType.Consumable)
		{
			_actionButtonText.text = "사용";
		}
	}

	private void OnActionClicked()
	{
		_actionCallback?.Invoke();
		ClosePopup();
	}

	private void ClosePopup()
	{
		Destroy(gameObject);
	}

}
