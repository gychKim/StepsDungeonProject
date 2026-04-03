public interface IMediator<TModifier, TQuery>
{
    void PerformQuery(object sender, TQuery query);
    void AddModifier(TModifier modifier);
    void RemoveAllModifier();
    void Update();
    void Clear();
}

public abstract class BaseMediator<TModifier, TQuery> : IMediator<TModifier, TQuery> where TModifier : BaseModifier<TQuery>
{
	/// <summary>
	/// 쿼리 실행
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="query"></param>
	public abstract void PerformQuery(object sender, TQuery query);
	
	/// <summary>
	/// 수정자 추가
	/// </summary>
	/// <param name="modifier"></param>
	public abstract void AddModifier(TModifier modifier);

	/// <summary>
	/// 중재자 초기화
	/// </summary>
	/// <param name="modifier"></param>
	public abstract void Clear();

	/// <summary>
	/// 적용한 수정자 모두 제거
	/// </summary>
	/// <param name="modifier"></param>
	public abstract void RemoveAllModifier();

	/// <summary>
	/// 수정자 갱신
	/// </summary>
	public abstract void Update();
}
