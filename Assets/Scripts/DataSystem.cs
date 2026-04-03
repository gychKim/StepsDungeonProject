using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public interface IDataService
{
	NormalRoomData GetRoomData(RoomLevel roomLevel);
	BossRoomData GetBossRoomData(DungeonType dungeonType);
	ChoiceData GetChoiceData(RoomLevel roomLevel, ObjectType objectType);
	SkillData GetSkillData(int index);
	SkillData GetSkillData(Rank rank);
}
public class DataSystem : IDataService, IGameLoad
{
	private readonly IGoogleSheetService googleSheetService;

	private readonly Dictionary<int, NormalRoomData> roomOriginDataDict = new Dictionary<int, NormalRoomData>();
	private Dictionary<RoomLevel, List<NormalRoomData>> roomDataDict = new ();

	private readonly Dictionary<int, BossRoomData> bossRoomOriginDataDict = new Dictionary<int, BossRoomData>();
	private Dictionary<DungeonType, List<BossRoomData>> bossRoomDataDict = new();

	private readonly Dictionary<int, SkillData> skillOriginDataDict = new Dictionary<int, SkillData>();

	private readonly Dictionary<int, ChoiceData> choiceOriginDataDict = new Dictionary<int, ChoiceData>();
	private Dictionary<(RoomLevel roomLevel, ObjectType objectType), List<ChoiceData>> choiceDataDict = new();

	public bool IsLoaded => isLoaded;
	private bool isLoaded = false;

	public event Action<float> OnProgressChanged;

	public DataSystem(IGoogleSheetService googleSheetService)
	{
		this.googleSheetService = googleSheetService;
	}

	public async UniTask StartAsync(CancellationToken cancellation = default)
	{
		if(isLoaded)
		{
			DebugX.YellowLog("이미 데이터가 로드되어 있습니다.");
			return;
		}

		OnProgressChanged.Invoke(0f);

		var roomDataList = await googleSheetService.LoadSheet<NormalRoomData>("RoomTable");
		foreach(NormalRoomData roomData in roomDataList)
		{
			roomOriginDataDict.Add(roomData.Index, roomData);

			var key = roomData.RoomLevel;
			if(!roomDataDict.ContainsKey(key))
			{
				roomDataDict.Add(key, new List<NormalRoomData>());
			}

			roomDataDict[key].Add(roomData);
		}

		var bossRoomDataList = await googleSheetService.LoadSheet<BossRoomData>("BossTable");
		foreach (BossRoomData bossRoomData in bossRoomDataList)
		{
			bossRoomOriginDataDict.Add(bossRoomData.Index, bossRoomData);

			var key = bossRoomData.DungeonType;
			if (!bossRoomDataDict.ContainsKey(key))
			{
				bossRoomDataDict.Add(key, new List<BossRoomData>());
			}

			bossRoomDataDict[key].Add(bossRoomData);
		}

		OnProgressChanged.Invoke(0.33f);

		var skillDataList = await googleSheetService.LoadSheet<SkillData>("SkillTable");
		foreach (SkillData skillData in skillDataList)
		{
			skillOriginDataDict.Add(skillData.Index, skillData);
		}

		OnProgressChanged.Invoke(0.66f);

		var choiceDataList = await googleSheetService.LoadSheet<ChoiceData>("ChoiceTable");
		foreach (ChoiceData choiceData in choiceDataList)
		{
			choiceOriginDataDict.Add(choiceData.Index, choiceData);

			var key = (choiceData.RoomLevel, choiceData.RoomObjectType);

			if (!choiceDataDict.ContainsKey(key))
			{
				choiceDataDict.Add(key, new List<ChoiceData>());
			}

			choiceDataDict[key].Add(choiceData);
		}

		OnProgressChanged.Invoke(1.0f);

		isLoaded = true;
	}

	public SkillData GetSkillData(int index)
	{
		SkillData skillData = null;
		if (!skillOriginDataDict.TryGetValue(index, out skillData))
		{
			DebugX.RedLog($"{index}에 해당하는 스킬 데이터 없음");
		}

		return skillData;
	}

	public SkillData GetSkillData(Rank rank)
	{
		return Random.Gacha(skillOriginDataDict.Where(skill => skill.Value.SkillRank == rank).ToList()).Value;
	}

	public NormalRoomData GetRoomData(RoomLevel roomLevel)
	{
		return Random.Gacha(roomDataDict[roomLevel]);
	}

	public BossRoomData GetBossRoomData(DungeonType dungeonType)
	{
		return Random.Gacha(bossRoomDataDict[dungeonType]);
	}

	public ChoiceData GetChoiceData(RoomLevel roomLevel, ObjectType objectType)
	{
		return Random.Gacha(choiceDataDict[(roomLevel, objectType)]);
	}
}
