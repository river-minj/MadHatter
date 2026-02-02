using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{

    [SerializeField] private GameObject _inventoryPanel;
    [SerializeField] private Transform _content;
    [SerializeField] private GameObject _companioanSlot;

	private void Awake()
	{
        Hide();
	}

    public void Show()
    {
		_inventoryPanel.SetActive(true);
		RefreshUI();
	}

	public void Hide()
	{
		_inventoryPanel.SetActive(false);
	}

	public void Toggle()
	{
		if(_inventoryPanel.activeSelf)
		{
			Hide();
		}
		else
		{
			Show();
		}
	}

	public void RefreshUI()
	{
		// Clear existing slots
		foreach (Transform child in _content)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < 33; i++)
		{
			// Populate with current companions
			foreach (var companion in CompanionManager.Instance.OwnedCompanions)
			{
				GameObject slot = Instantiate(_companioanSlot, _content);
				CompanionSlotController slotUI = slot.GetComponent<CompanionSlotController>();
				slotUI.SetData(companion);
			}
		}
	}
}
