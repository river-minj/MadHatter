using UnityEngine;

/// <summary>
/// Trigger 기반 입력 처리
/// OnTriggerEnter/Exit 으로 플레이어 입퇴장 감지
/// </summary>

public class TriggerInputHandler : MonoBehaviour, IInputStrategy
{
	private IInteractable interactable;
	private bool playerInRange = false;
	private bool isEnabled = true;

	public void Initialize(IInteractable target)
	{
		interactable = target;
	}
	public void Enable()
	{
		isEnabled = true;
	}
	public void Disable()
	{
		isEnabled = false;
		playerInRange = false;
	}

	void Start()
	{
		if (interactable == null)
		{
			interactable = GetComponent<IInteractable>();

			if (interactable == null)
			{
				Debug.LogError("TriggerInputHandler requires an IInteractable target.");
			}
		}

		Collider2D col = GetComponent<Collider2D>();
		if (col != null && !col.isTrigger)
		{
			Debug.LogWarning("Collider2D is not set as Trigger. Setting isTrigger to true.");
		}
	}

	//trigger 입퇴장
	void OnTriggerEnter2D(Collider2D other)
	{
		if (!isEnabled) return;

		if (other.CompareTag("Player"))
		{
			playerInRange = true;

			if (interactable is InteractionController controller)
			{
				controller.NotifyPlayerEnter(other.transform);
			}

			if (UIManager.Instance != null)
			{
				UIManager.Instance.ShowNPCPrompt("Press E to interact", transform);
			}

			Debug.Log("Player entered interaction range.");
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.CompareTag("Player"))
		{
			playerInRange = false;

			if (interactable is InteractionController controller)
			{
				controller.NotifyPlayerExit();
			}

			if (UIManager.Instance != null)
			{
				UIManager.Instance.HideNPCPrompt();
			}

			Debug.Log("Player exited interaction range.");
		}
	}


}
