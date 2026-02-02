using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollData
{
	public int _index;
}

public class InfiniteScrollItem : MonoBehaviour
{
	protected InfiniteScrollData _data;
	public void SetData(InfiniteScrollData data)
	{
		// 데이터 설정 로직 구현
		_data = data;
		RefreshUI();
	}

	protected virtual void RefreshUI()
	{
		// UI 갱신 로직 구현

	}
}
