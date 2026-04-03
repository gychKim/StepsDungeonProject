# StepsDungeon

> 층마다 선택지를 통해 보상과 위험을 결정하며 보스를 향해 나아가는 하이퍼캐주얼 로그라이크 게임

<!-- 플레이 GIF -->

---

## 📋 개요

| 항목 | 내용 |
|------|------|
| 개발 기간 | 2026.01 ~ 진행중 |
| 개발 인원 | 1인 |
| 플랫폼 | Android |
| 엔진 | Unity 6000.2.7f2 |

---

## 🛠 기술 스택

`Unity` `C#` `VContainer` `R3` `UniTask` `Addressables` `Google Sheets` `ScriptableObject`

---

## 📐 아키텍처

**VContainer DI + UseCase 인터페이스 패턴**으로 각 시스템의 책임을 분리했습니다.  
모든 시스템은 인터페이스를 통해서만 참조되어 교체와 테스트가 용이합니다.

```
LifetimeScope (씬별 DI 컨테이너)
├── GameLifetimeScope        ← 전역 서비스 (Data, Save, Audio, Resource, GoogleSheet)
└── GameSceneLifetimeScope   ← 씬 단위 시스템 (Dungeon, Boss, Skill, Choice, Room ...)

각 System은 IXxxUseCase 인터페이스로 노출
  예) DungeonSystem → IDungeonUseCase
      SkillSystem   → ISkillUseCase
      BossSystem    → IBossUseCase
```

게임 흐름: `Title → Lobby → [EnterRoom → ChoiceSelect → ApplyReward → MoveNext] × N → BossIntro → BossFight → Result`

---

## ⚙️ 주요 구현

### 1. 스킬 시스템 (Strategy + Factory 패턴)

**발동 조건**과, **효과**를 인터페이스로 분리하여 조합 방식으로 스킬을 구성했습니다.  
새로운 스킬 추가 시 기존 코드를 수정하지 않고 조합만으로 확장할 수 있습니다.

```csharp
// Trigger 종류: Immediate / IntervalTimer / AttackCounter / RemainingTime / BattleEnd
// Effect 종류:  AddScore / PowerMultiplier / PresentationDamage / Composite

public class Skill
{
    public ISkillTrigger skillTrigger;
    public ISkillEffect  skillEffect;

    public void Apply(IBattleContext battleContext)
    {
        skillTrigger.Action(battleContext, skillTriggerValue,
            () => skillEffect.Execute(battleContext, skillValue));
    }
}

// 예: "10번 공격할 때마다 점수 추가" 스킬
// AttackCounterTrigger + AddScoreEffect 조합
```

스킬 랭크(C / B / A / S)에 따라 풀에서 랜덤 지급되며, VContainer로 의존성이 주입된 `SkillFactory`가 생성을 담당합니다.

---

### 2. GameState Machine

모든 시스템이 `IGameStateTransitionUseCase`를 구현하고 상태 변화를 `Observable<GameState>`로 구독합니다.  
상태 전환 시 관련 시스템만 반응하므로 시스템 간 직접 참조 없이 흐름을 제어합니다.

```csharp
// 상태 구독 예 (DungeonSystem)
gameStateUseCase.GameStateObv
    .Subscribe(gameState =>
    {
        switch (gameState)
        {
            case GameState.EnterRoom:  await SettingRoom(ct); break;
            case GameState.MoveNext:   MoveNext(); break;
        }
    }).AddTo(disposables);

// 상태 전환
gameStateUseCase.SetGameState(GameState.EnterBossRoom);
```

---

### 3. 선택지 시스템 (Choice)

각 방에서 2개의 선택지를 제시하고, 선택에 따라 확률 기반으로 보상 또는 리스크가 적용됩니다.  
선택 이력(ChoiceHistory)을 저장하여 다음 방의 난이도(RoomLevel)를 결정합니다.

```csharp
private RewardData CalculateReward(int index)
{
    float chance = index == 1
        ? currentChoiceData.ChoiceReward01Chance
        : currentChoiceData.ChoiceReward02Chance;

    bool isSuccess = Random.Gacha(chance); // 확률 판정

    // 성공 → Reward 효과 / 실패 → Risk 효과 적용
    effect.type  = isSuccess ? rewardType  : riskType;
    effect.value = isSuccess ? rewardValue : riskValue;
    ...
}
```

---

### 4. 방 생성 시스템

Addressables로 `RoomDecoDataSO`를 비동기 로드한 뒤, Tilemap에 타일을 배치하고 오브젝트·이동수단을 스폰합니다.  
**Poisson Disc Sampling**으로 Props를 자연스럽게 분산 배치합니다.

```csharp
private async UniTask RoomGenerate(NormalRoomData roomData, CancellationToken ct)
{
    roomController.ClearAll();
    RoomDecoDataSO deco = await resourceService.LoadAssetDataAsync<RoomDecoDataSO>(
        "RoomDeco_" + roomData.RoomTheme, ct);

    // 타일 배치
    for (int i = 0; i < deco.tileDataArr.Length; i++)
        roomController.SetGroundTile(new Vector3Int(i % deco.roomSize.x, i / deco.roomSize.x), deco.tileDataArr[i]);

    await roomController.SpawnProps(deco); // Poisson Disc Sampling 적용

    await UniTask.WhenAll(
        roomController.SpawnObjectAsync(roomData, deco.groundLevelY, ct),
        roomController.SpawnTransportObjectsAsync(roomData, deco.groundLevelY, ct));
}
```

---

### 5. 보스 시스템 (UniTaskCompletionSource 비동기 시퀀스)

보스 데이터 준비와 스폰 로직을 `UniTaskCompletionSource`로 연결하여, 데이터가 준비되기 전까지 스폰을 안전하게 대기합니다.

```csharp
// 데이터 준비 대기
private UniTaskCompletionSource<BossRoomData> bossRoomDataUCS;

private async UniTask BossSpawnAsync(CancellationToken ct)
{
    await bossRoomDataUCS.Task; // 던전 시스템이 SetResult 할 때까지 대기
    GameObject prefab = await resourceService.LoadAssetDataAsync<GameObject>($"Boss_{...}", ct);
    bossController = GameObject.Instantiate(prefab, ...).GetComponent<BossController>();
    bossSpawnedSub.OnNext(bossController);
}
```

---

### 6. Google Sheets 연동 데이터 관리

스킬·선택지·방·보스 등의 게임 데이터를 Google Sheets에서 관리합니다.  
`Newtonsoft.Json` + 커스텀 `JsonConverter`(Vector3Int, StringEnum)로 역직렬화합니다.

---

### 7. AES + HMAC 세이브 시스템

세이브 데이터 위변조 방지를 위해 AES-CBC 암호화와 HMAC 무결성 검증을 적용했습니다.  
던전 Seed, 스킬 목록, 선택 이력을 저장하여 게임 재개 시 동일한 상태를 복원합니다.

---

## 🔧 트러블슈팅

### 1. 레이스 컨디션 문제
비동기 작업을 하면서 발생하는 레이스 컨디션 문제를 UniTask.Lazy를 이용하여 해결.

---

## 💬 회고

