public class DropData
{
	public string _itemId;
	public int _weight;

	public DropData(DropTableData raw)
	{
		_itemId = raw.itemId;
		_weight = raw.weight;
	}
}