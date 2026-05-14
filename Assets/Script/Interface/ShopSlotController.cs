using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotData : InfiniteScrollData
{
	public ShopItemData _shopItemData;
	public int _stock;
	public Action<ShopItemData, int> _onClicked;
}

public class ShopSlotController : InfiniteScrollItem
{
	[SerializeField] private Image _icon;
	[SerializeField] private TextMeshProUGUI _nameText;
	[SerializeField] private TextMeshProUGUI _priceText;
	[SerializeField] private TextMeshProUGUI _stockText;
	[SerializeField] private Button _slotButton;
	[SerializeField] private GameObject _soldOutOverlay;

	private ShopSlotData _slotData;

	private void Awake()
	{
		if (_slotButton != null)
			_slotButton.onClick.AddListener(OnSlotClicked);
	}

	protected override void RefreshUI()
	{
		if (_data is not ShopSlotData slotData)
			return;

		_slotData = slotData;
		var item = slotData._shopItemData;

		if (item == null || item.itemData == null)
			return;

		if (_nameText != null)
			_nameText.text = item.itemData._itemName;

		if (_priceText != null)
			_priceText.text = $"{item.price} G";

		if (_stockText != null)
		{
			if (slotData._stock == -1)
				_stockText.gameObject.SetActive(false);
			else
			{
				_stockText.gameObject.SetActive(true);
				_stockText.text = $"재고 {slotData._stock}";
			}
		}

		bool soldOut = slotData._stock == 0;
		if (_soldOutOverlay != null)
			_soldOutOverlay.SetActive(soldOut);
		if (_slotButton != null)
			_slotButton.interactable = !soldOut;

		if (_icon != null && !string.IsNullOrEmpty(item.itemData._iconPath))
		{
			Sprite sprite = Resources.Load<Sprite>(item.itemData._iconPath);
			if (sprite != null)
				_icon.sprite = sprite;
		}
	}

	private void OnSlotClicked()
	{
		if (_slotData == null) return;
		_slotData._onClicked?.Invoke(_slotData._shopItemData, _slotData._stock);
	}
}
