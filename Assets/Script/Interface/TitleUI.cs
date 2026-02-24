using UnityEngine;
using UnityEngine.UI;

public class TitleUI : MonoBehaviour
{
	[SerializeField] Button _continueButton;
	private void Start()
	{
		// to do : 세이브 데이터 존재 여부에 따라 이어하기 버튼 활성화/비활성화
		if(_continueButton != null)
		{
			_continueButton.interactable = false; // 아직 세이브 시스템 없음
		}
	}

	public void OnNewGame()
	{
		SceneLoader.Instance.StartNewGame();
	}

	public void OnContinue()
	{
		SceneLoader.Instance.ContinueGame();
	}

	public void OnOptions()
	{
		//to do : 옵션 ui
		Debug.Log("[TitleUI] 옵션 UI 제작 예정");
	}


	public void OnQuit()
	{
		Debug.Log("[TitleUI] 게임 종료");

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
	}

}
