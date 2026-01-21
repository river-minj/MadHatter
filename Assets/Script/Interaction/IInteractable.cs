public interface IInteractable
{
    //인터랙션 할수 있는 대상에 확장할 때 사용
    void Interact(PlayerController player); //수행할 인터랙션
    bool CanInteract(); //인터랙션 가능여부
}
