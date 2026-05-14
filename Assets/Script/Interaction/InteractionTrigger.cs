using UnityEngine;

/// <summary>
/// Trigger 기반 입력 처리
/// OnTriggerEnter/Exit 으로 플레이어 입퇴장 감지
/// </summary>

public class InteractionTrigger : MonoBehaviour
{
	private IInteractable interactable;
	private bool isEnabled = true;
	private bool playerInRange = false;

	private PlayerController _playercontroller;

	void Start()
	{
		if (interactable == null)
		{
			interactable = GetComponent<IInteractable>();

			if (interactable == null)
			{
				Debug.LogError("InteractionTrigger requires an IInteractable target.");
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
			_playercontroller = other.GetComponent<PlayerController>();
			_playercontroller.SetInteractable(interactable);

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
			//플레이어가 상호작용 하는 범위를 벗어났을 때 인터렉션 대상을 초기화
			_playercontroller.ClearInteractable();
			playerInRange = false;
			_playercontroller = null;

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
