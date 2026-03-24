using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
	private List<Tab> _tabs = new List<Tab>();
	public event Action<Tab> OnTabChanged;

	private void Awake()
	{
		_tabs = new List<Tab>(GetComponentsInChildren<Tab>(true));
		foreach (var tab in _tabs)
		{
			var captured = tab;
			captured.TabButton.onClick.AddListener(() => ChangeTab(captured));
		}
	}

	public void SetDefaultTab()
	{
		if(_tabs.Count >0)
		{
			ChangeTab(_tabs[0]);
		}
	}

	private void ChangeTab(Tab selectedTab)
	{
		HashSet<GameObject> activePages = new HashSet<GameObject>();

		// 선택된 탭의 페이지를 먼저 수집
		activePages.Add(selectedTab.LinkedPage.gameObject);

		// 수집된 페이지만 활성화, 나머지 비활성화
		foreach (var tab in _tabs)
		{
			bool shouldActive = activePages.Contains(tab.LinkedPage.gameObject);
			tab.LinkedPage.gameObject.SetActive(shouldActive);
		}

		OnTabChanged?.Invoke(selectedTab);
	}

}
