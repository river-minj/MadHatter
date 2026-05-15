using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

// Tools > Build Map3 Shop
//
// 레이어 역할:
//   Ground : 외곽 벽 시각 + 카운터 시각 + NPC 구역 바닥
//   Road   : 플레이어 통행 바닥 (고객 구역, x=-4~4)
//   Walls  : 충돌 전용 (렌더러 비활성) — 외곽 + 카운터 전체 폭
//   Deco   : 장식
//
// 타일 배역:
//   -4_34       : 바닥 (Road/NPC구역)
//   -4_35       : 장식 (Deco)
//   -4_68/69/70 : 카운터 좌/중/우 (Ground 시각)
//   -2 시트     : 외곽/내부 벽 시각
//     21=상단좌모서리  22=상단수평  27=상단우모서리
//     51=좌우수직(공통)
//     상단타일=하단타일 공통 사용
public static class Map3ShopBuilder
{
    const int StartX = -6, EndX = 6;    // width 13
    const int StartY = -10, EndY = 9;   // height 20

    const int DoorL = -1, DoorR = 1;    // 출입구 하단 중앙 3타일

    const int CntY    = 6;              // 카운터 행
    const int NpcYMin = 7, NpcYMax = 8; // NPC 구역
    const int CntXMin = -4, CntXMax = 4;

    [MenuItem("Tools/Build Map3 Shop")]
    public static void Build()
    {
        const string prefabPath = "Assets/Resources/Prefab/Map/Map03_Grid.prefab";
        const string r4 = "Assets/Resources/Tile/Map3/1775563734620-4_";
        const string r2 = "Assets/Resources/Tile/Map3/1775563734620-2_";

        var floor = Load(r4 + "34.asset");
        var decoT = Load(r4 + "35.asset");
        var cL    = Load(r4 + "68.asset");
        var cC    = Load(r4 + "69.asset");
        var cR    = Load(r4 + "70.asset");

        // 외곽/내부 벽 시각: -2 시트
        var wTL = Load(r2 + "21.asset"); // 상단 좌모서리
        var wTH = Load(r2 + "22.asset"); // 상단/하단 수평 (공통)
        var wTR = Load(r2 + "27.asset"); // 상단 우모서리
        var wV  = Load(r2 + "51.asset"); // 좌/우 수직 (공통)

        var prefabObj = PrefabUtility.LoadPrefabContents(prefabPath);
        var maps = FindMaps(prefabObj);

        if (!maps.IsValid())
        {
            Debug.LogError("[Map3ShopBuilder] Ground/Road/Walls/Deco 레이어를 찾지 못했습니다.");
            PrefabUtility.UnloadPrefabContents(prefabObj);
            return;
        }

        maps.ground.ClearAllTiles();
        maps.road.ClearAllTiles();
        maps.walls.ClearAllTiles();
        maps.deco.ClearAllTiles();

        PaintCustomerFloor(maps.road, floor);
        PaintGroundZone(maps.ground, floor, cL, cC, cR, wTL, wTH, wTR, wV);
        PaintCollision(maps.walls, wTH);
        PaintDeco(maps.deco, decoT);

        PrefabUtility.SaveAsPrefabAsset(prefabObj, prefabPath);
        PrefabUtility.UnloadPrefabContents(prefabObj);
        Debug.Log("[Map3ShopBuilder] 완료.");
    }

    // Road: 고객 통행 구역 (x=-4~4로 좌우 1타일씩 축소) + 출입구 바닥
    static void PaintCustomerFloor(Tilemap tm, TileBase tile)
    {
        for (int x = StartX + 2; x <= EndX - 2; x++)
        for (int y = StartY + 1; y <= CntY - 1; y++)
            Set(tm, x, y, tile);

        for (int x = DoorL; x <= DoorR; x++)
            Set(tm, x, StartY, tile);
    }

    // Ground: 외곽 벽 + 내부 수직 벽 + 카운터 + NPC 구역
    static void PaintGroundZone(Tilemap tm, TileBase floor,
        TileBase cL, TileBase cC, TileBase cR,
        TileBase wTL, TileBase wTH, TileBase wTR, TileBase wV)
    {
        // 상단 벽 (모서리는 사용자가 에디터에서 수정 예정)
        Set(tm, StartX, EndY, wTL);
        Set(tm, EndX,   EndY, wTR);
        for (int x = StartX + 1; x <= EndX - 1; x++)
            Set(tm, x, EndY, wTH);

        // 하단 벽: 상단과 동일 타일 (출입구 제외)
        Set(tm, StartX, StartY, wTL);
        Set(tm, EndX,   StartY, wTR);
        for (int x = StartX + 1; x <= EndX - 1; x++)
            if (x < DoorL || x > DoorR)
                Set(tm, x, StartY, wTH);

        // 외곽 좌/우 수직 벽 (x=-6, x=6)
        for (int y = StartY + 1; y <= EndY - 1; y++)
        {
            Set(tm, StartX, y, wV);
            Set(tm, EndX,   y, wV);
        }

        // 내부 수직 벽 (x=-5, x=5): 바닥이 x=-4~4로 줄어든 빈틈 메우기
        // NPC 구역(y=7~8)은 나중에 바닥으로 덮어씀
        for (int y = StartY + 1; y <= EndY - 1; y++)
        {
            Set(tm, StartX + 1, y, wV);
            Set(tm, EndX   - 1, y, wV);
        }

        // 카운터 행 (y=6): x=-4~4 카운터 타일, x=-5/+5는 내부 수직 벽 유지
        Set(tm, CntXMin, CntY, cL);
        Set(tm, CntXMax, CntY, cR);
        for (int x = CntXMin + 1; x <= CntXMax - 1; x++)
            Set(tm, x, CntY, cC);

        // NPC 구역 바닥 (x=-5~5 전체, 내부 수직 벽 위에 덮어씀)
        for (int x = StartX + 1; x <= EndX - 1; x++)
        for (int y = NpcYMin; y <= NpcYMax; y++)
            Set(tm, x, y, floor);
    }

    // Walls: 충돌 전용 (렌더러 비활성, 타일 종류 무관)
    static void PaintCollision(Tilemap tm, TileBase tile)
    {
        // 외곽 테두리 전체
        for (int x = StartX; x <= EndX; x++)
        {
            Set(tm, x, EndY, tile);
            if (x < DoorL || x > DoorR)
                Set(tm, x, StartY, tile);
        }
        for (int y = StartY + 1; y <= EndY - 1; y++)
        {
            Set(tm, StartX, y, tile);
            Set(tm, EndX,   y, tile);
        }

        // 카운터: 좌우 벽 사이 전체 폭 차단 (양옆 빈틈 없음)
        for (int x = StartX + 1; x <= EndX - 1; x++)
            Set(tm, x, CntY, tile);
    }

    // Deco: 고객 구역 장식
    static void PaintDeco(Tilemap tm, TileBase tile)
    {
        int[] xs = { -3, 0, 3 };
        int[] ys = { 4, 1, -3, -7 };
        foreach (int x in xs)
        foreach (int y in ys)
            Set(tm, x, y, tile);
    }

    static void Set(Tilemap tm, int x, int y, TileBase tile) =>
        tm.SetTile(new Vector3Int(x, y, 0), tile);

    static TileBase Load(string path) =>
        AssetDatabase.LoadAssetAtPath<TileBase>(path);

    static Maps FindMaps(GameObject root)
    {
        var maps = new Maps();
        foreach (var tm in root.GetComponentsInChildren<Tilemap>(true))
            switch (tm.gameObject.name)
            {
                case "Ground": maps.ground = tm; break;
                case "Road":   maps.road   = tm; break;
                case "Walls":  maps.walls  = tm; break;
                case "Deco":   maps.deco   = tm; break;
            }
        return maps;
    }

    struct Maps
    {
        public Tilemap ground, road, walls, deco;
        public bool IsValid() => ground != null && road != null && walls != null && deco != null;
    }
}
