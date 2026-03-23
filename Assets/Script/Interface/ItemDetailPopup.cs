using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailPopup : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _nameText; //아이템 이름
	[SerializeField] private TextMeshProUGUI _descriptionText; //아이템 정보
	[SerializeField] private TextMeshProUGUI _effectText; //효과
	[SerializeField] private Button _actionButton; //착용,해제
	[SerializeField] private TextMeshProUGUI _actionButtonText; //착용 해제
	[SerializeField] private Button _closeButton; //팝업 닫기

	private Action _actionCallback;

	private void Awake()
	{
		if (_actionButton != null)
			_actionButton.onClick.AddListener(OnActionClicked);
		if (_closeButton != null)
			_closeButton.onClick.AddListener(ClosePopup);
	}

	public void SetPopup(InventorySlot slot, bool isEquipped, Action actionCallback)
	{
		_actionCallback = actionCallback;

		_nameText.text = slot.data._itemName;
		_descriptionText.text = slot.data._description;

		if (slot.data._itemType == ItemType.Equipment)
		{
			_effectText.text = $"공격력 +{slot.data._effectValue}";
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
