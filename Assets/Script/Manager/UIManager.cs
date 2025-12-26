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
		}
	}

	private void Start()
	{
		if(npcPrompt != null)
		{
			npcPrompt = FindObjectOfType<NPCPromptUI>();

			if(npcPrompt == null)
			{
				Debug.LogWarning("NPCPromptUI not found in the scene.");
			}
		}
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
