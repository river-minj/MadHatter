#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class ShopUIBuilder
{
    private const string SlotSavePath = "Assets/Resources/Prefab/Slot/ShopSlot.prefab";
    private const string UISavePath   = "Assets/Prefab/UI/ShopUI.prefab";

    // ──────────────────────────────────────────
    // 1) ShopSlot 프리팹 생성
    // ──────────────────────────────────────────
    [MenuItem("Tools/Build ShopSlot Prefab")]
    public static void BuildShopSlotPrefab()
    {
        GameObject root = BuildShopSlotObject();

        EnsureDirectory("Assets/Resources/Prefab/Slot");
        PrefabUtility.SaveAsPrefabAsset(root, SlotSavePath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[ShopUIBuilder] ShopSlot 프리팹 저장 완료: {SlotSavePath}");
    }

    // ──────────────────────────────────────────
    // 2) ShopUI 패널 프리팹 생성
    // ──────────────────────────────────────────
    [MenuItem("Tools/Build ShopUI Prefab")]
    public static void BuildShopUIPrefab()
    {
        // SlotPrefab 필요
        var slotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(SlotSavePath);
        if (slotPrefab == null)
        {
            Debug.LogError("[ShopUIBuilder] ShopSlot 프리팹이 없습니다. 먼저 Tools > Build ShopSlot Prefab 실행하세요.");
            return;
        }

        GameObject root = BuildShopUIObject(slotPrefab);

        EnsureDirectory("Assets/Prefab/UI");
        PrefabUtility.SaveAsPrefabAsset(root, UISavePath);
        Object.DestroyImmediate(root);

        AssetDatabase.Refresh();
        Debug.Log($"[ShopUIBuilder] ShopUI 프리팹 저장 완료: {UISavePath}");
    }

    // ──────────────────────────────────────────
    // 내부 빌더 - ShopSlot
    // ──────────────────────────────────────────
    private static GameObject BuildShopSlotObject()
    {
        // 루트 (Button + ShopSlotController)
        var root = new GameObject("ShopSlot");
        var rootRect = root.AddComponent<RectTransform>();
        rootRect.sizeDelta = new Vector2(640, 100);
        root.AddComponent<CanvasRenderer>();

        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.15f, 0.10f, 0.08f, 0.95f);

        var btn = root.AddComponent<Button>();
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.9f, 0.8f, 0.5f, 0.3f);
        colors.pressedColor     = new Color(0.7f, 0.6f, 0.3f, 0.5f);
        btn.colors = colors;

        var ctrl = root.AddComponent<ShopSlotController>();

        // 아이콘 (좌측)
        var iconGO = CreateChild(root, "Icon", new Vector2(50, 50), new Vector2(-260, 0));
        var icon = iconGO.AddComponent<Image>();
        icon.preserveAspect = true;

        // 아이템 이름
        var nameGO = CreateChild(root, "NameText", new Vector2(250, 36), new Vector2(-80, 18));
        var nameTmp = nameGO.AddComponent<TextMeshProUGUI>();
        nameTmp.text = "아이템 이름";
        nameTmp.fontSize = 22;
        nameTmp.color = new Color(0.95f, 0.90f, 0.75f);
        nameTmp.fontStyle = FontStyles.Bold;
        nameTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // 재고 텍스트
        var stockGO = CreateChild(root, "StockText", new Vector2(250, 28), new Vector2(-80, -14));
        var stockTmp = stockGO.AddComponent<TextMeshProUGUI>();
        stockTmp.text = "재고 99";
        stockTmp.fontSize = 18;
        stockTmp.color = new Color(0.7f, 0.7f, 0.7f);
        stockTmp.alignment = TextAlignmentOptions.MidlineLeft;

        // 가격 텍스트 (우측)
        var priceGO = CreateChild(root, "PriceText", new Vector2(160, 40), new Vector2(220, 0));
        var priceTmp = priceGO.AddComponent<TextMeshProUGUI>();
        priceTmp.text = "100 G";
        priceTmp.fontSize = 24;
        priceTmp.color = new Color(1f, 0.85f, 0.2f);
        priceTmp.fontStyle = FontStyles.Bold;
        priceTmp.alignment = TextAlignmentOptions.MidlineRight;

        // 품절 오버레이
        var soldOutGO = CreateChild(root, "SoldOutOverlay", new Vector2(640, 100), Vector2.zero);
        var soldOutBg = soldOutGO.AddComponent<Image>();
        soldOutBg.color = new Color(0, 0, 0, 0.6f);
        var soldOutText = CreateChild(soldOutGO, "SoldOutText", new Vector2(200, 50), Vector2.zero);
        var soldOutTmp = soldOutText.AddComponent<TextMeshProUGUI>();
        soldOutTmp.text = "품절";
        soldOutTmp.fontSize = 30;
        soldOutTmp.color = new Color(0.9f, 0.3f, 0.3f);
        soldOutTmp.fontStyle = FontStyles.Bold;
        soldOutTmp.alignment = TextAlignmentOptions.Center;
        soldOutGO.SetActive(false);

        // ShopSlotController 필드 연결
        var so = new SerializedObject(ctrl);
        so.FindProperty("_icon").objectReferenceValue           = icon;
        so.FindProperty("_nameText").objectReferenceValue       = nameTmp;
        so.FindProperty("_priceText").objectReferenceValue      = priceTmp;
        so.FindProperty("_stockText").objectReferenceValue      = stockTmp;
        so.FindProperty("_slotButton").objectReferenceValue     = btn;
        so.FindProperty("_soldOutOverlay").objectReferenceValue = soldOutGO;
        so.ApplyModifiedProperties();

        return root;
    }

    // ──────────────────────────────────────────
    // 내부 빌더 - ShopUI 패널
    // ──────────────────────────────────────────
    private static GameObject BuildShopUIObject(GameObject slotPrefab)
    {
        // 루트 ShopUI
        var root = new GameObject("ShopUI");
        root.AddComponent<RectTransform>();
        var shopUI = root.AddComponent<ShopUI>();

        // 패널 (반투명 배경)
        var panel = new GameObject("ShopPanel");
        panel.transform.SetParent(root.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0);
        panelRect.anchorMax = new Vector2(1, 0);
        panelRect.pivot     = new Vector2(0.5f, 0f);
        panelRect.sizeDelta = new Vector2(0, 900);
        panelRect.anchoredPosition = Vector2.zero;
        panel.AddComponent<CanvasRenderer>();
        var panelBg = panel.AddComponent<Image>();
        panelBg.color = new Color(0.08f, 0.05f, 0.03f, 0.97f);

        // 타이틀
        var titleGO = CreateChild(panel, "TitleText", new Vector2(600, 60), new Vector2(0, -40));
        var titleRect = titleGO.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot     = new Vector2(0.5f, 1f);
        var titleTmp = titleGO.AddComponent<TextMeshProUGUI>();
        titleTmp.text      = "상  점";
        titleTmp.fontSize  = 34;
        titleTmp.color     = new Color(1f, 0.85f, 0.4f);
        titleTmp.fontStyle = FontStyles.Bold;
        titleTmp.alignment = TextAlignmentOptions.Center;

        // 닫기 버튼
        var closeGO = CreateChild(panel, "CloseButton", new Vector2(70, 70), new Vector2(0, -40));
        var closeRect = closeGO.GetComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot     = new Vector2(1f, 1f);
        closeRect.anchoredPosition = new Vector2(-20, -20);
        closeGO.AddComponent<CanvasRenderer>();
        var closeBg = closeGO.AddComponent<Image>();
        closeBg.color = new Color(0.6f, 0.15f, 0.1f);
        var closeBtn = closeGO.AddComponent<Button>();
        var closeTxt = CreateChild(closeGO, "Text", new Vector2(70, 70), Vector2.zero);
        var closeTmp = closeTxt.AddComponent<TextMeshProUGUI>();
        closeTmp.text      = "✕";
        closeTmp.fontSize  = 32;
        closeTmp.color     = Color.white;
        closeTmp.alignment = TextAlignmentOptions.Center;

        // ScrollRect 영역
        var scrollGO = new GameObject("ScrollView");
        scrollGO.transform.SetParent(panel.transform, false);
        var scrollRect2 = scrollGO.AddComponent<RectTransform>();
        scrollRect2.anchorMin = new Vector2(0, 0);
        scrollRect2.anchorMax = new Vector2(1, 1);
        scrollRect2.offsetMin = new Vector2(20, 20);
        scrollRect2.offsetMax = new Vector2(-20, -100);
        var scrollComp = scrollGO.AddComponent<ScrollRect>();
        scrollComp.horizontal = false;

        // Viewport
        var viewportGO = CreateChild(scrollGO, "Viewport", Vector2.zero, Vector2.zero);
        var vpRect = viewportGO.GetComponent<RectTransform>();
        vpRect.anchorMin = Vector2.zero;
        vpRect.anchorMax = Vector2.one;
        vpRect.sizeDelta = Vector2.zero;
        viewportGO.AddComponent<CanvasRenderer>();
        viewportGO.AddComponent<Image>().color = new Color(0, 0, 0, 0);
        var mask = viewportGO.AddComponent<Mask>();
        mask.showMaskGraphic = false;
        scrollComp.viewport = vpRect;

        // Content
        var contentGO = CreateChild(viewportGO, "Content", Vector2.zero, Vector2.zero);
        var contentRect = contentGO.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot     = new Vector2(0.5f, 1f);
        contentRect.sizeDelta = new Vector2(0, 0);
        scrollComp.content = contentRect;

        // InfiniteScrollView 컴포넌트 (ScrollView 오브젝트에 붙임)
        var infiniteScroll = scrollGO.AddComponent<InfiniteScrollView>();
        var isv = new SerializedObject(infiniteScroll);
        isv.FindProperty("_scrollRect").objectReferenceValue = scrollComp;
        isv.FindProperty("_content").objectReferenceValue    = contentRect;
        isv.FindProperty("_columns").intValue  = 1;
        isv.FindProperty("_spacing").vector2Value = new Vector2(0, 8);
        isv.FindProperty("_paddingLeft").intValue   = 20;
        isv.FindProperty("_paddingRight").intValue  = 20;
        isv.FindProperty("_paddingTop").intValue    = 10;
        isv.FindProperty("_paddingBottom").intValue = 10;

        // SlotPrefab 연결
        var slotCtrl = slotPrefab.GetComponent<ShopSlotController>();
        isv.FindProperty("_slotPrefab").objectReferenceValue = slotCtrl;
        isv.ApplyModifiedProperties();

        // ShopUI 필드 연결
        var suiSo = new SerializedObject(shopUI);
        suiSo.FindProperty("_shopPanel").objectReferenceValue    = panel;
        suiSo.FindProperty("_shopTitleText").objectReferenceValue = titleTmp;
        suiSo.FindProperty("_scrollView").objectReferenceValue   = infiniteScroll;
        suiSo.FindProperty("_closeButton").objectReferenceValue  = closeBtn;
        suiSo.ApplyModifiedProperties();

        return root;
    }

    // ──────────────────────────────────────────
    // 유틸
    // ──────────────────────────────────────────
    private static GameObject CreateChild(GameObject parent, string name, Vector2 size, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin       = new Vector2(0.5f, 0.5f);
        rt.anchorMax       = new Vector2(0.5f, 0.5f);
        rt.pivot           = new Vector2(0.5f, 0.5f);
        rt.sizeDelta       = size;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    private static void EnsureDirectory(string path)
    {
        var parts = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
#endif
