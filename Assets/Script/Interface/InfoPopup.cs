using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InfoPopup : MonoBehaviour, IPopup
{
	[SerializeField]
	private TextMeshProUGUI _messageText;
	
	[SerializeField]
	private Button _closeButton;

	public void SetPopup(string message)
	{
		if (_messageText != null)
		{
			_messageText.text = message;
		}
	}

	public void ClosePopup()
	{
		UIManager.Instance.ClearCurrentPopup();
		Destroy(gameObject);
	}
}