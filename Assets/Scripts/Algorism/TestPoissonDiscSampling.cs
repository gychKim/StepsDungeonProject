using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TestPoissonDiscSampling : MonoBehaviour
{
	public float radius = -1;
	public Vector2 regionSize = Vector2.one;
	public Vector2 calcullatedRegionSize = Vector2.one;
	public int rejectionSamples = 30;
	public Vector2 offset = Vector2.zero; // 0, -4 or 2, 4
	public Vector2 startOffset = Vector2.zero; // 0, 4 or -2, -4

	public float displayRadius = -1;

	List<Vector2> points;

	private bool isTest = false;

	private void OnValidate()
	{
		if(isTest) return;

		points = PoissonDiscSampling.GeneratePoint(radius, regionSize, rejectionSamples, offset, startOffset);

		calcullatedRegionSize = offset != default ? regionSize + offset : regionSize;
	}

	private void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(calcullatedRegionSize / 2, calcullatedRegionSize);
		if(points != null)
		{
			points = points.Select((vec) => vec = new Vector2(Mathf.CeilToInt(vec.x), Mathf.CeilToInt(vec.y))).ToList();
			foreach (Vector2 point in points)
				Gizmos.DrawSphere(point, displayRadius);
		}
	}

	public void Test(List<Vector2> list)
	{
		points = list;
		isTest = true;
	}

	// private void OnGUI()
	// {
	// 	// Rect(x좌표, y좌표, 너비, 높이)로 버튼의 위치와 크기를 설정합니다.
	// 	// GUI.Button은 버튼이 클릭되었을 때 true를 반환합니다.
	// 	if (GUI.Button(new Rect(20, 20, 150, 50), "함수 실행 버튼"))
	// 	{
	// 		//// 버튼을 클릭했을 때 실행할 함수를 호출합니다.
	// 		//transform.position = new Vector3(10, 0, 0);
	// 		//EnterRoom();

	// 		points = PoissonDiscSampling.GeneratePoint(radius, regionSize, rejectionSamples, offset, startOffset);

	// 		calcullatedRegionSize = offset != default ? regionSize + offset : regionSize;
	// 	}
	// }
}
