using System;
using UnityEngine;

public class PromptController : InteractionController
{
	[SerializeField]
	private string _promptMessage;

	protected override void OnInteract()
	{
		// PromptController는 단순히 상호작용 메시지를 표시하는 역할만 수행
		UIManager.Instance.ShowInfoPopup(_promptMessage, null);
	}
}