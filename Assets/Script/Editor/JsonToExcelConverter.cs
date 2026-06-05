#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

public static class JsonToExcelConverter
{
    private static readonly string ExcelFolderPath = "Assets/Data/Excel";
    private static readonly string JsonFolderPath = "Assets/Resources/Json";

    private static readonly Dictionary<string, Type> TableTypeMap = new Dictionary<string, Type>
    {
        { "DialogueTable", typeof(DialogueTableData) },
        { "QuestTable", typeof(QuestTableData) },
        { "RewardTable", typeof(RewardTableData) },
        { "NpcTable", typeof(NpcTableData) },
        { "CompanionTable", typeof(CompanionTableData) },
        { "ItemTable", typeof(ItemTableData) },
        { "DropTable", typeof(DropTableData) },
        { "ShopTable", typeof(ShopTableData) },
    };

    [MenuItem("Tools/Sync Excel from JSON")]
    public static void SyncAll()
    {
        int count = 0;
        foreach (var kvp in TableTypeMap)
        {
            string jsonPath = Path.Combine(JsonFolderPath, $"{kvp.Key}.json");
            string excelPath = Path.Combine(ExcelFolderPath, $"{kvp.Key}.xlsx");

            if (!File.Exists(jsonPath))
            {
                Debug.LogWarning($"[JsonToExcel] {kvp.Key}: JSON 파일 없음, 건너뜀");
                continue;
            }

            SyncTable(jsonPath, excelPath, kvp.Key, kvp.Value);
            count++;
        }

        AssetDatabase.Refresh();
        Debug.Log($"[JsonToExcel] 동기화 완료: {count}개 테이블");
    }

    private static void SyncTable(string jsonPath, string excelPath, string tableName, Type tableType)
    {
        try
        {
            string json = File.ReadAllText(jsonPath);
            var rows = JsonConvert.DeserializeObject<List<JObject>>(json);

            if (rows == null || rows.Count == 0)
            {
                Debug.LogWarning($"[JsonToExcel] {tableName}: JSON 데이터 없음");
                return;
            }

            FieldInfo[] fields = tableType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            var fileInfo = new FileInfo(excelPath);

            using (var package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet sheet;
                if (package.Workbook.Worksheets.Count == 0)
                    sheet = package.Workbook.Worksheets.Add("Sheet1");
                else
                    sheet = package.Workbook.Worksheets[1];

                sheet.Cells.Clear();

                // 1행: 헤더
                for (int i = 0; i < fields.Length; i++)
                    sheet.Cells[1, i + 1].Value = fields[i].Name;

                // 2행~: 데이터
                for (int r = 0; r < rows.Count; r++)
                {
                    for (int c = 0; c < fields.Length; c++)
                    {
                        JToken token = rows[r][fields[c].Name];
                        sheet.Cells[r + 2, c + 1].Value = token == null || token.Type == JTokenType.Null
                            ? ""
                            : token.ToString();
                    }
                }

                package.Save();
            }

            Debug.Log($"[JsonToExcel] {tableName}: {rows.Count}행 동기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[JsonToExcel] {tableName} 실패: {e.Message}");
        }
    }
}
#endif
