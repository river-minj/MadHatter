#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 개발용 치트 콘솔. F12로 토글.
/// UNITY_EDITOR 또는 DEVELOPMENT_BUILD에서만 컴파일됨.
/// </summary>
public class CheatConsole : MonoBehaviour
{
    private bool _isOpen = false;

    private string _goldInput   = "";
    private string _levelInput  = "";
    private string _atkInput    = "";
    private string _companionInput = "";

    private string _feedback = "";
    private float  _feedbackTimer = 0f;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F12))
            _isOpen = !_isOpen;

        if (_feedbackTimer > 0f)
            _feedbackTimer -= Time.deltaTime;
    }

    // 세로 기준 가상 해상도 (이 값에 맞춰 스케일 계산)
    private const float RefH = 480f;

    private void OnGUI()
    {
        if (!_isOpen) return;

        // 세로는 화면 절반, 가로는 꽉 채움
        float panelH = Screen.height * 0.5f;
        float scale  = panelH / RefH;

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1f));

        // 스케일 공간에서의 가로 너비
        float scaledW = Screen.width / scale;

        GUILayout.BeginArea(new Rect(0, 0, scaledW, RefH));
        GUILayout.BeginVertical(GUI.skin.box);

        GUILayout.Label("[ Cheat Console ]  F12 to close");
        GUILayout.Space(6);

        // ── Player ──────────────────────────────
        GUILayout.Label("─ Player ─────────────────────────────────────");
        DrawStatRow("Gold",    ref _goldInput,  Current_Gold(),  OnSetGold,  scaledW);
        DrawStatRow("Level",   ref _levelInput, Current_Level(), OnSetLevel, scaledW);
        DrawStatRow("BaseAtk", ref _atkInput,   Current_Atk(),   OnSetAtk,   scaledW);

        GUILayout.Space(10);

        // ── Companion ───────────────────────────
        GUILayout.Label("─ Companion ──────────────────────────────────");

        GUILayout.BeginHorizontal();
        GUILayout.Label("ID", GUILayout.Width(scaledW * 0.08f));
        _companionInput = GUILayout.TextField(_companionInput, GUILayout.Width(scaledW * 0.55f));
        if (GUILayout.Button("Add", GUILayout.Width(scaledW * 0.12f)))
            OnAddCompanion(_companionInput.Trim());
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        foreach (var comp in CompanionDatabase.Instance.GetAllCompanions())
        {
            if (GUILayout.Button(comp._companionName, GUILayout.Width(scaledW * 0.2f)))
                OnAddCompanion(comp._companionId);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        // ── System ──────────────────────────────
        GUILayout.Label("─ System ─────────────────────────────────────");
        if (GUILayout.Button("세이브 데이터 초기화", GUILayout.Width(scaledW * 0.25f)))
            OnResetSave();

        // ── Feedback ────────────────────────────
        if (_feedbackTimer > 0f)
        {
            GUILayout.Space(6);
            GUILayout.Label(">> " + _feedback);
        }

        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    // ── Stat row helper ─────────────────────────────────────────────────────

    private void DrawStatRow(string label, ref string input, string current, System.Action<int> onSet, float rowW)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label($"{label} ({current})", GUILayout.Width(rowW * 0.3f));
        input = GUILayout.TextField(input, GUILayout.Width(rowW * 0.4f));
        if (GUILayout.Button("Set", GUILayout.Width(rowW * 0.12f)) && int.TryParse(input, out int val))
            onSet(val);
        GUILayout.EndHorizontal();
    }

    // ── Current value helpers ────────────────────────────────────────────────

    private string Current_Gold()  => PlayerInfoManager.Instance != null
        ? PlayerInfoManager.Instance.PlayerInfo._gold.ToString() : "-";

    private string Current_Level() => PlayerInfoManager.Instance != null
        ? PlayerInfoManager.Instance.PlayerInfo._level.ToString() : "-";

    private string Current_Atk()   => PlayerInfoManager.Instance != null
        ? PlayerInfoManager.Instance.Atk.ToString() : "-";

    // ── Handlers ────────────────────────────────────────────────────────────

    private void OnSetGold(int val)
    {
        PlayerInfoManager.Instance.SetGold(val);
        ShowFeedback($"Gold → {val}");
    }

    private void OnSetLevel(int val)
    {
        PlayerInfoManager.Instance.SetLevel(val);
        ShowFeedback($"Level → {val}");
    }

    private void OnSetAtk(int val)
    {
        PlayerInfoManager.Instance.SetBaseAtk(val);
        ShowFeedback($"BaseAtk → {val}");
    }

    private void OnAddCompanion(string companionId)
    {
        if (string.IsNullOrEmpty(companionId)) return;

        var data = CompanionDatabase.Instance.GetCompanionById(companionId);
        if (data == null) { ShowFeedback($"없는 ID: {companionId}"); return; }

        CompanionManager.Instance.AddCompanion(data);
        ShowFeedback($"{data._companionName} 추가");
    }

    private void OnResetSave()
    {
        GameSystem.Delete();
        SceneManager.LoadScene(0);
        ShowFeedback("세이브 초기화 → 씬 재로드");
    }

    private void ShowFeedback(string msg)
    {
        _feedback      = msg;
        _feedbackTimer = 2.5f;
    }
}
#endif
