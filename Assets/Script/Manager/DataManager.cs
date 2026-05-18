using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class DataManager : MonoBehaviour
{
	public static DataManager Instance { get; private set; }

	public bool IsLoaded => _isLoaded;
	private bool _isLoaded = false;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}

	public IEnumerator LoadAllDataAsync(Action<float, string> onProgress = null)
	{
		if (_isLoaded) yield break;
		// Database 인스턴스 생성
		DialogueDatabase.CreateInstance();
		QuestDatabase.CreateInstance();
		NpcDatabase.CreateInstance();
		CompanionDatabase.CreateInstance();
		ItemDatabase.CreateInstance();
		DropDatabase.CreateInstance();
		ShopDatabase.CreateInstance();

		onProgress?.Invoke(0f, "대화 데이터 로드 중...");
		var dialogueList = LoadTable<DialogueTableData>("DialogueTable");
		DialogueDatabase.Instance.ApplyData(dialogueList);
		yield return null;

		onProgress?.Invoke(0.33f, "퀘스트 데이터 로드 중...");
		var questList = LoadTable<QuestTableData>("QuestTable");
		yield return null;

		onProgress?.Invoke(0.66f, "보상 데이터 로드 중...");
		var rewardList = LoadTable<RewardTableData>("RewardTable");
		QuestDatabase.Instance.ApplyData(questList, rewardList);
		yield return null;

		onProgress?.Invoke(0.75f, "NPC 데이터 로드 중...");
		var npcList = LoadTable<NpcTableData>("NpcTable");
		NpcDatabase.Instance.ApplyData(npcList);
		yield return null;

		onProgress?.Invoke(0.85f, "동료 데이터 로드 중...");
		var companionList = LoadTable<CompanionTableData>("CompanionTable");
		CompanionDatabase.Instance.ApplyData(companionList);
		yield return null;

		onProgress?.Invoke(0.92f, "아이템 데이터 로드 중...");
		var itemList = LoadTable<ItemTableData>("ItemTable");
		ItemDatabase.Instance.ApplyData(itemList);
		yield return null;


		onProgress?.Invoke(0.96f, "드롭 데이터 로드 중...");
		var dropList = LoadTable<DropTableData>("DropTable");
		DropDatabase.Instance.ApplyData(dropList);
		yield return null;

		onProgress?.Invoke(0.98f, "상점 데이터 로드 중...");
		var shopList = LoadTable<ShopTableData>("ShopTable");
		ShopDatabase.Instance.ApplyData(shopList);
		yield return null;

		_isLoaded = true;
		onProgress?.Invoke(1f, "데이터 로드 완료!");
	}

	private List<T> LoadTable<T>(string tableName)
	{
		TextAsset json = Resources.Load<TextAsset>($"Json/{tableName}");

		if (json == null)
		{
			Debug.LogError($"[DataManager] JSON 파일을 찾을 수 없습니다: Json/{tableName}");
			return new List<T>();
		}

		try
		{
			var list = JsonConvert.DeserializeObject<List<T>>(json.text);
			return list ?? new List<T>();
		}
		catch (Exception e)
		{
			Debug.LogError($"[DataManager] JSON 파싱 실패 ({tableName}): {e.Message}");
			return new List<T>();
		}
	}
}