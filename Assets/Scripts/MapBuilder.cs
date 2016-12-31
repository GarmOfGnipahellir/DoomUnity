using UnityEngine;
using Doom.IO;

public class MapBuilder : MonoBehaviour
{
	bool inited = false;
	public float scale = 1.0f / 64;
	public Map map;

	// Use this for initialization
	void Start()
	{
		map = WADReader.LoadMap("E1M1");

		inited = false;
	}

	void OnDrawGizmos()
	{
		if (!inited) return;

		Gizmos.color = Color.red;
		foreach (Thing thing in map.things)
		{
			Gizmos.DrawWireCube(new Vector3(thing.x, 0, thing.y) * scale, Vector3.one * 10 * scale);
		}
		Gizmos.color = Color.blue;
		foreach (Vertex vertex in map.vertexes)
		{
			Gizmos.DrawWireSphere(new Vector3(vertex.x, 0, vertex.y) * scale, 0.5f * 10 * scale);
		}
		Gizmos.color = Color.white;
		foreach (Linedef linedef in map.linedefs)
		{
			Vertex startVertex = map.vertexes[linedef.start];
			Vertex endVertex = map.vertexes[linedef.end];
			Gizmos.DrawLine(new Vector3(startVertex.x, 0, startVertex.y) * scale, new Vector3(endVertex.x, 0, endVertex.y) * scale);
		}
	}
}