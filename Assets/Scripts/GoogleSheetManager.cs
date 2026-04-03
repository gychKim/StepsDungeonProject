using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using VContainer.Unity;

public interface IGoogleSheetService
{
	UniTask<List<T>> LoadSheet<T>(string sheetName);
}

public class GoogleSheetManager : IGoogleSheetService
{
	// 배포한 웹 앱 URL
	const string BASE_URL = "https://script.google.com/macros/s/AKfycbz-morHMPK69QBxdGMbiyV0tQMZdhJajyhWLXYt-eR27Nyb5_ao-DbCUbEKle3viTEagw/exec";

	public async UniTask<List<T>> LoadSheet<T>(string sheetName)
	{
		// URL 뒤에 파라미터 붙이기 (?sheetName=이름)
		string url = $"{BASE_URL}?sheetName={sheetName}";

		DebugX.Log($"데이터 요청 중: {sheetName}...");

		using (UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(url))
		{
			await www.SendWebRequest();

			if (www.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
			{
				DebugX.LogError($"에러 발생 ({sheetName}): {www.error}");
			}
			else
			{
				string jsonResult = www.downloadHandler.text;
				DebugX.Log($"[{sheetName}] 수신 완료: {jsonResult}");

				// 여기서 시트 이름에 따라 다른 파싱 함수를 호출하거나 처리
				return ProcessData<T>(sheetName, jsonResult);
			}
		}

		return default;
	}

	private List<T> ProcessData<T>(string sheetName, string json)
	{
		try
		{
			// Json 배열 문자열을 List<T>로 바로 변환
			List<T> roomList = JsonConvert.DeserializeObject<List<T>>(json);
			DebugX.Log($"{sheetName} 파싱 성공! 데이터 개수: {roomList.Count}");

			return roomList;
		}
		catch (System.Exception e)
		{
			DebugX.LogError($"{sheetName} 파싱 실패: {e.Message}");
			return default;
		}
	}

}
