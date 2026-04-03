using UnityEngine;

[CreateAssetMenu(fileName = "PropsTable", menuName = "Room/PropsTable")]
public class PropsTable : ScriptableObject
{
	[Header("테마")]
	public RoomTheme theme;

	[Header("테마에 사용되는 장식품들")]
	public PropObject[] propDataArr; 
}
