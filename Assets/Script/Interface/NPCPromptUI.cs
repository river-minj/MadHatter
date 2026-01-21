using TMPro;
using UnityEngine;

public class NPCPromptUI : MonoBehaviour
{
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private TextMeshProUGUI promptText;
	[SerializeField] private RectTransform promptRect;

	[SerializeField] private Vector2 offset = new Vector2(0, 80); // NPC 위치로부터의 오프셋

	private Camera mainCamera;
	private Transform currentTarget;


	// Start is called before the first frame update
	void Start()
    {
		mainCamera = Camera.main;

		if(mainCamera == null)
		{
			Debug.LogError("Do not find Main Camera");
		}

		if(promptRect == null && promptPanel  != null)
		{
			promptRect = promptPanel.GetComponent<RectTransform>();
		}



        Hide();
    }

	private void Update()
	{

		if(promptPanel != null && promptPanel.activeSelf && currentTarget != null)
		{
			UpdatePosition();
		}
	}

	public void Hide()
    {
		currentTarget = null;

        if(promptPanel != null)
        {
            promptPanel.SetActive(false);
		}
    }

	public void Show(string message, Transform npcTransform)
	{
		if (promptPanel == null)
		{
			Debug.LogWarning("Prompt panel is not assigned in the inspector.");
			return;
		}

		if (npcTransform == null)
		{
			Debug.LogWarning("Target Transform is null");
			return;
		}

		currentTarget = npcTransform;

		if (promptText != null)
		{
			promptText.text = message;
		}

		UpdatePosition();
		promptPanel.SetActive(true);
	}

	void UpdatePosition()
	{
		if (mainCamera == null || promptRect == null || currentTarget == null)
			return;

		Vector3 worldPosition = currentTarget.position;
		Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
		Vector2 finalPosition = screenPosition + offset;

		float panelHeight = promptRect.rect.height;
		Vector2 pivot = promptRect.pivot;

		//Debug.Log($"[NPCPromptUI] 스크린: {screenPosition}, 초기: {finalPosition}");

		// 상단 체크
		float topEdge = finalPosition.y + (panelHeight * (1 - pivot.y));
		if (topEdge > Screen.height)
		{
			finalPosition = screenPosition - new Vector2(0, offset.y);
			//Debug.Log($"[NPCPromptUI] 상단 벗어남 ({topEdge} > {Screen.height}) -> 아래로");
		}

		// 하단 체크
		float bottomEdge = finalPosition.y - (panelHeight * pivot.y);
		if (bottomEdge < 0)
		{
			finalPosition.y = panelHeight * pivot.y + 10;
			//Debug.Log($"[NPCPromptUI] 하단 벗어남 ({bottomEdge} < 0) -> 위로");
		}

		//Debug.Log($"[NPCPromptUI] 최종: {finalPosition}");

		promptRect.position = finalPosition;
	}
}
