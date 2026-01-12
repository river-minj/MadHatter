using System;
using System.Collections;
using UnityEngine;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	[SerializeField] private NPCPromptUI npcPrompt;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
		}
		else
		{
			Debug.LogWarning("Multiple instances of UIManager detected. Destroying duplicate.");
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		if(npcPrompt == null)
		{
			//to do : 이 것의 위치 정리 고민 필요..
			npcPrompt = FindObjectOfType<NPCPromptUI>();

			if(npcPrompt == null)
			{
				Debug.LogWarning("NPCPromptUI not found in the scene.");
			}
		}
	}

	bool _isFading = false;
	private IEnumerator FadeRoutine(float fadeDuration, Action onAction, Action onComplete= null)
	{
		if (_isFading)
			yield break;

		_isFading = true;

		FadeOut();
		yield return new WaitForSeconds(fadeDuration);
		
		onAction?.Invoke();

		//한 프레임 대기 (새 맵 초기화)
		yield return null;

		
		yield return new WaitForSeconds(fadeDuration);

		onComplete?.Invoke();

		_isFading = false;
	}

	//어두워짐
	private void FadeOut()
	{
	}

	//밝아짐	
	private void FadeIn()
	{
	}

	public void RequestFade(float fadeDuratio, Action onAction, Action onComplete = null)
	{
		if (_isFading)
			return;

		if (onAction == null)
			return;

		StartCoroutine(FadeRoutine(fadeDuratio, onAction, onComplete));
	}


	public void ShowNPCPrompt(string message, Transform npcTransfomt)
	{
		if (npcPrompt != null)
		{
			npcPrompt.Show(message, npcTransfomt);
		}
	}

	public void HideNPCPrompt()
	{
		if (npcPrompt != null)
		{
			npcPrompt.Hide();
		}
	}
}
