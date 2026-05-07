using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬전환과 로딩을 담당하는 클래스
/// Start 씬에 존재, DontDestroyOnLoad(별도의 스크립트)
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] private LoadingUI _loadingUI;

    private bool _isLoading = false;

    private void Awake()
    {
		if (Instance != null && Instance != this)
        {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

    public void StartNewGame()
    {
		LoadScene("Main", true);
    }

    public void ContinueGame()
    {
		LoadScene("Main", false);
    }

    public void LoadScene(string sceneName, bool isNewGame = true)
    {
		if (_isLoading)
			return;

		StartCoroutine(LoadSceneRoutine(sceneName, isNewGame));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, bool isNewGame)
    {
		_isLoading = true;

		// 1. 로딩 UI 표시
		_loadingUI.Show();
		_loadingUI.SetProgress(0f, "로딩 준비 중...");
		yield return null;

		// 2. 세이브 데이터 로드 (이어하기)
		//if (!isNewGame)
		//{
		//	_loadingUI.SetProgress(0.1f, "세이브 데이터 로드 중...");

		//	SaveData data = GameSystem.Load();
		//	GameManager.Instance.LoadGame(data);
		//	yield return null;
		//}

		// 3. 씬 비동기 로드
		_loadingUI.SetProgress(0.2f, "씬 로드 중...");

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		if (asyncLoad == null)
		{
			Debug.LogError($"씬 '{sceneName}'을 찾을 수 없습니다. Build Settings를 확인하세요.");
			_loadingUI.Hide();
			_isLoading = false;
			yield break;
		}
		asyncLoad.allowSceneActivation = false;//즉시 씬 전환을 방지
		// allowSceneActivation = false
		//1. 씬 로드 진행(progress 0 ~0.9까지만 올라감)
		//2. 그 동안 로딩바 갱신
		//3. 준비 완료되면 allowSceneActivation = true
		//4. 그 때 실제 씬 전환 발생
		

		while (asyncLoad.progress < 0.9f)
		{
			float progress = 0.2f + (asyncLoad.progress / 0.9f) * 0.6f;
			_loadingUI.SetProgress(progress, "씬 로드 중...");
			yield return null;
		}

		// 4. 씬 활성화
		_loadingUI.SetProgress(0.9f, "초기화 중...");
		asyncLoad.allowSceneActivation = true;

		yield return new WaitUntil(() => asyncLoad.isDone); //씬 전환 완료 대기

		// 5. 매니저 Awake/Start 완료 대기
		yield return null;

		// 5-1. 게임 데이터 로드 (JSON → Database 적재)
		if (DataManager.Instance != null)
		{
			yield return DataManager.Instance.LoadAllDataAsync((progress, message) =>
			{
				float totalProgress = 0.9f + (progress * 0.05f);
				_loadingUI.SetProgress(totalProgress, message);
			});
		}

		// 6. 세이브 데이터 매니저에 주입
		if (!isNewGame)
		{
			_loadingUI.SetProgress(0.95f, "데이터 적용 중...");

			SaveData data = GameSystem.Load();
			GameManager.Instance.LoadGame(data);
			yield return null;
		}

		// 7. 완료
		_loadingUI.SetProgress(1f, "완료!");
		yield return new WaitForSeconds(0.3f);

		_loadingUI.Hide();
		_isLoading = false;
	}
}
