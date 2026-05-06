using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollView : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private ScrollRect _scrollRect;
	[SerializeField] private RectTransform _content;
	[SerializeField] private InfiniteScrollItem _slotPrefab;

	[Header("Grid Settings")]
	[SerializeField] private int _columns = 2;
	[SerializeField] private Vector2 _spacing = new Vector2(10, 10);

	[Header("Padding")]
	[SerializeField] private int _paddingLeft = 10;
	[SerializeField] private int _paddingRight = 10;
	[SerializeField] private int _paddingTop = 10;
	[SerializeField] private int _paddingBottom = 10;
	private RectOffset _padding;

	private List<InfiniteScrollData> _dataList;
	private List<InfiniteScrollItem> _slotPool = new List<InfiniteScrollItem>();

	private Vector2 _slotSize;
	private int _poolSize;
	private int _currentTopRow = 0;

	private RectTransform _viewport;

	void Awake()
	{
		InitPadding();
		_viewport = _scrollRect.viewport;
		CalculateSlotSize();
	}

	private void InitPadding()
	{
		if (_padding != null) return;

		_padding = new RectOffset(
			_paddingLeft,
			_paddingRight,
			_paddingTop,
			_paddingBottom
		);
	}

	private void CalculateSlotSize()
	{
		if (_slotPrefab == null)
		{
			Debug.LogError("[InfiniteScrollView] Slot Prefab is not assigned.");
			return;
		}

		RectTransform prefabRect = _slotPrefab.GetComponent<RectTransform>();
		_slotSize = prefabRect.sizeDelta;
	}

	/// <summary>
	/// 화면에 보이는 행 수를 기반으로 풀 크기 계산 (데이터 개수 초과 불가)
	/// </summary>
	private void CalculatePoolSize()
	{
		if (_scrollRect == null || _scrollRect.viewport == null)
		{
			Debug.LogError("[InfiniteScroll] ScrollRect or Viewport is null!");
			return;
		}

		float viewportHeight = _viewport.rect.height;

		if (viewportHeight <= 0)
		{
			Debug.LogWarning($"[InfiniteScroll] Viewport height is {viewportHeight}, using default");
			viewportHeight = 500f;
		}

		float rowHeight = _slotSize.y + _spacing.y;
		int visibleRows = Mathf.CeilToInt(viewportHeight / rowHeight) + 2;
		int maxVisibleSlots = visibleRows * _columns;

		// 데이터 수보다 많은 슬롯은 불필요
		int dataCount = _dataList != null ? _dataList.Count : 0;
		_poolSize = Mathf.Min(maxVisibleSlots, dataCount);
	}

	public void SetData(List<InfiniteScrollData> dataList)
	{
		if (dataList == null) return;

		_dataList = new List<InfiniteScrollData>(dataList); // 외부 변경 방어를 위해 복사본 저장
		InitPadding();

		CalculateContentHeight();
		CalculatePoolSize();
		ResizePool(_poolSize);

		if (_dataList.Count == 0)
		{
			foreach (var slot in _slotPool)
				slot.gameObject.SetActive(false);
			return;
		}

		_content.anchoredPosition = Vector2.zero;
		_currentTopRow = 0;

		UpdateSlots(true);
	}

	private void CalculateContentHeight()
	{
		int totalRows = Mathf.CeilToInt((float)_dataList.Count / _columns);

		float contentHeight = _padding.top + _padding.bottom
							+ (totalRows * _slotSize.y)
							+ (Mathf.Max(0, totalRows - 1) * _spacing.y); // totalRows=0일 때 음수 방지

		_content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);

#if UNITY_EDITOR
		Debug.Log($"[InfiniteScroll] Content height set to {contentHeight}");
#endif
	}

	/// <summary>
	/// 풀을 targetSize에 맞게 축소하거나 확장
	/// </summary>
	private void ResizePool(int targetSize)
	{
		// 초과 슬롯 제거
		while (_slotPool.Count > targetSize)
		{
			int last = _slotPool.Count - 1;
			Destroy(_slotPool[last].gameObject);
			_slotPool.RemoveAt(last);
		}

		// 부족한 슬롯 추가
		while (_slotPool.Count < targetSize)
		{
			var slot = Instantiate(_slotPrefab, _content);
			_slotPool.Add(slot);
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

		float scrollPos = -_content.anchoredPosition.y; // 스크롤 내릴수록 anchoredPosition.y가 음수이므로 부호 반전
		float rowHeight = _slotSize.y + _spacing.y;
		int newTopRow = Mathf.Max(0, Mathf.FloorToInt(scrollPos / rowHeight));

		if (!forceRefresh && newTopRow == _currentTopRow)
			return;

		_currentTopRow = newTopRow;

#if UNITY_EDITOR
		if (forceRefresh)
			Debug.Log($"[Update] Current top row: {_currentTopRow}");
#endif

		for (int i = 0; i < _slotPool.Count; i++)
		{
			int slotRow = _currentTopRow + (i / _columns);
			int slotCol = i % _columns;

			int dataIndex = slotRow * _columns + slotCol;

			if (dataIndex < 0 || dataIndex >= _dataList.Count)
			{
				_slotPool[i].gameObject.SetActive(false);
				continue;
			}

			_slotPool[i].gameObject.SetActive(true);

			RectTransform rt = _slotPool[i].transform as RectTransform;
			float xPos = _padding.left + slotCol * (_slotSize.x + _spacing.x) + _slotSize.x / 2f;
			float yPos = -(_padding.top + slotRow * (_slotSize.y + _spacing.y) + _slotSize.y / 2f);
			rt.anchoredPosition = new Vector2(xPos, yPos);

			bool isVisible = IsSlotInViewport(rt);

			InfiniteScrollData data = _dataList[dataIndex];
			data._index = dataIndex;
			_slotPool[i].SetData(data);

			if (isVisible)
				_slotPool[i].Show();
			else
				_slotPool[i].Hide();
		}
	}

	private bool IsSlotInViewport(RectTransform slot)
	{
		if (_viewport == null)
			return true;

		Vector3[] slotCorners = new Vector3[4];
		Vector3[] viewportCorners = new Vector3[4];

		slot.GetWorldCorners(slotCorners);
		_viewport.GetWorldCorners(viewportCorners);

		float slotTop = slotCorners[1].y;
		float slotBottom = slotCorners[0].y;
		float viewTop = viewportCorners[1].y;
		float viewBottom = viewportCorners[0].y;

		return slotBottom < viewTop && slotTop > viewBottom;
	}
}
