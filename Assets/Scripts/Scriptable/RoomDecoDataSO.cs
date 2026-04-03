using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomDecoDataSO", menuName = "Room/RoomDecoDataSO")]
public class RoomDecoDataSO : ScriptableObject
{
	public RoomTheme roomTheme;
	public TileType[] tileDataArr;
	public Vector2Int roomSize;

	public Vector2Int objectPosition;
	public Vector2Int playerStartPosition;

	public float groundLevelY; // 바닥 타일의 Y 좌표 (월드 좌표 기준)

	public List<Vector2Int> groundPosList; // 바닥 타일 위치
	public List<Vector2Int> wallPosList;   // 벽 타일 위치

	public TileType GetTile(int x, int y)
	{
		return tileDataArr[y * roomSize.x + x];
	}

	public void SetTile(int x, int y, TileType type)
	{
		tileDataArr[y * roomSize.x + x] = type;
	}
}
