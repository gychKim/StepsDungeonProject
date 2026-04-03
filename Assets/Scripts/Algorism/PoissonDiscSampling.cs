using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PoissonDiscSampling
{
	public static List<Vector2> GeneratePoint(float radius, Vector2 sampleRegionSize, int numSamplesBeforRejection = 30, Vector2 offset = default, Vector2 startOffset = default)
	{
		// 그리드 셀의 크기를 계산한다.
		// 대각선의 길이 == 반지름
		// r^2 = s^2 + s^2
		// r^2 = 2 * s^2
		// s^2 = r^2 / 2
		// s = Sqrt(r^2 / 2)
		// s = r / Sqrt(2) 다.
		float cellSize = radius / Mathf.Sqrt(2);

		if(offset != default)
		{
			sampleRegionSize += offset;
		}

		// x축과 y축에 셀이 몇 개 들어가는지 알아야 한다.
		// 이는 셀 크기가 샘플 영역 크기에 몇 번 들어가는지를 나타낸다.
		// 두 값 모두 정수로 반올림한다.
		// grid는 셀에 대해 해당 셀에 있는 점들의 인덱스를 알려준다.
		int[,] grid = new int[Mathf.CeilToInt(sampleRegionSize.x / cellSize), Mathf.CeilToInt(sampleRegionSize.y / cellSize)];

		// 새로운 점들의 리스트
		List<Vector2> points = new List<Vector2>();


		// 새로운 점을 점들의 리스트에 추가하면, 그 점을 새로운 스폰 포인트로도 추가할 것이다.
		// 그 스폰 포인트 주변에 점들을 추가하려고 시도하고, 여러 번 시도했는데도 새로운 점을 추가할 위치를 찾지 못하면 스폰 포인트 리스트에서 그 점을 제거한다.
		List<Vector2> spawnPoints = new List<Vector2>();

		// 초기 스폰 지점 추가
		// 스폰 중심을 기준으로 여러 번 시도하여 새로운 지점을 생성한다.
		// 만약 적절한 위치를 찾지 못하면 해당 스폰 지점을 목록에서 제거한다.
		// 물론 문제는 몇 번 시도해야 제거할 수 있느냐 다.
		// 이를 위해 함수의 매개변수로 int numSamplesBeforRejection = 30을 추가한거다.
		// 이 값을 줄이면 알고리즘 속도를 높일 수 있지만, 그러면 지점이 촘촘하게 채워지지 않을 위험이 있다.
		spawnPoints.Add(sampleRegionSize / 2);

		while(spawnPoints.Count > 0)
		{
			int spawnIndex = Random.RangeOriginal(0, spawnPoints.Count);
			Vector2 spawnCentre = spawnPoints[spawnIndex];
			bool candidateAccepted = false;
			for (int i = 0; i < numSamplesBeforRejection; i++)
			{
				// 0에서 1 사이의 임의의 값에 난수를 곱한 값에 파이*2를 곱한다.
				// 이러면 라디안 단위로 임의의 각도를 얻을 수 있다.
				float angle = Random.Value * Mathf.PI * 2;

				// 방향을 나타내는 벡터를 생성
				Vector2 dir = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));

				// 후보 지점에 대한 벡터
				// 이 벡터는 스폰 중심 + 방향 * 반지름과 반지름*2 사이의 임의의 값이다.
				// 반지름을 최소값으로 설정하는 이유는, 이미 스폰 중심에 있는 지점 외부에 후보지점을 생성하고 있기 때문??
				Vector2 candidate = spawnCentre + dir * Random.Range(radius, radius * 2);

				// 지점을 수락할 지 여부
				if(IsVaild(candidate, sampleRegionSize, cellSize, radius, points, grid))
				{
					// 후보 지점을 포인트 목록에 추가
					points.Add(candidate);
					// 새로운 스폰 지점으로 추가
					spawnPoints.Add(candidate);
					// 어떤 셀에 위치하게 되는지 기록한다.
					// 해당 셀의 인덱스를 포인트 목록의 인덱스(1부터 시작하는 인덱스, 즉 0부터 시작하는 인덱스가 아님)로 저장한다.
					grid[(int)(candidate.x / cellSize), (int)(candidate.y / cellSize)] = points.Count;
					candidateAccepted = true; // 유효포인트가 있으니 true로 처리
					break;
				}
			}

			// 후보 지점이 없으면 다른 셀을 선택해야 한다.
			// 후보 포인트가 승인되면 스폰 포인트 목록에서 해당 포인트를 제거한다.
			if(!candidateAccepted)
			{
				spawnPoints.RemoveAt(spawnIndex);
			}
		}

		if(offset != default)
		{
			points = points.Select((pos) => pos += startOffset).ToList();
		}
		return points;
	}

	public static bool IsVaild(Vector2 candidate, Vector2 sampleRegionSize, float cellSize, float radius, List<Vector2> points, int[,] grid)
	{
		// 후보 포인터가 샘플 영역 내에 있는지 확인한다.
		if(candidate.x >= 0 && candidate.x < sampleRegionSize.x && candidate.y >= 0 && candidate.y < sampleRegionSize.y)
		{
			// 후보 점이 위치한 셀을 찾아서 주변 셀을 검색한다.
			int cellX = (int)(candidate.x / cellSize);
			int cellY = (int)(candidate.y / cellSize);

			// 5x5 블록을 검색한다.
			// 축에서 왼쪽으로 두 셀, 오른쪽으로 두 셀 떨어진 지점에서 시작해야 한다.
			// 이 변수가 그리드 범위를 벗어나지 않도록 해야 하므로 Max를 이용하여 0과 cellX - 2 중 더 큰 값으로 설정한다.
			int searchStartX = Mathf.Max(0, cellX - 2);
			// cellX + 2와 그리드의 x축 셀 개수에서 1을 뺀 값 중 더 작은 값을 int search end로 설정한다.???
			int searchEndX = Mathf.Min(cellX + 2, grid.GetLength(0) - 1);


			int searchStartY = Mathf.Max(0, cellY - 2);
			int searchEndY = Mathf.Min(cellY + 2, grid.GetLength(1) - 1);

			// 셀의 점 인덱스를 얻는다.
			for(int x = searchStartX; x <= searchEndX; x++)
			{
				for(int y = searchStartY; y <= searchEndY; y++)
				{
					// pointIndex가 -1이 된다면 해당 셀에 점이 없다는 뜻이 된다.
					int pointIndex = grid[x, y] - 1;
					if(pointIndex != -1)
					{
						// 후보점과 해당 점 사이의 거리를 구한다.
						float sqrDst = (candidate - points[pointIndex]).sqrMagnitude; // 최적화를 위해 제곱 거리를 사용 ???

						// 거리가 반경보다 작으면 오류가 없는 것으로 간주
						// 제곱 거리를 넣으면 왜 radius + radius를 하는가???
						if(sqrDst < radius * radius)
						{
							return false;
						}
					}
				}
			}
			return true;
		}
		return false;
	}
}
