
public static class Mathf
{
    public static int Max(int v1, int v2)
	{
		return UnityEngine.Mathf.Max(v1, v2);
	}

	public static int Max(int[] values)
	{
		return UnityEngine.Mathf.Max(values);
	}

	public static float Max(float v1, float v2)
	{
		return UnityEngine.Mathf.Max(v1, v2);
	}

	public static float Max(float[] values)
	{
		return UnityEngine.Mathf.Max(values);
	}

	public static int Min(int v1, int v2)
	{
		return UnityEngine.Mathf.Min(v1, v2);
	}

	public static int Min(int[] values)
	{
		return UnityEngine.Mathf.Min(values);
	}

	public static float Min(float v1, float v2)
	{
		return UnityEngine.Mathf.Min(v1, v2);
	}

	public static float Min(float[] values)
	{
		return UnityEngine.Mathf.Min(values);
	}

	public static int Abs(int value)
	{
		return UnityEngine.Mathf.Abs(value);
	}

	public static float Abs(float value)
	{
		return UnityEngine.Mathf.Abs(value);
	}

	public static float Round(float v1)
	{
		return UnityEngine.Mathf.Round(v1);
	}

	/// <summary>
	/// 반올림(0.5라면 항상 1로)
	/// </summary>
	/// <param name="v1"></param>
	/// <returns></returns>
	public static int RoundToInt(float v1)
	{
		return (int)System.Math.Round(v1, System.MidpointRounding.AwayFromZero);
	}

	public static int Clamp(int value, int min, int max)
	{
		return UnityEngine.Mathf.Clamp(value, min, max);
	}

	public static float Clamp(float value, float min, float max)
	{
		return UnityEngine.Mathf.Clamp(value, min, max);
	}

	public static float Clamp01(float value)
	{
		return UnityEngine.Mathf.Clamp01(value);
	}

	public static float Lerp(float a, float b, float t)
	{
		return UnityEngine.Mathf.Lerp(a, b, t);
	}

	public static float Ceil(float value)
	{
		return UnityEngine.Mathf.Ceil(value);
	}

	public static int CeilToInt(float value)
	{
		return UnityEngine.Mathf.CeilToInt(value);
	}

	public static float PI => UnityEngine.Mathf.PI;
	public static float Sin(float f) => UnityEngine.Mathf.Sin(f);
	public static float Cos(float f) => UnityEngine.Mathf.Cos(f);
    public static int FloorToInt(float value) => UnityEngine.Mathf.FloorToInt(value);
    public static float Sqrt(float value) => UnityEngine.Mathf.Sqrt(value);
    public static float Pow(float f, float p) => UnityEngine.Mathf.Pow(f, p);
    public static float InverseLerp(float a, float b, float value) => UnityEngine.Mathf.InverseLerp(a, b, value);
}
