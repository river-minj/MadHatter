using UnityEngine;

public abstract class InteractionController : MonoBehaviour, IInteractable
{

    [SerializeField] protected bool isInteractable = true; //인터렉션 가능여부
	[SerializeField] protected string interactionMessage = "Press 'E' to interact"; //인터랙션 메시지

	protected bool playerInside = false; //플레이어가 인터렉션 범위 안에 있는지 여부
	protected Transform playerTransform; //플레이어 트랜스폼 참조


	//IInteractable 구현 > 수행할 행동
	public virtual void Interact(PlayerController player)

	{
		if (CanInteract() == false)
		{
			Debug.Log("Do Not Interacted with " + gameObject.name);
			return;
		}

		Debug.Log("Interacted with " + gameObject.name);
		OnInteract();
	}

	//iinteractable 구현 > 인터랙션 가능 여부
	public virtual bool CanInteract()
	{
		//인터랙션 가능 여부 확인
		return isInteractable;
	}

	public virtual string GetInteractionMessage()
	{
		//인터랙션 메시지 반환
		return interactionMessage;
	}
	
	//자식 클래스에서 필수 구현
	protected abstract void OnInteract();

	//입력 핸들러용 public API
	public void NotifyPlayerEnter(Transform player)
	{
		playerInside = true;
		playerTransform = player;

		OnPlayerEnter();
	}

	public void NotifyPlayerExit()
	{
		playerInside = false;
		playerTransform = null;
	
		OnPlayerExit();
	}

	//자식 클래스에서 필요에 따라 오버라이드 가능
	protected virtual void OnPlayerEnter()
	{
		//플레이어가 인터랙션 범위에 들어왔을 때의 기본 동작 (서브클래스에서 오버라이드 가능)
		Debug.Log("Player entered interaction range of " + gameObject.name);
	}

	protected virtual void OnPlayerExit()
	{
		//플레이어가 인터랙션 범위에서 나갔을 때의 기본 동작 (서브클래스에서 오버라이드 가능)
		Debug.Log("Player exited interaction range of " + gameObject.name);
	}

}
