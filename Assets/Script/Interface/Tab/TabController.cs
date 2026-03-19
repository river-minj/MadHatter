using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class TabEntry
{
	public Button tabButton;
	public GameObject tabPage;
}


public class TabController : MonoBehaviour
{
	[SerializeField] private List<TabEntry> _tabs;

	public event Action<TabEntry> OnTabChanged;

	private void Awake()
	{
		for(int i = 0; i < _tabs.Count; i++)
		{
			var tab = _tabs[i];
			tab.tabButton.onClick.AddListener(()=> ChangeTab(tab));
		}
	}

	public void SetDefaultTab()
	{
		if(_tabs.Count >0)
		{
			ChangeTab(_tabs[0]);
		}
	}

	private void ChangeTab(TabEntry selectedTab)
	{
		foreach(var tab in _tabs)
		{
			tab.tabPage.SetActive(tab == selectedTab);
		}

		OnTabChanged?.Invoke(selectedTab);
	}

}
