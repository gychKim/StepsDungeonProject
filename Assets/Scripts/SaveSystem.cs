using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor.Overlays;
using UnityEngine;
using VContainer.Unity;

public interface ISaveService
{
	UniTask Save();
	SaveData SaveData { get; set; }

	void Delete();
}

public class SaveData
{
	public int currentPower;
	public List<int> currentSkillIdList;

	public DungeonData dungeonData;

	// 게임 시드
	// 현재 힘
	// 현재 스킬 리스트
	// 현재 RoomCount (몇 번째 룸인지)


}

public class SaveSystem : IInitializable, ISaveService, IDisposable
{
	public SaveData SaveData { get => saveData; set => saveData = value; }

	private SaveData saveData; // 저장 데이터

	public void Initialize()
	{
		Load(); // 게임 데이터 로드
	}

	public async UniTask Save()
	{
		await JsonManager.SaveAsync(saveData, "saveData.sav", false);
		DebugX.GoldLog("저장 데이터 Save");
	}

	public async void Load()
	{
		var data = await JsonManager.LoadAsync<SaveData>("saveData.sav", false);

		if (data == null)
		{
			await InitSaveData();
			return;
		}

		saveData = data;
	}

	public async void Delete()
	{
		JsonManager.Delete("saveData.sav");
		await InitSaveData();
	}

	private async UniTask InitSaveData()
	{
		saveData = new SaveData()
		{
			
		};

		await Save();

		Load();
	}

	public void Dispose()
	{
		Save().Forget();
	}
}

public static class JsonManager
{
	private static string aesKeyText = "TheDungeonWakeUpAeSText159738246"; // 32바이트 
	private static string aesIVText = "aeSIvText2391Bro"; // 16바이트
	private static string hmacText = "9321TheDungeonWakTexthMac";

	/// <summary>
	/// 데이터를 fileName이름을 지닌 Json으로 저장
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="saveData"></param>
	/// <param name="fileName"></param>
	/// <param name="encryption"></param>
	public static void Save<T>(T saveData, string fileName, bool encryption = true) where T : class
	{
		string jsonText = JsonConvert.SerializeObject(saveData, Formatting.Indented); // saveData(클래스)를 직렬화해서 jsonText로 전환
		string path = Path.Combine(Application.persistentDataPath, fileName); // Application.persistentDataPath + fileName으로 저장위치 지정 (윈도우 같은 경우 AppData/LocalLow/...에 저장)

		// 암호화를 할 것이라면
		if (encryption)
		{
			byte[] aesKey = Encoding.UTF8.GetBytes(aesKeyText); // aesKey 생성
			byte[] aesIV = Encoding.UTF8.GetBytes(aesIVText); // aesIV 생성
			byte[] hmacKey = Encoding.UTF8.GetBytes(hmacText); // hmacKey 생성

			byte[] encrypted = AES.AESEncrypt(jsonText, aesKey, aesIV); // jsonText를 aes로 암호화

			byte[] hmac = HMAC.ComputeHMAC(encrypted, hmacKey); // 암호화된 파일을 hmac로 생성

			byte[] finalByteArr = new byte[encrypted.Length + hmac.Length]; // 암호화된 데이터 길이 + 위변조 탐지용인 hmac 길이를 붙여, 최종 Byte[]를 생성.
			Buffer.BlockCopy(encrypted, 0, finalByteArr, 0, encrypted.Length); // 암호화된 데이터를 finalByteArr의 처음(4번째 인자인 0)부터 암호화된 데이터 길이만큼 복사한다. 
			Buffer.BlockCopy(hmac, 0, finalByteArr, encrypted.Length, hmac.Length); // finalByteArr의 암호화된 데이터 길이 이후부터, hmac길이만큼 복사한다.

			string code = Convert.ToBase64String(finalByteArr); // Base64로 인코딩
			File.WriteAllText(path, code); // 저장
		}
		// 암호화 안하면
		else
		{
			File.WriteAllText(path, jsonText); // Json데이터를 Path에 저장
		}
	}

	/// <summary>
	/// T와 fileName에 맞는 데이터 Load후 리턴받음
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="fileName"></param>
	/// <param name="encryption"></param>
	/// <returns></returns>
	public static T Load<T>(string fileName, bool encryption = true) where T : class
	{
		string path = Path.Combine(Application.persistentDataPath, fileName); // Application.persistentDataPath + fileName의 경로를 가져온다.

		// 해당 path가 존재하는지 확인하고, 존재하지 않으면 에러 출력 후 null리턴
		if (!File.Exists(path))
		{
			DebugX.LogWarning($"{fileName}가 존재하지 않습니다.");
			return null;
		}

		// 해당 path를 Read하고 데이터를 가져온다.
		string jsonText = File.ReadAllText(path);

		// 만약 암호화가 true라면 복호화 해준다.
		if (encryption)
		{
			// 암호화 반대로 진행
			byte[] dataByteArr = Convert.FromBase64String(jsonText); // Base64를 복호화(디코딩)한다.

			byte[] aesKey = Encoding.UTF8.GetBytes(aesKeyText); // aesKey 생성
			byte[] aesIV = Encoding.UTF8.GetBytes(aesIVText); // aesIV 생성
			byte[] hmacKey = Encoding.UTF8.GetBytes(hmacText); // hmacKey 생성

			int hmacLen = 32; // HMAC-SHA256으로 암호화 했다 => HMAC-SHA256는 32바이트
			int encryptedLen = dataByteArr.Length - hmacLen; // Base64로 복호화한 데이터(encrypted + hmac)이므로, hmac길이만큼 제외하여 encrypted 길이를 구한다.

			byte[] encrypted = new byte[encryptedLen]; // 바이트 생성
			byte[] hmac = new byte[hmacLen]; // 바이트 생성

			Buffer.BlockCopy(dataByteArr, 0, encrypted, 0, encryptedLen); // 데이터 바이트의 0부터 encryptedLen만큼 데이터를 enctypted의 0부터 복사한다.
			Buffer.BlockCopy(dataByteArr, encryptedLen, hmac, 0, hmacLen); // 데이터 바이트의 encryptedLen부터 hmacLen만큼 데이터를 hmac의 0부터 복사한다.


			byte[] expectedHMAC = HMAC.ComputeHMAC(encrypted, hmacKey); // HMAC를 검증하기위해, 암호화에서 한 것 처럼 encrypted(암호화된 파일)를 hmacKey로 HMAC를 생성한다.
			if (!expectedHMAC.SequenceEqual(hmac)) // Save할 때의 데이터와 불일치 하다면 위변조가 발생한 것.
			{
				DebugX.Log("파일 위변조가 감지됨 : HMAC 불일치");
				return null;
			}

			string data = AES.AESDecrypt(encrypted, aesKey, aesIV);

			return JsonConvert.DeserializeObject<T>(data); // 디코딩된 최종 데이터를 역직렬화 해준 후 데이터를 리턴한다.
		}

		// 암호화 안되어 있으면 데이터를 역직렬화 해준 후 데이터를 리턴한다
		return JsonConvert.DeserializeObject<T>(jsonText);
	}

	/// <summary>
	/// 비동기 저장
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="data"></param>
	/// <param name="fileName"></param>
	/// <param name="encryption"></param>
	/// <returns></returns>
	public static async UniTask SaveAsync<T>(T data, string fileName, bool encryption = true) where T : class
	{
		string jsonText = JsonConvert.SerializeObject(data, Formatting.Indented); // saveData(클래스)를 직렬화해서 jsonText로 전환
		string path = Path.Combine(Application.persistentDataPath, fileName); // Application.persistentDataPath + fileName으로 저장위치 지정 (윈도우 같은 경우 AppData/LocalLow/...에 저장)

		// 암호화를 할 것이라면
		if (encryption)
		{
			byte[] aesKey = Encoding.UTF8.GetBytes(aesKeyText); // aesKey 생성
			byte[] aesIV = Encoding.UTF8.GetBytes(aesIVText); // aesIV 생성
			byte[] hmacKey = Encoding.UTF8.GetBytes(hmacText); // hmacKey 생성

			byte[] encrypted = AES.AESEncrypt(jsonText, aesKey, aesIV); // jsonText를 aes로 암호화

			byte[] hmac = HMAC.ComputeHMAC(encrypted, hmacKey); // 암호화된 파일을 hmac로 생성

			byte[] finalByteArr = new byte[encrypted.Length + hmac.Length]; // 암호화된 데이터 길이 + 위변조 탐지용인 hmac 길이를 붙여, 최종 Byte[]를 생성.
			Buffer.BlockCopy(encrypted, 0, finalByteArr, 0, encrypted.Length); // 암호화된 데이터를 finalByteArr의 처음(4번째 인자인 0)부터 암호화된 데이터 길이만큼 복사한다. 
			Buffer.BlockCopy(hmac, 0, finalByteArr, encrypted.Length, hmac.Length); // finalByteArr의 암호화된 데이터 길이 이후부터, hmac길이만큼 복사한다.

			string code = Convert.ToBase64String(finalByteArr); // Base64로 인코딩
			await File.WriteAllTextAsync(path, code); // Base64로 인코딩된 데이터를 비동기로 Path에 저장
		}
		// 암호화 안하면
		else
		{
			await File.WriteAllTextAsync(path, jsonText); // Json데이터를 비동기로 Path에 저장
		}
	}

	public static async UniTask<T> LoadAsync<T>(string fileName, bool encryption = true) where T : class, new()
	{
		string path = Path.Combine(Application.persistentDataPath, fileName); // Application.persistentDataPath + fileName의 경로를 가져온다.

		// 해당 path가 존재하는지 확인하고, 존재하지 않으면 에러 출력 후 null리턴
		if (!File.Exists(path))
		{
			DebugX.LogWarning($"{fileName}가 존재하지 않습니다.");
			return null;
		}

		// 해당 path를 비동기로 Read하고 끝났으면 데이터를 가져온다.
		string jsonText = await File.ReadAllTextAsync(path);

		// 만약 암호화가 true라면 복호화 해준다.
		if (encryption)
		{
			// 암호화 반대로 진행
			byte[] dataByteArr = Convert.FromBase64String(jsonText); // Base64를 복호화(디코딩)한다.

			byte[] aesKey = Encoding.UTF8.GetBytes(aesKeyText); // aesKey 생성
			byte[] aesIV = Encoding.UTF8.GetBytes(aesIVText); // aesIV 생성
			byte[] hmacKey = Encoding.UTF8.GetBytes(hmacText); // hmacKey 생성

			int hmacLen = 32; // HMAC-SHA256으로 암호화 했다 => HMAC-SHA256는 32바이트
			int encryptedLen = dataByteArr.Length - hmacLen; // Base64로 복호화한 데이터(encrypted + hmac)이므로, hmac길이만큼 제외하여 encrypted 길이를 구한다.

			byte[] encrypted = new byte[encryptedLen]; // 바이트 생성
			byte[] hmac = new byte[hmacLen]; // 바이트 생성

			Buffer.BlockCopy(dataByteArr, 0, encrypted, 0, encryptedLen); // 데이터 바이트의 0부터 encryptedLen만큼 데이터를 enctypted의 0부터 복사한다.
			Buffer.BlockCopy(dataByteArr, encryptedLen, hmac, 0, hmacLen); // 데이터 바이트의 encryptedLen부터 hmacLen만큼 데이터를 hmac의 0부터 복사한다.


			byte[] expectedHMAC = HMAC.ComputeHMAC(encrypted, hmacKey); // HMAC를 검증하기위해, 암호화에서 한 것 처럼 encrypted(암호화된 파일)를 hmacKey로 HMAC를 생성한다.
			if (!expectedHMAC.SequenceEqual(hmac)) // Save할 때의 데이터와 불일치 하다면 위변조가 발생한 것.
			{
				DebugX.Log("파일 위변조가 감지됨 : HMAC 불일치");
				return null;
			}

			string data = AES.AESDecrypt(encrypted, aesKey, aesIV);

			return JsonConvert.DeserializeObject<T>(data); // 디코딩된 최종 데이터를 역직렬화 해준 후 데이터를 리턴한다.
		}

		// 암호화 안되어 있으면 데이터를 역직렬화 해준 후 데이터를 리턴한다
		return JsonConvert.DeserializeObject<T>(jsonText);
	}

	/// <summary>
	/// 세이브 파일 삭제
	/// </summary>
	/// <param name="fileName"></param>
	public static void Delete(string fileName)
	{
		// 1. 저장할 때와 동일한 경로 결합
		string path = Path.Combine(Application.persistentDataPath, fileName);
		// 2. 파일이 존재하는지 확인 (없는데 지우려고 하면 에러가 날 수 있음)
		if (File.Exists(path))
		{
			File.Delete(path); // 3. 파일 삭제
			Debug.Log($"[System] 파일이 삭제되었습니다: {path}");
		}
		else
		{
			Debug.LogWarning($"[System] 삭제할 파일이 존재하지 않습니다: {path}");
		}
	}
}
