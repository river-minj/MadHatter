using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollView : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private ScrollRect _scrollRect;
	[SerializeField] private RectTransform _content;
	[SerializeField] private InfiniteScrollItem _slotPrefab; //이 스크롤 뷰에 사용할 슬롯 프리팹

	[Header("Settings")]
	[SerializeField] private int _spawnCount = 10; // 화면에 보일 풀링 슬롯 개수
												   // to do : 동적으로 계산

	private List<InfiniteScrollData> _dataList;
	private List<InfiniteScrollItem> _slots = new List<InfiniteScrollItem>();

	private float _slotHeight;
	private int _currentTopIndex = 0;

	void Awake()
	{
		// 슬롯 높이 계산
		_slotHeight = (_slotPrefab.transform as RectTransform).sizeDelta.y;
	}

	public void SetData(List<InfiniteScrollData> dataList)
	{
		_dataList = dataList;

		// content 높이 확장
		float height = _slotHeight * dataList.Count;
		_content.sizeDelta = new Vector2(_content.sizeDelta.x, height);

		CreatePool();
		UpdateSlots(true);
	}

	private void CreatePool()
	{
		for (int i = 0; i < _spawnCount; i++)
		{
			var item = Instantiate(_slotPrefab, _content);
			_slots.Add(item);
		}
	}

	void Update()
	{
		UpdateSlots();
	}

	private void UpdateSlots(bool forceRefresh = false)
	{
		if (_dataList == null || _dataList.Count == 0)
			return;

		float scrollPos = _content.anchoredPosition.y;
		int newTop = Mathf.FloorToInt(scrollPos / _slotHeight);

		if (!forceRefresh && newTop == _currentTopIndex)
			return;

		_currentTopIndex = newTop;

		for (int i = 0; i < _slots.Count; i++)
		{
			int dataIndex = _currentTopIndex + i;
			if (dataIndex < 0 || dataIndex >= _dataList.Count)
			{
				_slots[i].gameObject.SetActive(false);
				continue;
			}

			_slots[i].gameObject.SetActive(true);

			// 위치 조정
			RectTransform rt = _slots[i].transform as RectTransform;
			float yPos = -_slotHeight * dataIndex - _slotHeight / 2f;
			rt.anchoredPosition = new Vector2(0, yPos);

			// 데이터 바인딩
			InfiniteScrollData d = _dataList[dataIndex];
			d._index = dataIndex;
			_slots[i].SetData(d);
		}
	}
}
