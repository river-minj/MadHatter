using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollData
{
	public int _index;
}

public abstract class InfiniteScrollItem : MonoBehaviour
{
	protected InfiniteScrollData _data;

	private bool _isVisible = true;
	public virtual void SetData(InfiniteScrollData data)
	{
		// 데이터 설정 로직 구현
		_data = data;
		RefreshUI();
	}

	public void Show()
	{
		if (_isVisible)
			return;

		_isVisible = true;
		OnShow();
	}

	public void Hide()
	{
		if (!_isVisible)
			return;

		_isVisible = false;
		OnHide();
	}

	protected abstract void RefreshUI();
	protected virtual void OnShow()
	{
		// 아이템이 화면에 보일 때 실행할 로직 구현
	}
	protected virtual void OnHide()
	{
		// 아이템이 화면에서 사라질 때 실행할 로직 구현
	}
}
