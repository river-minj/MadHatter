public enum ItemType
{
	None,
	Equipment, //장비형
	Consumable, //소비형
}

public class ItemData
{
	public string _itemId;
	public string _itemName;
	public ItemType _itemType;
	public string _description;
	public int _effectValue;
	public string _iconPath;

	public ItemData(ItemTableData row)
	{
		_itemId = row.itemId;
		_itemName = row.itemName;
		_itemType = row.itemType;
		_description = row.description;
		_effectValue = row.effectValue;
		_iconPath = row.iconPath;
	}
}