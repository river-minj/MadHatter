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
		foreach(var tab in _tabs)
		{
			tab.LinkedPage.gameObject.SetActive(tab == selectedTab);
		}

		OnTabChanged?.Invoke(selectedTab);
	}

}
