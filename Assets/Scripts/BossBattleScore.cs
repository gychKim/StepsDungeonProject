using R3;
using UnityEngine;

public interface IBossBattleScore
{
    ReadOnlyReactiveProperty<int> ScoreRP { get; }

    void AddScore(int value);

    int GetScore();

}
public class BossBattleScore : IBossBattleScore
{
    public ReadOnlyReactiveProperty<int> ScoreRP => scoreRP;
    private ReactiveProperty<int> scoreRP = new ReactiveProperty<int>();
    
    public void AddScore(int value)
    {
        scoreRP.Value += value;
    }

    public int GetScore()
    {
        return scoreRP.Value;
    }
}
