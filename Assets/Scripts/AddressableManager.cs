using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 이미지 적용을 위해 필요
					  // ★ 필수 네임스페이스
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableManager : MonoBehaviour
{
	// 결과를 보여줄 UI 이미지 (Inspector에서 연결)
	public Image displayImage;

	// 로드된 리소스의 핸들(메모리 해제용)을 저장할 변수들
	private AsyncOperationHandle<Sprite> spriteHandle;
	private AsyncOperationHandle<GameObject> prefabHandle;

	void Start()
	{
		// 테스트 실행
		LoadImageA();
	}

	// ---------------------------------------------------------
	// 1. 이미지(Sprite) 로드 예시
	// ---------------------------------------------------------
	public void LoadImageA()
	{
		// 아까 설정한 주소 이름 "MyImage_A"
		Addressables.LoadAssetAsync<Sprite>("TitleImage").Completed += (op) =>
		{
			// 로딩 성공 여부 확인
			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				Debug.Log("이미지 A 로드 성공!");

				// 로드된 스프라이트를 UI에 적용
				displayImage.sprite = op.Result;

				// 나중에 메모리 해제를 위해 핸들 저장
				spriteHandle = op;
			}
			else
			{
				Debug.LogError("이미지 A 로드 실패");
			}
		};
	}

	// ---------------------------------------------------------
	// 2. 프리팹(Prefab) 로드 및 생성 예시
	// ---------------------------------------------------------
	public void LoadPrefabB()
	{
		// 프리팹은 LoadAssetAsync 대신 InstantiateAsync를 쓰면 로드+생성을 한 번에 해줍니다.
		// 주소 이름: "MyPrefab_B"
		Addressables.InstantiateAsync("MyPrefab_B").Completed += (op) =>
		{
			if (op.Status == AsyncOperationStatus.Succeeded)
			{
				Debug.Log("프리팹 B 생성 성공!");

				// 생성된 게임 오브젝트 가져오기 (필요하다면 위치 조절 등 가능)
				GameObject createdObj = op.Result;
				createdObj.transform.position = Vector3.zero;

				// InstantiateAsync의 핸들은 생성된 객체 관리를 포함합니다.
				prefabHandle = op;
			}
			else
			{
				Debug.LogError("프리팹 B 생성 실패");
			}
		};
	}

	// ---------------------------------------------------------
	// 3. 메모리 해제 (매우 중요)
	// ---------------------------------------------------------
	public void UnloadAssets()
	{
		// 이미지 해제
		if (spriteHandle.IsValid())
		{
			// UI에서 먼저 뺍니다.
			displayImage.sprite = null;
			// 메모리에서 해제
			Addressables.Release(spriteHandle);
			Debug.Log("이미지 A 메모리 해제 완료");
		}

		// 프리팹 인스턴스 해제 (게임 오브젝트 파괴 + 메모리 해제)
		if (prefabHandle.IsValid())
		{
			// InstantiateAsync로 만든 건 ReleaseInstance로 지워야 합니다.
			// (Destroy(obj)를 쓰면 메모리 릭이 발생할 수 있습니다)
			Addressables.ReleaseInstance(prefabHandle);
			Debug.Log("프리팹 B 삭제 및 메모리 해제 완료");
		}
	}

	// 오브젝트가 파괴될 때 자동으로 정리
	private void OnDestroy()
	{
		UnloadAssets();
	}
}
