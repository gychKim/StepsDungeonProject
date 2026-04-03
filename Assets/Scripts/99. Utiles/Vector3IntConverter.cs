using Newtonsoft.Json;
using System;
using UnityEngine;

public class Vector3IntConverter : JsonConverter<Vector3Int>
{
	public override Vector3Int ReadJson(JsonReader reader, Type objectType, Vector3Int existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		// 1. JSON 값을 문자열로 가져옴
		string s = (string)reader.Value;
		if (string.IsNullOrEmpty(s))
			return Vector3Int.zero;
		try
		{
			// 2. 대괄호([])와 공백 제거
			s = s.Replace("[", "").Replace("]", "").Trim();

			// 3. 쉼표(,)로 분리
			string[] parts = s.Split(',');
			// 4. xyz 파싱 후 리턴
			int x = int.Parse(parts[0].Trim());
			int y = int.Parse(parts[1].Trim());
			int z = int.Parse(parts[2].Trim());
			return new Vector3Int(x, y, z);
		}
		catch
		{
			Debug.LogError($"Vector3Int 파싱 실패: {s} (포맷을 확인하세요)");
			return Vector3Int.zero;
		}
	}
	public override void WriteJson(JsonWriter writer, Vector3Int value, JsonSerializer serializer)
	{
		// (필요 시) 반대로 저장할 때: "[x, y, z]" 형태로 쓰기
		writer.WriteValue($"[{value.x}, {value.y}, {value.z}]");
	}
}
