using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Tools > Setup Map Transistors 실행 시:
//   1. Map01_Grid  : 기존 MapTransistor에 _nextMapMc = Map02_Collider 재연결
//   2. Map03_Grid  : MapTransistor 오브젝트 신규 추가, _nextMapMc = Map01_Grid
// 1회성 설정 스크립트 — 실행 후 삭제해도 무방
public static class MapTransistorSetup
{
    const string Map01Path = "Assets/Resources/Prefab/Map/Map01_Grid.prefab";
    const string Map02Path = "Assets/Resources/Prefab/Map/Map02_Collider.prefab";
    const string Map03Path = "Assets/Resources/Prefab/Map/Map03_Grid.prefab";

    [MenuItem("Tools/Setup Map Transistors")]
    public static void Setup()
    {
        SetupMap01();
        SetupMap03();
        AssetDatabase.SaveAssets();
        Debug.Log("[MapTransistorSetup] 완료.");
    }

    // Map01: 기존 MapTransistor._nextMapMc = Map02 재연결
    static void SetupMap01()
    {
        var map02Mc = GetMapController(Map02Path);
        if (map02Mc == null) { Debug.LogError("[Setup] Map02 MapController를 찾지 못했습니다."); return; }

        var prefab = PrefabUtility.LoadPrefabContents(Map01Path);
        var transistor = prefab.GetComponentInChildren<MapTransistor>(true);
        if (transistor == null) { Debug.LogError("[Setup] Map01 MapTransistor를 찾지 못했습니다."); PrefabUtility.UnloadPrefabContents(prefab); return; }

        SetField(transistor, "_nextMapMc", map02Mc);

        PrefabUtility.SaveAsPrefabAsset(prefab, Map01Path);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("[MapTransistorSetup] Map01 MapTransistor._nextMapMc → Map02 설정 완료.");
    }

    // Map03: MapTransistor 신규 추가, _nextMapMc = Map01
    static void SetupMap03()
    {
        var map01Mc = GetMapController(Map01Path);
        if (map01Mc == null) { Debug.LogError("[Setup] Map01 MapController를 찾지 못했습니다."); return; }

        var prefab = PrefabUtility.LoadPrefabContents(Map03Path);

        // 중복 실행 방지
        if (prefab.GetComponentInChildren<MapTransistor>(true) != null)
        {
            Debug.LogWarning("[MapTransistorSetup] Map03에 이미 MapTransistor가 있습니다. 건너뜁니다.");
            PrefabUtility.UnloadPrefabContents(prefab);
            return;
        }

        // MapTransistor GameObject 생성 및 계층 배치
        var go = new GameObject("MapTransistor");
        go.transform.SetParent(prefab.transform, false);
        go.transform.localPosition = new Vector3(0f, -9.5f, 0f); // 출입구 안쪽 (y=-10 문 바로 위)

        // BoxCollider2D: 문 너비 3타일 (x=-1~1), 높이 0.5
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(3f, 0.5f);
        col.offset = Vector2.zero;

        // MapTransistor 컴포넌트
        var transistor = go.AddComponent<MapTransistor>();
        SetField(transistor, "_nextMapMc", map01Mc);
        SetField(transistor, "_spawnPointId", (int)SpawnPointId.Default);

        PrefabUtility.SaveAsPrefabAsset(prefab, Map03Path);
        PrefabUtility.UnloadPrefabContents(prefab);
        Debug.Log("[MapTransistorSetup] Map03 MapTransistor 추가 완료. _nextMapMc → Map01, SpawnPoint → Default.");
    }

    static MapController GetMapController(string prefabPath)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        return asset != null ? asset.GetComponent<MapController>() : null;
    }

    static void SetField(Object target, string fieldName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        else Debug.LogWarningFormat("[MapTransistorSetup] 필드 '{0}'를 찾지 못했습니다.", fieldName);
    }

    static void SetField(Object target, string fieldName, int value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null) { prop.intValue = value; so.ApplyModifiedPropertiesWithoutUndo(); }
        else Debug.LogWarningFormat("[MapTransistorSetup] 필드 '{0}'를 찾지 못했습니다.", fieldName);
    }
}
