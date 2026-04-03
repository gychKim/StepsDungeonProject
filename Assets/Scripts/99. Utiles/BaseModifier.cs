using System;

/// <summary>
/// 연산 순서
/// </summary>
public enum OperationLayer
{
    Add = 10, // 합연산 
    Mul = 20, // 곱연산
    ClampMin = 30,
    ClampMax = 40,
    Override = 50,
}

public abstract class BaseModifier
{
	public abstract void Handle<T>(object sender, T query) where T : Query;
}

public abstract class BaseModifier<TQuery> : BaseModifier
{
	public OperationLayer OperLayer { get; protected set; } // 연산 순서
	public float Value { get; protected set; } // 값
	public int Layer { get; protected set; } = 10; // 10 : Add, 20 : Mul, 30 : Clamp ...
	public bool MarkedForRemoval { get; protected set; } // 제거 조건이 만족하였는지

	public float TotalTime { get; protected set; } // 총 시간 > -1이면 영구
	public float RemainingValue { get; protected set; } // 남은 주간(턴) > -1이면 영구

	protected Func<float, float> operation; // 계산 로직

	public event Action<BaseModifier> OnDispose = delegate { }; // 해제

	public BaseModifier(OperationLayer operLayer, float value, int layer, float remainingValue)
	{
		OperLayer = operLayer;
		Value = value;
		Layer = layer;
		TotalTime = remainingValue;
		RemainingValue = remainingValue;
	}

	/// <summary>
	/// Modifier가 수행하는 작업
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="sender"></param>
	/// <param name="query"></param>
	public override void Handle<T>(object sender, T query)
	{
		if (query is TQuery qry)
			HandleLogic(sender, qry);
	}

	/// <summary>
	/// Modifier가 수행하는 실제 작업로직
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="query"></param>
	protected abstract void HandleLogic(object sender, TQuery query);

	/// <summary>
	/// 수행할 작업 결정
	/// </summary>
	protected void Build()
	{
		switch (OperLayer)
		{
			case OperationLayer.Add:
				operation = (x) => x + Value;
				break;
			case OperationLayer.Mul:
				operation = (x) => x * (1f + Value); // Value=0.2 => +20%
				break;
			case OperationLayer.ClampMin:
				operation = (x) => Mathf.Max(x, Value);
				break;
			case OperationLayer.ClampMax:
				operation = (x) => Mathf.Min(x, Value);
				break;
			case OperationLayer.Override:
				operation = (x) => Value;
				break;
			default:
				operation = (x) => x;
				break;
		}
	}

	/// <summary>
	/// 중재자 갱신
	/// </summary>
	public void Update()
	{
		if (RemainingValue < 0)
			return;

		RemainingValue -= 1;
		if (RemainingValue <= 0)
			MarkedForRemoval = true;
	}

	/// <summary>
	/// 새로운 값으로 Override시
	/// </summary>
	/// <param name="value"></param>
	public void OverrideValue(float value)
	{
		Value = value;
		Build();
	}

	/// <summary>
	/// 새로운 시간으로 Override시
	/// </summary>
	/// <param name="value"></param>
	public void OverrideRemainingTime(float time)
	{
		RemainingValue = time;
		TotalTime = time;
	}

	/// <summary>
	/// 중재자 제거
	/// </summary>
	public void Dispose()
	{
		DebugX.Log("Modifier 제거");
		OnDispose?.Invoke(this);
	}

}
