using UnityEngine;
using UnityEngine.Tilemaps;
[CreateAssetMenu(fileName = "RoomTileData", menuName = "Dungeon/RoomTileData")]
public class RoomTileDataSO : ScriptableObject
{
	[Header("테마 이름")]
	public string themeName;

	[Header("테마 타일")]
	public TileBase groundRuletile; // 바닥 타일
	public TileBase wallRuleTile;   // 벽 타일
}
