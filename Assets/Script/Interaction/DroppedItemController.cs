using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : InteractionController
{
	[SerializeField] private SpriteRenderer _iconRenderer;

	private string _itemId;

	public void Init(string itemId)
	{
		_itemId = itemId;

		ItemData data = ItemDatabase.Instance.GetItemById(itemId);
		if (data == null) return;

		interactionMessage = $"'{data._itemName}' 줍기";

		if (_iconRenderer != null && !string.IsNullOrEmpty(data._iconPath))
		{
			Sprite sprite = Resources.Load<Sprite>(data._iconPath);
			if (sprite != null)
			{
				_iconRenderer.sprite = sprite;
			}
		}
	}

	protected override void OnInteract()
	{
		if (string.IsNullOrEmpty(_itemId)) return;

		ItemData data = ItemDatabase.Instance.GetItemById(_itemId);
		if (data != null)
			UIManager.Instance.ShowToast($"{data._itemName}{KoreanParticle.EulReul(data._itemName)} 획득했다!");

		InventoryManager.Instance.AddItem(_itemId, 1);
		Destroy(gameObject);
	}
}
