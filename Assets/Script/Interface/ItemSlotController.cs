using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlotData : InfiniteScrollData
{
    public InventorySlot _inventorySlot;
    public bool _isEquipped;
	public Action<InventorySlot, bool> _onClicked;
}

public class ItemSlotController : InfiniteScrollItem
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _countText;
    [SerializeField] private GameObject _equipped;
    [SerializeField] private Button _slotButton;

    private ItemSlotData _slotData;


	protected override void RefreshUI()
	{
		if (_data is not ItemSlotData slotData)
			return;

		_slotData = slotData;

		var item = slotData._inventorySlot;

		if (item == null || item.data == null)
			return;

		if (_nameText != null)
			_nameText.text = item.data._itemName;

		if (_countText != null)
		{
			if (item.data._itemType == ItemType.Equipment)
				_countText.gameObject.SetActive(false);
			else
			{
				_countText.gameObject.SetActive(true);
				_countText.text = item.count.ToString();
			}
		}

		if (_equipped != null)
			_equipped.SetActive(slotData._isEquipped);

		if (_icon != null && !string.IsNullOrEmpty(item.data._iconPath))
		{
			Sprite sprite = Resources.Load<Sprite>(item.data._iconPath);

			if (sprite != null)
				_icon.sprite = sprite;
		}
	}

	private void Awake()
	{
		if (_slotButton != null)
			_slotButton.onClick.AddListener(OnSlotClicked);
	}

	private void OnSlotClicked()
	{
		if (_slotData == null)
			return;

		_slotData._onClicked?.Invoke(_slotData._inventorySlot , _slotData._isEquipped);
	}


}
