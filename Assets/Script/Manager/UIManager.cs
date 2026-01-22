using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	
	//NPC 프롬프트 UI
	[SerializeField] private NPCPromptUI npcPrompt;

	//fade 관련	
	[SerializeField] private Image fadeImage;
	[SerializeField] private float fadeDuration = 0.5f;
	private Coroutine _fadeCoroutine;

	//대화 컨트롤러
	[SerializeField] private DialogueController _dialogueController;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			DontDestroyOnLoad(gameObject);
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
		InitializeFadeImage();

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

	//fadeImage 초기화
	private void InitializeFadeImage()
	{
		if(fadeImage == null)
		{
			Debug.LogWarning("Fade Image is not assigned in UIManager.");
			return;
		}

		SetFadeAlpha(0f); //처음에는 투명하게
		fadeImage.gameObject.SetActive(false);
	}

	private void SetFadeAlpha(float alpha)
	{
		if (fadeImage == null)
			return;

		Color color = fadeImage.color;
		color.a = alpha;
		fadeImage.color = color;

	}

	private IEnumerator FadeOutCouroutine(float duration)
	{
		if (fadeImage == null)
			yield break;

		//페이드 아웃 실행 중 클릭 방지
		fadeImage.raycastTarget = true;

		float elapsedTime = 0f;
		float startAlpha = fadeImage.color.a;
		float targetAlpha = 1f; //완전 불투명

		fadeImage.gameObject.SetActive(true);
		
		//fade out을 위한 값 변경을 while에서 반복
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration; //알파값이 변화하는 진행률

			float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
			SetFadeAlpha(alpha);

			yield return null;
		}

		//최종 알파값 설정
		SetFadeAlpha(targetAlpha);
	}

	private IEnumerator FadeInCoroutine(float duration)
	{
		if (fadeImage == null)
			yield break;

		float elapsedTime = 0f;
		float startAlpha = fadeImage.color.a;
		float targetAlpha = 0f; //완전 투명
		
		//fade in을 위한 값 변경을 while에서 반복
		while (elapsedTime < duration)
		{
			elapsedTime += Time.deltaTime;
			float t = elapsedTime / duration; //알파값이 변화하는 진행률
			
			float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
			SetFadeAlpha(alpha);

			yield return null;
		}

		//최종 알파값 설정
		SetFadeAlpha(targetAlpha);
		//페이드 인 완료 후 클릭 허용
		fadeImage.raycastTarget = false;
		fadeImage.gameObject.SetActive(false);
	}

	private bool _isFading = false;

	//외부 호출용
	public void RequestFadeTransition(float duration, Action onFadeOutComplete, Action onComplete = null)
	{
		if (_isFading)
			return;

		if(fadeImage == null)
		{
			//fadeImage가 없으면 바로 콜백 호출 후 종료
			onFadeOutComplete?.Invoke();
			onComplete?.Invoke();
			return;
		}

		duration = Mathf.Max(fadeDuration, duration); //0 이하 방지
	
		_fadeCoroutine = StartCoroutine(FadeTransitionCoroutine(duration, onFadeOutComplete, onComplete));
	}

	private IEnumerator FadeTransitionCoroutine(float duration, Action onFadeOutComplete, Action onComplete)
	{
		_isFading = true;

		//페이드 아웃
		yield return StartCoroutine(FadeOutCouroutine(duration));

		//페이드 아웃 완료 콜백
		onFadeOutComplete?.Invoke();

		//페이드 인
		yield return StartCoroutine(FadeInCoroutine(duration));

		//페이드 인 완료 콜백
		onComplete?.Invoke();

		_isFading = false;
		_fadeCoroutine = null;
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

	public void StartDialogue(string name, IEnumerable<string> lines, Action onComplete = null)
	{
		if(_dialogueController != null)
		{
			_dialogueController.StartDialogue(name, lines, onComplete);
		}
	}

	public bool IsDialogueOpen()
	{
		return _dialogueController != null && _dialogueController.IsDialogueRunning;
	}
	
}
