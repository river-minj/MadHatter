#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using OfficeOpenXml;
using UnityEditor;
using UnityEngine;

public static class ExcelToJsonConverter //static : 인스턴스가 필요없는 유틸리티
{
	private static readonly string ExcelFolderPath = "Assets/Data/Excel";
	private static readonly string JsonFolderPath = "Assets/Resources/Json";

	// 테이블명 → TableData 클래스 매핑
	private static readonly Dictionary<string, Type> TableTypeMap = new Dictionary<string, Type>
	{
		{ "DialogueTable", typeof(DialogueTableData) },
		{ "QuestTable", typeof(QuestTableData) },
		{ "RewardTable", typeof(RewardTableData) },
		{ "NpcTable", typeof(NpcTableData) },
		{ "CompanionTable", typeof(CompanionTableData) },
		{ "ItemTable", typeof(ItemTableData) },
		{ "DropTable", typeof(DropTableData) },

	};

	[MenuItem("Tools/Convert All Excel to JSON")]
	public static void ConvertAll()
	{
		if (!Directory.Exists(ExcelFolderPath))
		{
			Debug.LogError($"[ExcelToJson] 엑셀 폴더가 없습니다: {ExcelFolderPath}");
			return;
		}

		if (!Directory.Exists(JsonFolderPath))
		{
			Directory.CreateDirectory(JsonFolderPath);
		}

		string[] excelFiles = Directory.GetFiles(ExcelFolderPath, "*.xlsx");
		int convertedCount = 0;

		foreach (string filePath in excelFiles)
		{
			// 엑셀 임시 파일(~$) 무시
			if (Path.GetFileName(filePath).StartsWith("~$"))
				continue;

			string tableName = Path.GetFileNameWithoutExtension(filePath);

			if (TableTypeMap.TryGetValue(tableName, out var tableType))
			{
				ConvertExcel(filePath, tableName, tableType);
				convertedCount++;
			}
			else
			{
				Debug.LogWarning($"[ExcelToJson] {tableName}에 매핑된 TableData 클래스가 없습니다. TableTypeMap에 등록해주세요.");
			}
		}

		AssetDatabase.Refresh();
		Debug.Log($"[ExcelToJson] 변환 완료: {convertedCount}개 파일");
	}

	private static void ConvertExcel(string filePath, string tableName, Type tableType)
	{
		try
		{
			using (var package = new ExcelPackage(new FileInfo(filePath)))
			{
				var sheet = package.Workbook.Worksheets[1]; // 첫 번째 시트

				if (sheet == null || sheet.Dimension == null)
				{
					Debug.LogWarning($"[ExcelToJson] {tableName}: 시트가 비어있습니다.");
					return;
				}

				int rowCount = sheet.Dimension.Rows;
				int colCount = sheet.Dimension.Columns;

				// 첫 행: 변수명
				string[] headers = new string[colCount];
				for (int col = 1; col <= colCount; col++)
				{
					headers[col - 1] = sheet.Cells[1, col].Text.Trim();
				}

				// 변수명 검증
				ValidateHeaders(tableName, headers, tableType);

				// 데이터 행 파싱 <변수명, 변환된 데이터
				var dataList = new List<Dictionary<string, object>>();

				for (int row = 2; row <= rowCount; row++)
				{
					// 빈 행 무시 (uniqueId가 비어있으면 스킵)
					string firstCell = sheet.Cells[row, 1].Text.Trim();
					if (string.IsNullOrEmpty(firstCell))
						continue;

					var rowData = new Dictionary<string, object>();
					FieldInfo[] fields = tableType.GetFields(BindingFlags.Public | BindingFlags.Instance);

					for (int col = 1; col <= colCount; col++)
					{
						string header = headers[col - 1];
						string cellValue = sheet.Cells[row, col].Text.Trim();

						FieldInfo field = fields.FirstOrDefault(f => f.Name == header);

						if (field == null)
							continue;

						object value = ConvertValue(cellValue, field.FieldType, tableName, row, header);
						rowData[header] = value;
					}

					dataList.Add(rowData);
				}

				// JSON 저장
				string json = JsonConvert.SerializeObject(dataList, Formatting.Indented, new JsonSerializerSettings
				{
					Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() }
				});

				string jsonPath = Path.Combine(JsonFolderPath, $"{tableName}.json");
				File.WriteAllText(jsonPath, json);

				Debug.Log($"[ExcelToJson] {tableName}: {dataList.Count}행 변환 완료 → {jsonPath}");
			}
		}
		catch (Exception e)
		{
			Debug.LogError($"[ExcelToJson] {tableName} 변환 실패: {e.Message}");
		}
	}

	private static object ConvertValue(string cellValue, Type fieldType, string tableName, int row, string header)
	{
		try
		{
			if (fieldType == typeof(string))
			{
				return cellValue;
			}
			else if (fieldType == typeof(int))
			{
				if (string.IsNullOrEmpty(cellValue)) return 0;
				return int.Parse(cellValue);
			}
			else if (fieldType == typeof(float))
			{
				if (string.IsNullOrEmpty(cellValue)) return 0f;
				return float.Parse(cellValue);
			}
			else if (fieldType == typeof(bool))
			{
				if (string.IsNullOrEmpty(cellValue)) return false;
				return bool.Parse(cellValue);
			}
			else if (fieldType.IsEnum)
			{
				if (string.IsNullOrEmpty(cellValue))
					return Enum.ToObject(fieldType, 0);

				if (Enum.TryParse(fieldType, cellValue, true, out var enumValue))
					return enumValue;

				Debug.LogError($"[ExcelToJson] {tableName} {row}행 [{header}]: '{cellValue}'는 {fieldType.Name}에 없는 값입니다.");
				return Enum.ToObject(fieldType, 0);
			}

			Debug.LogWarning($"[ExcelToJson] {tableName} [{header}]: 지원하지 않는 타입 {fieldType.Name}");
			return cellValue;
		}
		catch (Exception e)
		{
			Debug.LogError($"[ExcelToJson] {tableName} {row}행 [{header}]: 변환 실패 ({cellValue}) - {e.Message}");
			return fieldType.IsValueType ? Activator.CreateInstance(fieldType) : null;
		}
	}

	private static void ValidateHeaders(string tableName, string[] headers, Type tableType)
	{
		FieldInfo[] fields = tableType.GetFields(BindingFlags.Public | BindingFlags.Instance);
		HashSet<string> fieldNames = new HashSet<string>(fields.Select(f => f.Name));
		HashSet<string> headerSet = new HashSet<string>(headers);

		// 엑셀에 있는데 클래스에 없는 칼럼
		foreach (string header in headers)
		{
			if (!string.IsNullOrEmpty(header) && !fieldNames.Contains(header))
			{
				Debug.LogWarning($"[ExcelToJson] {tableName}: 엑셀 칼럼 '{header}'가 {tableType.Name} 클래스에 없습니다.");
			}
		}

		// 클래스에 있는데 엑셀에 없는 필드
		foreach (string fieldName in fieldNames)
		{
			if (!headerSet.Contains(fieldName))
			{
				Debug.LogWarning($"[ExcelToJson] {tableName}: {tableType.Name}.{fieldName} 필드가 엑셀에 없습니다.");
			}
		}
	}
}
#endif