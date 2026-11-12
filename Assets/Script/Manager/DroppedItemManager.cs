using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class DroppedItemManager : MonoBehaviour
{
	public static DroppedItemManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);

			// ObjectPool 초기화
			_dropPool = new ObjectPool<DroppedItemController>(CreateDropItem, OnGetDropItem, OnReleaseDropItem, OnDestroyDropItem);
		}
		else
		{
			Destroy(gameObject);
		}

	}

	//필드
	[SerializeField] private Transform _droppedItemRoot;
	[SerializeField] private DroppedItemController _dropPrefab;
	private ObjectPool<DroppedItemController> _dropPool;
	private HashSet<DroppedItemController> _activeItems = new HashSet<DroppedItemController>();

	private DroppedItemController CreateDropItem()
	{
		//풀에서 새 아이템을 생성할 때
		DroppedItemController dropObj = Instantiate(_dropPrefab, _droppedItemRoot);
		dropObj.SetReleaseCallback(ReleaseDropItem);
		return dropObj;
	}

	private void OnGetDropItem(DroppedItemController dropItem)
	{
		//풀에서 꺼낼때
		dropItem.gameObject.SetActive(true);
	}

	private void OnReleaseDropItem(DroppedItemController dropItem)
	{
		//풀에 반환할때
		dropItem.gameObject.SetActive(false);
	}

	private void OnDestroyDropItem(DroppedItemController dropItem)
	{
		//풀에서 제거될때

		Destroy(dropItem.gameObject);
	}

	//드롭 아이템을 꺼네 정해진 위치에 두고 아이템 정보를 세팅한 뒤 활성화 풀에 추가하는 함수
	public void SpawnDrop(Vector3 position, string itemId)
	{
		DroppedItemController dropItem = _dropPool.Get();
		dropItem.transform.position = position;
		dropItem.Init(itemId);

		_activeItems.Add(dropItem);
	}

	//드롭 아이템을 비활성화하고 풀에 반환하는 함수
	public void ReleaseDropItem(DroppedItemController dic)
	{
		bool result = _activeItems.Remove(dic);
		if (result)
		{
			_dropPool.Release(dic);
		}
	}

	//드롭 아이템을 모두 비활성화하고 풀에 반환하는 함수
	public void ClearDropItems()
	{
		List<DroppedItemController> itemsToRelease = new List<DroppedItemController>(_activeItems);
		foreach(var item in itemsToRelease)
		{
			ReleaseDropItem(item);
		}

	}
}
