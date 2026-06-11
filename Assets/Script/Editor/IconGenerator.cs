using UnityEngine;
using UnityEditor;
using System.IO;

public static class IconGenerator
{
    private static readonly string SavePath = Application.dataPath + "/Resources/Icons/";

    [MenuItem("Tools/Generate Item Icons")]
    public static void GenerateAll()
    {
        Directory.CreateDirectory(SavePath);

        SavePotion("potion_hp_small",   new Color(0.90f, 0.10f, 0.15f, 1f), new Color(0.55f, 0.00f, 0.00f, 1f));
        SavePotion("potion_hp_medium",  new Color(1.00f, 0.20f, 0.25f, 1f), new Color(0.65f, 0.00f, 0.05f, 1f));
        SavePotion("potion_hp_large",   new Color(1.00f, 0.30f, 0.30f, 1f), new Color(0.70f, 0.05f, 0.05f, 1f));
        SavePotion("potion_antidote",   new Color(0.10f, 0.75f, 0.20f, 1f), new Color(0.00f, 0.45f, 0.05f, 1f));
        SavePotion("potion_stamina",    new Color(0.95f, 0.80f, 0.10f, 1f), new Color(0.60f, 0.45f, 0.00f, 1f));

        SaveSword("weapon_sword_01", new Color(0.75f, 0.75f, 0.80f, 1f), new Color(0.40f, 0.40f, 0.45f, 1f));
        SaveSword("weapon_sword_02", new Color(0.40f, 0.65f, 1.00f, 1f), new Color(0.10f, 0.30f, 0.75f, 1f));
        SaveSword("weapon_sword_03", new Color(1.00f, 0.85f, 0.20f, 1f), new Color(0.75f, 0.55f, 0.00f, 1f));

        SaveArmor("armor_leather", new Color(0.65f, 0.40f, 0.15f, 1f), new Color(0.35f, 0.20f, 0.05f, 1f));
        SaveArmor("armor_iron",    new Color(0.70f, 0.72f, 0.75f, 1f), new Color(0.35f, 0.36f, 0.40f, 1f));

        SaveRing("ring_magic", new Color(0.80f, 0.30f, 1.00f, 1f), new Color(0.45f, 0.05f, 0.65f, 1f));

        AssetDatabase.Refresh();
        Debug.Log("[IconGenerator] 아이콘 11개 생성 완료: " + SavePath);
    }

    // ───────── Potion ─────────
    private static void SavePotion(string name, Color liquid, Color rim)
    {
        var t = New32();
        Color cork = new Color(0.55f, 0.35f, 0.15f, 1f);
        Color dark = new Color(rim.r * 0.5f, rim.g * 0.5f, rim.b * 0.5f, 1f);
        Color shine = new Color(1f, 1f, 1f, 0.65f);

        // 병 몸통
        Fill(t, 10, 4, 22, 24, liquid);
        Fill(t, 9,  8, 23, 22, liquid);
        Outline(t,  9,  4, 23, 24, rim);
        Outline(t,  8,  8, 24, 22, dark);

        // 병목
        Fill(t, 13, 25, 19, 27, liquid);
        Outline(t, 13, 25, 19, 27, dark);

        // 코르크
        Fill(t, 13, 28, 19, 30, cork);
        Outline(t, 13, 28, 19, 30, dark);

        // 반짝임
        t.SetPixel(12, 20, shine);
        t.SetPixel(11, 19, shine);
        t.SetPixel(12, 19, shine);

        Save(t, name);
    }

    // ───────── Sword ─────────
    private static void SaveSword(string name, Color blade, Color dark)
    {
        var t = New32();
        Color guard = new Color(0.80f, 0.65f, 0.30f, 1f);
        Color guardDark = new Color(0.50f, 0.38f, 0.10f, 1f);
        Color handle = new Color(0.50f, 0.28f, 0.10f, 1f);
        Color handleDark = new Color(0.28f, 0.14f, 0.04f, 1f);
        Color shine = new Color(1f, 1f, 1f, 0.55f);

        // 칼날 (왼아래 → 오른위 대각선)
        for (int i = 0; i < 16; i++)
        {
            int bx = 4 + i;
            int by = 4 + i;
            t.SetPixel(bx,     by,     blade);
            t.SetPixel(bx + 1, by,     blade);
            t.SetPixel(bx,     by + 1, dark);
        }
        // 칼끝 포인트
        t.SetPixel(20, 21, blade);
        t.SetPixel(21, 22, blade);
        t.SetPixel(22, 23, dark);

        // 가드
        Fill(t, 7, 10, 12, 12, guard);
        Outline(t, 7, 10, 12, 12, guardDark);

        // 손잡이
        Fill(t, 4, 4, 7, 10, handle);
        Outline(t, 4, 4, 7, 10, handleDark);

        // 반짝임
        t.SetPixel(14, 16, shine);
        t.SetPixel(15, 17, shine);

        Save(t, name);
    }

    // ───────── Armor ─────────
    private static void SaveArmor(string name, Color body, Color dark)
    {
        var t = New32();
        Color metal = new Color(body.r * 1.2f, body.g * 1.2f, body.b * 1.2f, 1f);
        Color shine = new Color(1f, 1f, 1f, 0.50f);

        // 어깨 패드 (왼)
        Fill(t, 3, 20, 9, 27, body);
        Outline(t, 3, 20, 9, 27, dark);

        // 어깨 패드 (오)
        Fill(t, 23, 20, 29, 27, body);
        Outline(t, 23, 20, 29, 27, dark);

        // 흉갑
        Fill(t, 8, 6, 24, 22, body);
        Outline(t, 8, 6, 24, 22, dark);

        // 중앙 장식선
        Fill(t, 15, 8, 17, 21, dark);
        Fill(t, 11, 12, 21, 14, dark);

        // 반짝임
        t.SetPixel(10, 20, shine);
        t.SetPixel(11, 21, shine);
        t.SetPixel(10, 21, shine);

        Save(t, name);
    }

    // ───────── Ring ─────────
    private static void SaveRing(string name, Color gem, Color dark)
    {
        var t = New32();
        Color gold = new Color(1.00f, 0.80f, 0.20f, 1f);
        Color goldDark = new Color(0.65f, 0.50f, 0.05f, 1f);
        Color shine = new Color(1f, 1f, 1f, 0.70f);

        // 반지 밴드 (타원형 테두리)
        int cx = 16, cy = 13, rx = 10, ry = 6;
        for (int angle = 0; angle < 360; angle++)
        {
            double rad = angle * System.Math.PI / 180.0;
            int px = cx + (int)(rx * System.Math.Cos(rad));
            int py = cy + (int)(ry * System.Math.Sin(rad));
            t.SetPixel(px, py, gold);
            t.SetPixel(px + 1, py, gold);
            t.SetPixel(px, py + 1, goldDark);
        }

        // 보석 (다이아몬드형)
        Fill(t, 13, 19, 19, 24, gem);
        t.SetPixel(12, 22, gem); t.SetPixel(20, 22, gem);
        t.SetPixel(16, 26, gem); t.SetPixel(16, 17, gem);
        Outline(t, 13, 19, 19, 24, dark);

        // 반짝임
        t.SetPixel(14, 23, shine);
        t.SetPixel(15, 24, shine);

        Save(t, name);
    }

    // ───────── Helpers ─────────
    private static Texture2D New32()
    {
        var t = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                t.SetPixel(x, y, clear);
        return t;
    }

    private static void Fill(Texture2D t, int x0, int y0, int x1, int y1, Color c)
    {
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                t.SetPixel(x, y, c);
    }

    private static void Outline(Texture2D t, int x0, int y0, int x1, int y1, Color c)
    {
        for (int x = x0; x <= x1; x++) { t.SetPixel(x, y0, c); t.SetPixel(x, y1, c); }
        for (int y = y0; y <= y1; y++) { t.SetPixel(x0, y, c); t.SetPixel(x1, y, c); }
    }

    private static void Save(Texture2D t, string fileName)
    {
        t.Apply();
        File.WriteAllBytes(SavePath + fileName + ".png", t.EncodeToPNG());
        Object.DestroyImmediate(t);
    }
}
