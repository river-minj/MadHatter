#if UNITY_EDITOR
using UnityEditor;

public class ExcelPostprocessor : AssetPostprocessor
{
	private static void OnPostprocessAllAssets(
		string[] importedAssets, string[] deletedAssets,
		string[] movedAssets, string[] movedFromAssetPaths)
	{
		foreach (string path in importedAssets)
		{
			if (path.StartsWith("Assets/Data/Excel/") && path.EndsWith(".xlsx"))
			{
				ExcelToJsonConverter.ConvertAll();
				return;
			}
		}
	}
}
#endif