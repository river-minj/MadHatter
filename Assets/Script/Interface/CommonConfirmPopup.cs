using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommonConfirmPopup : MonoBehaviour
{
	public enum ConfirmType
	{
		OKCancel,
		OK,
	}

	[SerializeField] private TextMeshProUGUI _messageText;
	[SerializeField] private TextMeshProUGUI _confirmText;
	[SerializeField] private TextMeshProUGUI _cancelText;
	
	[SerializeField] private GameObject _confirmButton;
	[SerializeField] private GameObject _cancelButton;

	private Action _confirmAction;
	private Action _cancelAction;

	public void SetPopup(ConfirmType type, string message, string confirm, string cancle, Action confirmAction, Action cancelAction)
	{
		if(_messageText != null)
			_messageText.text = message;

		switch (type)
		{
			case ConfirmType.OKCancel:
				if(_confirmButton != null)
					_confirmButton.SetActive(true);
				if(_cancelButton != null)
					_cancelButton.SetActive(true);
				break;
			case ConfirmType.OK:
				if(_confirmButton != null)
					_confirmButton.SetActive(true);
				if(_cancelButton != null)
					_cancelButton.SetActive(false);
				break;
			default:
				Debug.LogError("Unsupported ConfirmType: " + type);
				break;
		}

		if(_confirmText != null)
			_confirmText.text = confirm;	

		if(_cancelText != null)
			_cancelText.text = cancle;

		_confirmAction = confirmAction;
		_cancelAction = cancelAction;

    }

	public void OnClickConfirm()
	{
		_confirmAction?.Invoke();
		ClosePopup();
	}

	public void OnClickCancel()
	{
		_cancelAction?.Invoke();
		ClosePopup();
	}

	private void ClosePopup()
	{
		//to do : 팝업 풀링 시스템이 도입되면 Destroy 대신 비활성화 후 재사용하는 방식으로 변경할 것
		Destroy(gameObject);
	}
}
