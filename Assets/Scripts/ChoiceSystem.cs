using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using R3;
using System;
using System.Threading;
using UnityEngine;
using VContainer.Unity;



public class ChoiceHistoryData
{
	//public ChoiceData choiceData; // 선택한 선택지 데이터

	public int selectedIndex; // 몇 번을 선택했는지(1번 or 2번)

	public RoomLevel nextRoomLevel; // 다음 룸 난이도
}

public class ChoiceData
{
	public int Index; // 인덱스

	[JsonConverter(typeof(StringEnumConverter))]
	public RoomLevel RoomLevel; // 룸 난이도(키값)
	[JsonConverter(typeof(StringEnumConverter))]
	public ObjectType RoomObjectType; // 룸 오브젝트 타입(키값)

	public string ChoiceTitle; // 선택지 제목
	public string ChoiceContent; // 선택지 내용

	public string Choice01ButtonText; // 선택지 01 버튼 텍스트
	[JsonConverter(typeof(StringEnumConverter))]
	public EffectType ChoiceReward01Type; // 선택지01 보상 타입
	public int ChoiceReward01Value; // 선택지 01 보상 값
	public int ChoiceReward01Target; // 선택지 01 보상 타겟 (스킬이라면 스킬 인덱스, 파워라면 0)
	public float ChoiceReward01Chance; // 선택지 01 보상 확률

	[JsonConverter(typeof(StringEnumConverter))]
	public EffectType ChoiceRisk01Type; // 선택지01 리스크 타입
	public int ChoiceRisk01Value; // 선택지01 리스크 값
	public int ChoiceRisk01Target; 

	[JsonConverter(typeof(StringEnumConverter))]
	public RoomLevel Choice01Level; // 선택지01 룸 난이도

	public string Choice02ButtonText; // 선택지 02 버튼 텍스트

	[JsonConverter(typeof(StringEnumConverter))]
	public EffectType ChoiceReward02Type; // 선택지02 보상 타입
	public int ChoiceReward02Value; // 선택지 02 보상 값
	public int ChoiceReward02Target; // 선택지 02 보상 타겟 (스킬이라면 스킬 인덱스, 파워라면 0)
	public float ChoiceReward02Chance; // 선택지 02 보상 확률

	[JsonConverter(typeof(StringEnumConverter))]
	public EffectType ChoiceRisk02Type; // 선택지02 리스크 타입
	public int ChoiceRisk02Value; // 선택지02 리스크 값
	public int ChoiceRisk02Target;

	[JsonConverter(typeof(StringEnumConverter))]
	public RoomLevel Choice02Level; // 선택지02 룸 난이도
}

public interface IChoiceUseCase
{
	void SetChoiceData(NormalRoomData roomData);
}
public class ChoiceSystem : IChoiceUseCase, IGameStateTransitionUseCase, IInitializable, IDisposable
{
	private readonly ISaveService saveService;
	private readonly IDataService dataService;
	private readonly IGameStateUseCase gameStateUseCase;
	private readonly IDungeonUseCase dungeonUseCase;
	private readonly IUIUseCase uiUseCase;
	private readonly IRewardUseCase rewardUseCase;

	private readonly CompositeDisposable disposables;

	private ChoiceData currentChoiceData = null;
	private RewardData currentRewardData = null;

	private UniTaskCompletionSource choiceUCS;
	public ChoiceSystem(ISaveService saveService, IGameStateUseCase gameStateUseCase, IDungeonUseCase dungeonUseCase, IDataService dataService, IUIUseCase uiUseCase, IRewardUseCase rewardUseCase)
	{
		disposables = new CompositeDisposable();

		this.saveService = saveService;
		this.gameStateUseCase = gameStateUseCase;
		gameStateUseCase.Init(this);
		this.dungeonUseCase = dungeonUseCase;
		this.dataService = dataService;
		this.uiUseCase = uiUseCase;
		this.rewardUseCase = rewardUseCase;
	}

	public void Initialize()
	{
		gameStateUseCase.GameStateObv
			.Where(state => state == GameState.ChoiceSelect)
			.Subscribe(state =>
			{
				ChoiceSelect().Forget();
			}).AddTo(disposables);

		dungeonUseCase.RoomDataInitObv
			.Subscribe(roomData =>
			{
				SetChoiceData(roomData);
			}).AddTo(disposables);
	}

	public async UniTask OnStateEnterAsync(GameState gameState, CancellationToken ct = default)
	{
		switch(gameState)
		{
			case GameState.ApplyReward:
				// 결과 UI 전달 데이터 세팅
				UIChoiceResultDTO choiceResultDTO = new UIChoiceResultDTO()
				{
					content = "결과"
				};

				// 결과 UI Open 및 종료대기
				await uiUseCase.OpenPopupAsync(UIKeys.ChoiceResult, choiceResultDTO, ct);

				// 보상 적용
				await rewardUseCase.ApplyReward(currentRewardData, ct);

				// 게임 상태 변경
				gameStateUseCase.SetGameState(GameState.MoveNext);
				break;
		}
	}

	public void SetChoiceData(NormalRoomData roomData)
	{
		currentChoiceData = dataService.GetChoiceData(roomData.RoomLevel, roomData.ObjectType);
	}

	public async UniTask ChoiceSelect()
	{
		// UI 활성화
		CancellationTokenSource cts = new CancellationTokenSource();
		choiceUCS = new UniTaskCompletionSource();

		UIChoiceContext choiceContext = new UIChoiceContext()
		{
			choiceTitle = currentChoiceData.ChoiceTitle,
			choiceContent = currentChoiceData.ChoiceContent,
			
			choice01ButtonText = currentChoiceData.Choice01ButtonText,
			choice01ChanceText = (currentChoiceData.ChoiceReward01Chance * 100).ToString("F1"), // 예: 0.75 -> "75.0"
			choice01NextLevelText = currentChoiceData.Choice01Level.ToString(),

			choice02ButtonText = currentChoiceData.Choice02ButtonText,
			choice02ChanceText = (currentChoiceData.ChoiceReward02Chance * 100).ToString("F1"), // 예: 0.75 -> "75.0"
			choice02NextLevelText = currentChoiceData.Choice02Level.ToString(),
		};

		int selectedIndex = await uiUseCase.OpenPopupAsync(UIKeys.Choice, choiceContext, cts.Token);

		// 이후 처리
		// 선택한 히스토리를 저장
		RoomLevel nextRoomLevel = selectedIndex == 1 ? currentChoiceData.Choice01Level : currentChoiceData.Choice02Level;
		ChoiceHistoryData historyData = new ChoiceHistoryData()
		{
			selectedIndex = selectedIndex,
			nextRoomLevel = nextRoomLevel,
		};
		dungeonUseCase.AddChoiceHistory(historyData); // 현재 선택지 기록 저장

		// 보상 데이터 로직
		currentRewardData = CalculateReward(selectedIndex);

		gameStateUseCase.SetGameState(GameState.ApplyReward);
	}

	/// <summary>
	/// 선택한 인덱스에 따라 확률을 계산하고 최종 보상 데이터를 생성합니다.
	/// </summary>
	private RewardData CalculateReward(int index)
	{
		// 선택지에 따른 데이터 매핑 (1번 선택지 vs 2번 선택지)
		// 변수를 미리 할당하여 로직 중복 제거
		float chance;
		EffectType rewardType, riskType;
		int rewardValue, riskValue;
		int rewardTarget, riskTarget; // Target 추가
		if (index == 1)
		{
			chance = currentChoiceData.ChoiceReward01Chance;

			rewardType = currentChoiceData.ChoiceReward01Type;
			rewardValue = currentChoiceData.ChoiceReward01Value;
			rewardTarget = currentChoiceData.ChoiceReward01Target; // 추가된 필드
			riskType = currentChoiceData.ChoiceRisk01Type;
			riskValue = currentChoiceData.ChoiceRisk01Value;
			riskTarget = currentChoiceData.ChoiceRisk01Target; // 추가된 필드
		}
		else
		{
			chance = currentChoiceData.ChoiceReward02Chance;
			rewardType = currentChoiceData.ChoiceReward02Type;
			rewardValue = currentChoiceData.ChoiceReward02Value;
			rewardTarget = currentChoiceData.ChoiceReward02Target;
			riskType = currentChoiceData.ChoiceRisk02Type;
			riskValue = currentChoiceData.ChoiceRisk02Value;
			riskTarget = currentChoiceData.ChoiceRisk02Target;
		}

		// 확률 계산 로직 (0.0 ~ 1.0)
		// UnityEngine.Random.value는 0.0 ~ 1.0 사이의 랜덤 float 반환
		// 예: chance가 0.7(70%)이면, Random.value가 0.7보다 작거나 같을 때 성공
		bool isSuccess = Random.Gacha(chance);
		RewardData data = new RewardData();
		data.isSuccess = isSuccess;
		EffectData effect = new EffectData();

		if (isSuccess)
		{
			// 성공 (Reward 적용)
			effect.type = rewardType;
			effect.value = rewardValue;
			effect.target = rewardTarget;

			// 결과 텍스트 예시 (실제 게임에서는 로컬라이징 키 등을 사용 추천)
			data.resultText = $"성공! {rewardType} 효과를 얻었습니다. (+{rewardValue})";
		}
		else
		{
			// 실패 (Risk 적용)
			effect.type = riskType;
			effect.value = riskValue; // DB에 -10으로 적혀있다면 그대로 적용
			effect.target = riskTarget;
			data.resultText = $"실패... {riskType} 효과를 받았습니다. ({riskValue})";
		}

		data.effectList.Add(effect);

		return data;
	}

	public void Dispose()
	{
		disposables.Dispose();
	}
}
