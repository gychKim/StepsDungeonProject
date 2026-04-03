using System.Linq;

public class Random
{
    /// <summary>
	/// 시드 변경
	/// </summary>
	/// <param name="seed"></param>
    public static void SetSeed(int seed) => UnityEngine.Random.InitState(seed);

    /// <summary>
	/// 셔플
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
    public static void Shuffle<T>(System.Collections.Generic.IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = RangeNew(0, i);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

	/// <summary>
	/// min 부터 max이전 수 중 랜덤한 수를 리턴한다.
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static int RangeOriginal(int min, int max)
	{
		return UnityEngine.Random.Range(min, max);
	}

	/// <summary>
	/// min 부터 max까지 수 중 랜덤한 수를 리턴한다.
	/// </summary>
	/// <param name="min"></param>
	/// <param name="max"></param>
	/// <returns></returns>
	public static int RangeNew(int min, int max)
    {
        return UnityEngine.Random.Range(min, max + 1);
    }

    /// <summary>
    /// min 부터 max까지 수 중 랜덤한 수를 리턴한다.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public static float Range(float min, float max)
    {
        return UnityEngine.Random.Range(min, max);
    }

    /// <summary>
    /// True혹은 False를 리턴한다.
    /// </summary>
    /// <returns></returns>
    public static bool Bool()
    {
        return RangeNew(0,1) == 1;
    }

    /// <summary>
    /// 받은 확률에 따라 true 혹은 false를 리턴한다.
    /// </summary>
    /// <param name="percentage"></param>
    /// <returns></returns>
    public static bool Gacha(float percentage)
    {
        if (percentage <= 0f)
            return false;

        if(percentage >= 1f)
            return true;

		return Range(0f, 1f) < percentage;
    }

	/// <summary>
	/// 확률 리스트를 전부 시도하여, true가 된 순서값(Index)이 리턴된다.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="list"></param>
	/// <param name="weightList"></param>
	/// <returns></returns>
	public static int Gacha(System.Collections.Generic.IReadOnlyList<float> weightList)
	{
		if (weightList == null || weightList.Count <= 0)
			return -1;

		if(weightList.Sum() > 1.0f)
		{
			DebugX.RedLog("확률의 합이 1보다 클 순 없습니다.");
			return -1;
		}

		for(int i = 0; i < weightList.Count; i++)
		{
			if(Gacha(weightList[i]))
				return i;
		}

		return weightList.Count - 1;
	}

	/// <summary>
	/// 배열/리스트 내부의 랜덤한 값을 리턴한다.
	/// </summary>
	/// <param name="percentage"></param>
	/// <returns></returns>
	public static T Gacha<T>(System.Collections.Generic.IReadOnlyList<T> list)
	{
		if(list == null || list.Count <= 0)
			return default(T);

		return list[RangeNew(0, list.Count - 1)];
	}

	/// <summary>
	/// 사전의 Key값들을 가져와 사전의 랜덤한 값을 리턴한다.
	/// </summary>
	/// <typeparam name="TKey"></typeparam>
	/// <typeparam name="TValue"></typeparam>
	/// <param name="dict"></param>
	/// <param name="keyArr"></param>
	/// <returns></returns>
	public static TValue Gacha<TKey, TValue>(System.Collections.Generic.IReadOnlyDictionary<TKey, TValue> dict)
	{
		if (dict == null || dict.Count <= 0)
			return default(TValue);

        int targetIndex = UnityEngine.Random.Range(0, dict.Count); // max는 제외!
        int currentIndex = 0;

        foreach (var pair in dict)
        {
            if (currentIndex == targetIndex)
                return pair.Value;

            currentIndex++;
        }

        return default(TValue);
	}

	/// <summary>
	/// 0 ~ 1.0 사이의 랜덤한 값을 가져온다.
	/// </summary>
	public static float Value { get => UnityEngine.Random.value; }
}
