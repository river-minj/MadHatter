using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 매니저는 게임 내 모든 UI 요소를 관리하는 싱글톤 클래스
/// UI의 한개뿐인 전역 진입점 역할을 수행
/// </summary>
public class UIManager : MonoBehaviour
{
	public static UIManager Instance { get; private set; }
	
	//NPC 프롬프트 UI
	[SerializeField] private NPCPromptUI _npcPrompt;

	//fade 관련	
	[SerializeField] private Image fadeImage;
	[SerializeField] private float fadeDuration = 0.5f;
	private Coroutine _fadeCoroutine;

	//대화 컨트롤러
	[SerializeField] private DialogueUI _dialogueUI;

	//인벤토리 UI
	[SerializeField] private InventoryUI _inventoryUI;

	//퀘스트 UI
	[SerializeField] private QuestUI _questUI;

	//상점 UI
	[SerializeField] private ShopUI _shopUI;

	//popup root
	[SerializeField] private Transform _popupRoot;

	CommonConfirmPopup _currentPopup;
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

		if(_npcPrompt == null)
		{
			//to do : 이 것의 위치 정리 고민 필요..
			_npcPrompt = FindObjectOfType<NPCPromptUI>();

			if(_npcPrompt == null)
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
		if (_npcPrompt != null)
		{
			_npcPrompt.Show(message, npcTransfomt);
		}
	}

	public void HideNPCPrompt()
	{
		if (_npcPrompt != null)
		{
			_npcPrompt.Hide();
		}
	}

	public void StartDialogue(List<DialogueLine> lines, Action onComplete = null)
	{
		if(_dialogueUI != null)
		{
			_dialogueUI.StartDialogue(lines, onComplete);
		}
	}

	public bool IsDialogueOpen()
	{
		return _dialogueUI != null && _dialogueUI.IsDialogueRunning;
	}
	
	public void ToggleInventory()
	{
		if(_inventoryUI != null)
		{
			_inventoryUI.Toggle();
		}
	}
	
	public void AdvanceDialogue()
	{
		if(_dialogueUI != null)
		{
			_dialogueUI.AdvanceDialogue();
		}
	}

	public void ToggleQuest()
	{
		if (_questUI == null)
			return;

		_questUI.Toggle();
	}

	public void ShowShop(string shopId)
	{
		Debug.Log($"[UIManager] ShowShop | shopId={shopId} | _shopUI={((_shopUI == null) ? "null" : "assigned")}");
		if (_shopUI == null) return;
		_shopUI.Show(shopId);
	}

	public void HideShop()
	{
		if (_shopUI == null) return;
		_shopUI.Hide();
	}

	public void ShowConfirmPopup(string prefabName, string message, string confirm, string cancel,
	CommonConfirmPopup.ConfirmType type, Action confirmAction, Action cancelAction = null)
	{

		if(_currentPopup != null)
		{ return;
		}

		GameObject prefab = Resources.Load<GameObject>($"Prefab/Popup/{prefabName}");
		if (prefab == null)
		{
			Debug.LogError($"[UIManager] 팝업 프리팹을 찾을 수 없습니다: Prefab/Popup/{prefabName}");
			return;
		}
		GameObject popupObj = Instantiate(prefab, _popupRoot);
		_currentPopup = popupObj.GetComponent<CommonConfirmPopup>();
		_currentPopup.SetPopup(type, message, confirm, cancel, confirmAction, cancelAction);
	}

	public void ClearCurrentPopup()
	{
		_currentPopup = null;
	}

	public ItemDetailPopup CreateItemDetailPopup()
	{
		GameObject prefab = Resources.Load<GameObject>("Prefab/Popup/ItemDetailPopup");
		if (prefab == null)
		{
			Debug.LogError("[UIManager] 팝업 프리팹을 찾을 수 없습니다: Prefab/Popup/ItemDetailPopup");
			return null;
		}
		GameObject popupObj = Instantiate(prefab, _popupRoot);
		return popupObj.GetComponent<ItemDetailPopup>();
	}
}
