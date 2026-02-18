using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollView : MonoBehaviour
{
	[Header("UI References")]
	[SerializeField] private ScrollRect _scrollRect;
	[SerializeField] private RectTransform _content;
	[SerializeField] private InfiniteScrollItem _slotPrefab; //이 스크롤 뷰에 사용할 슬롯 프리팹

	[Header("Grid Settings")]
	[SerializeField] private int _columns = 2; // 열 개수
	[SerializeField] private Vector2 _spacing = new Vector2(10, 10); // 슬롯 간격

	[Header("Padding")]
	[SerializeField] private int _paddingLeft = 10;
	[SerializeField] private int _paddingRight = 10;
	[SerializeField] private int _paddingTop = 10;
	[SerializeField] private int _paddingBottom = 10;
	private RectOffset _padding;
	
	private List<InfiniteScrollData> _dataList;
	private List<InfiniteScrollItem> _slotPool = new List<InfiniteScrollItem>();

	//자동 계산되는 값들
	private Vector2 _slotSize; //슬롯크기 (from prefab)
	private int _poolSize; //풀 크기 (동적 계산)
	private int _currentTopRow = 0; //현재 최상단 행 인덱스
	private bool _isInitialized = false;

	private RectTransform _viewport;

	void Awake()
	{
		InitPadding();

		_viewport = _scrollRect.viewport; 
		CalculateSlotSize();

	}

	private void InitPadding()
	{
		if ( _padding != null ) { return; }

		_padding = new RectOffset(
	   _paddingLeft,
	   _paddingRight,
	   _paddingTop,
	   _paddingBottom
	   );

	}

	/// <summary>
	/// 슬롯 크기를 프리팹에서 가져옴
	/// </summary>
	private void CalculateSlotSize()
	{
		if(_slotPrefab == null)
		{
			Debug.LogError("[InfiniteScrollView] Slot Prefab is not assigned.");
			return;
		}


		RectTransform prefabRect = _slotPrefab.GetComponent<RectTransform>();
		_slotSize = prefabRect.sizeDelta;

		Debug.Log($"[InfiniteScrollView] Slot Size : {_slotSize}");
	}

	/// <summary>
	/// 화면에 보이는 행 수를 계산해서 풀 크기를 정함
	/// </summary>
	private void CalculatePoolSize()
	{
		//null 체크
		if (_scrollRect == null || _scrollRect.viewport == null)
		{
			Debug.LogError("[InfiniteScroll] ScrollRect or Viewport is null!");
			return;
		}

		float viewportHeight = _viewport.rect.height;

		//0 체크
		if (viewportHeight <= 0)
		{
			Debug.LogWarning($"[InfiniteScroll] Viewport height is {viewportHeight}, using default");
			viewportHeight = 500f; // 기본값
		}

		float rowHeight = _slotSize.y + _spacing.y;
		int visibleRows = Mathf.CeilToInt(viewportHeight / rowHeight) + 2;
		_poolSize = visibleRows * _columns;
	}


	public void SetData(List<InfiniteScrollData> dataList)
	{
		if(dataList == null)
		{
			return;
		}	

		_dataList = dataList;
		InitPadding();

		//content 높이 계산
		CalculateContentHeight();

		if(!_isInitialized)
		{
			CalculatePoolSize();
			CreatePool();
			_isInitialized = true;
		}

		_content.anchoredPosition = Vector2.zero;
		_currentTopRow = 0;

		UpdateSlots(true);
	}

	/// <summary>
	/// content 높이를 데이터 수에 맞게 계산
	/// </summary>
	private void CalculateContentHeight()
	{
		int totalRows = Mathf.CeilToInt((float)_dataList.Count / _columns);

		float contentHeight = _padding.top + _padding.bottom
							+ (totalRows * _slotSize.y)
							+ ((totalRows - 1) * _spacing.y);



		// 설정
		_content.sizeDelta = new Vector2(_content.sizeDelta.x, contentHeight);

#if UNITY_EDITOR
		Debug.Log($"[InfiniteScroll] Content height set to {contentHeight}");
#endif
	}


	private void CreatePool()
	{
		//기존 슬롯 제거
		foreach (var child in _slotPool)
		{
			if(child != null)
			{
				Destroy(child.gameObject);
			}
		}
		_slotPool.Clear();

		// 슬롯 풀링 생성
		for (int i = 0; i < _poolSize; i++)
		{
			var slot = Instantiate(_slotPrefab, _content);
			RectTransform rt = slot.GetComponent<RectTransform>();
			if(rt!= null)
			{
				_slotPool.Add(slot);
			}
		}
	}

	void Update()
	{
		UpdateSlots();
	}

	/// <summary>
	/// 슬롯 위치 및 데이터 업데이트
	/// </summary>
	/// <param name="forceRefresh"></param>
	private void UpdateSlots(bool forceRefresh = false)
	{
		if (_dataList == null || _dataList.Count == 0)
			return;
		
		//현재 스크롤 위치에서 최상단 행 계산
		float scrollPos = _content.anchoredPosition.y;
		float rowHeight = _slotSize.y + _spacing.y;
		int newTopRow = Mathf.Max(0, Mathf.FloorToInt(scrollPos / rowHeight));

		if (!forceRefresh && newTopRow == _currentTopRow)
			return;

		_currentTopRow = newTopRow;


		if (forceRefresh)
		{
			Debug.Log($"[Update] Current top row: {_currentTopRow}");
		}

		//그리드 배치
		for (int i = 0; i < _slotPool.Count; i++)
		{
			int slotRow = _currentTopRow +(i / _columns);
			int slotCol = i % _columns;

			int dataIndex = slotRow * _columns + slotCol;

			//데이터 인덱스가 범위 밖이면 비활성화
			if (dataIndex < 0 || dataIndex >= _dataList.Count)
			{
				_slotPool[i].gameObject.SetActive(false);
				continue;
			}
			else
			{
				_slotPool[i].gameObject.SetActive(true);
			}

			// 위치 조정
			RectTransform rt = _slotPool[i].transform as RectTransform;
			float xPos = _padding.left + slotCol * (_slotSize.x + _spacing.x) + _slotSize.x / 2f;
			float yPos = -(_padding.top + slotRow * (_slotSize.y + _spacing.y) + _slotSize.y / 2f);

			rt.anchoredPosition = new Vector2(xPos, yPos);

			//가시성 계산
			bool isVisible = IsSlotInViewport(rt);

			// 데이터 설정
			InfiniteScrollData data = _dataList[dataIndex];
			data._index = dataIndex;
			_slotPool[i].SetData(data);

			if(isVisible)
			{
				_slotPool[i].Show();
			}
			else
			{
				_slotPool[i].Hide();
			}
			
		}

	}

	private bool IsSlotInViewport(RectTransform slot)
	{
		if(_viewport == null)
		{
			Debug.LogWarning("[ScrollDebug] _viewport is NULL → always visible");
			return true;
		}

		Vector3[] slotCorners = new Vector3[4];
		Vector3[] viewportCorners = new Vector3[4];

		slot.GetWorldCorners(slotCorners);
		_viewport.GetWorldCorners(viewportCorners);


		float slotTop = slotCorners[1].y;
		float slotBottom = slotCorners[0].y;

		float viewTop = viewportCorners[1].y;
		float viewBottom = viewportCorners[0].y;


		bool vertically = slotBottom < viewTop && slotTop > viewBottom;

		Debug.Log(
		$"[VIS] slotTop={slotTop:F1}, slotBottom={slotBottom:F1}, " +
		$"viewTop={viewTop:F1}, viewBottom={viewBottom:F1}, isVisible={vertically}"
	);

		return vertically;
	}
}
