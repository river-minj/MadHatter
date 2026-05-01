using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
	private List<Tab> _tabs = new List<Tab>();
	public event Action<Tab> OnTabChanged;

	public enum TabState
	{
		Normal,
		Selected,
		Locked,
	}

	private void Awake()
	{
		_tabs = new List<Tab>(GetComponentsInChildren<Tab>(true));
		foreach (var tab in _tabs)
		{
			var captured = tab;
			//자식으로 있는 모든 탭 버튼에 이벤트 연결
			captured.TabButton.onClick.AddListener(() => ChangeTab(captured));
		}
	}

	public void SetDefaultTab()
	{
		// 첫 번째 잠기지 않은 탭을 선택
		foreach (var tab in _tabs)
		{
			if (!tab.IsLocked)
			{
				ChangeTab(tab);
				return;
			}
		}
	}

	private void ChangeTab(Tab selectedTab)
	{
		//잠긴 탭은 선택 불가
		if (selectedTab.IsLocked)
		{
			return;
		}

		HashSet<GameObject> activePages = new HashSet<GameObject>();

		// 선택된 탭의 페이지를 먼저 수집
		activePages.Add(selectedTab.LinkedPage.gameObject);

		// 수집된 페이지만 활성화, 나머지 비활성화 + 탭 비주얼 갱신
		foreach (var tab in _tabs)
		{
			bool shouldActive = activePages.Contains(tab.LinkedPage.gameObject);
			tab.LinkedPage.gameObject.SetActive(shouldActive);
			tab.SetSelected(tab == selectedTab);
		}

		OnTabChanged?.Invoke(selectedTab);
	}

}
