using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
	[SerializeField] private GameObject _shopPanel;
	[SerializeField] private TextMeshProUGUI _shopTitleText;
	[SerializeField] private InfiniteScrollView _scrollView;
	[SerializeField] private Button _closeButton;

	private string _currentShopId;

#if UNITY_EDITOR
	[SerializeField] private bool _previewInEditor;
#endif

	private void Awake()
	{
		if (_closeButton != null)
			_closeButton.onClick.AddListener(Hide);

		Hide();
	}

	public void Show(string shopId)
	{
		_currentShopId = shopId;
		_shopPanel.SetActive(true);

		if (_shopTitleText != null)
			_shopTitleText.text = "상점";

		ShopManager.Instance.OnShopStockChanged += RefreshList;

		RefreshList();
	}

	public void Hide()
	{
		if (ShopManager.Instance != null)
			ShopManager.Instance.OnShopStockChanged -= RefreshList;

		_currentShopId = null;
		_shopPanel.SetActive(false);
	}

	private void RefreshList()
	{
		if (string.IsNullOrEmpty(_currentShopId)) return;

		var items = ShopDatabase.Instance?.GetShopItems(_currentShopId);
		if (items == null) return;

		var dataList = new List<InfiniteScrollData>();
		for (int i = 0; i < items.Count; i++)
		{
			var shopItem = items[i];
			dataList.Add(new ShopSlotData
			{
				_index = i,
				_shopItemData = shopItem,
				_stock = ShopManager.Instance.GetStock(shopItem.uniqueId),
				_onClicked = (item, stock) => OnSlotClicked(item, stock)
			});
		}

		_scrollView.SetData(dataList);
	}

	private void OnSlotClicked(ShopItemData item, int stock)
	{
		if (stock == 0) return;

		string stockText = stock == -1 ? "" : $" (재고 {stock})";
		string message = $"{item.itemData._itemName}{stockText}\n{item.price} G";

		UIManager.Instance.ShowConfirmPopup(
			"CommonConfirmPopup",
			message,
			"구매",
			"취소",
			CommonConfirmPopup.ConfirmType.OKCancel,
			() => ShopManager.Instance.TryBuy(item.uniqueId)
		);
	}
}
