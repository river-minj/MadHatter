#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class WebGLBuilder
{
    private const string BuildPath = "Build/WebGL";

    [MenuItem("Tools/Build WebGL (720x1280)")]
    public static void BuildWebGL()
    {
        PlayerSettings.defaultWebScreenWidth = 720;
        PlayerSettings.defaultWebScreenHeight = 1280;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;

        var scenes = EditorBuildSettings.scenes;
        var scenePaths = new string[scenes.Length];
        for (int i = 0; i < scenes.Length; i++)
            scenePaths[i] = scenes[i].path;

        var options = new BuildPlayerOptions
        {
            scenes = scenePaths,
            locationPathName = BuildPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[WebGLBuilder] Result: {report.summary.result}  |  Path: {BuildPath}");
    }
}
#endif
