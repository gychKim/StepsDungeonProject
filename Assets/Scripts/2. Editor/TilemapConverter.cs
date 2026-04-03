using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapConverter : MonoBehaviour
{
	[Header("Settings")]
	public Tilemap targetTilemap;
	public RoomDecoDataSO targetSO;

	[ContextMenu("Export to SO")]
	public void Export()
	{
		if (targetTilemap == null || targetSO == null)
		{
			Debug.LogError("대상 타일맵이나 SO가 비어있다. 확인해라.");
			return;
		}

		// 1. 범위 압축 및 계산 (고무줄 줄이기)
		targetTilemap.CompressBounds();
		BoundsInt bounds = targetTilemap.cellBounds;

		// 2. SO 데이터 초기화
		targetSO.roomSize = new Vector2Int(bounds.size.x, bounds.size.y);
		targetSO.tileDataArr = new TileType[targetSO.roomSize.x * targetSO.roomSize.y];

		Debug.Log($"익스포트 시작: 범위 {bounds.position}, 크기 {targetSO.roomSize}");

		// 3. 루프를 돌며 데이터 추출
		for (int x = bounds.xMin; x < bounds.xMax; x++)
		{
			for (int y = bounds.yMin; y < bounds.yMax; y++)
			{
				Vector3Int cellPos = new Vector3Int(x, y, 0);
				TileBase tile = targetTilemap.GetTile(cellPos);

				// 상대 좌표 계산 (xMin, yMin을 빼서 0,0부터 시작하게 만듦)
				int indexX = x - bounds.xMin;
				int indexY = y - bounds.yMin;

				targetSO.SetTile(indexX, indexY, GetTileType(tile));
			}
		}

		targetSO.groundPosList.Clear();
		targetSO.wallPosList.Clear();

		for (int x = 0; x < targetSO.roomSize.x; x++)
		{
			for (int y = 0; y < targetSO.roomSize.y; y++)
			{
				// 1. 바닥 프롭 후보지: 현재 Floor이고 위(y+1)가 Empty인 곳
				if (IsGround(x, y))
				{
					targetSO.groundPosList.Add(new Vector2Int(x, y + 1));
				}

				// 2. 벽 프롭 후보지: 현재 Ground이고 위(y+2)가 Wall인 곳
				if(IsWall(x, y))
				{
					targetSO.wallPosList.Add(new Vector2Int(x, y + 2));
				}
			}
		}

		// 바닥 타일의 Y 좌표 저장
		targetSO.groundLevelY = targetSO.groundPosList.Count > 0 ? targetSO.groundPosList[0].y : 0;

		// 4. 변경사항 저장 (에디터 전용)
#if UNITY_EDITOR
		UnityEditor.EditorUtility.SetDirty(targetSO); // 저장
		UnityEditor.AssetDatabase.SaveAssets(); // 실제 SSD 에 저장
#endif
		Debug.Log("익스포트 완료. 데이터가 SO에 안전하게 구워졌다.");
	}

	private TileType GetTileType(TileBase tile)
	{
		if (tile == null) return TileType.Empty;

		string tileName = tile.name;

		// 네가 제안한 이름 기반 판별 로직
		if (tileName.Contains("Wall")) return TileType.Wall;
		if (tileName.Contains("Ground")) return TileType.Ground;

		return TileType.Empty;
	}

	private bool IsWall(int x, int y) => targetSO.GetTile(x, y) == TileType.Wall;
	private bool IsGround(int x, int y) => targetSO.GetTile(x, y) == TileType.Ground && targetSO.GetTile(x, y + 1) == TileType.Wall;
}
