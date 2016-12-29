using UnityEngine;

public class MapBuilder : MonoBehaviour
{
	bool inited = false;
	public float scale = 1.0f / 64;
	public MapData mapData;

	// Use this for initialization
	void Start()
	{
		WADReader.loadWAD();
		mapData = WADReader.loadMap("E1M1");

		inited = true;
	}

	void OnDrawGizmos()
	{
		if (!inited) return;

		Gizmos.color = Color.red;
		foreach (MapData.Thing thing in mapData.things)
		{
			Gizmos.DrawWireCube(new Vector3(thing.x, 0, thing.y) * scale, Vector3.one * 10 * scale);
		}
		Gizmos.color = Color.blue;
		foreach (MapData.Vertex vertex in mapData.vertexes)
		{
			Gizmos.DrawWireSphere(new Vector3(vertex.x, 0, vertex.y) * scale, 0.5f * 10 * scale);
		}
		Gizmos.color = Color.white;
		foreach (MapData.Linedef linedef in mapData.linedefs)
		{
			MapData.Vertex startVertex = mapData.vertexes[linedef.start];
			MapData.Vertex endVertex = mapData.vertexes[linedef.end];
			Gizmos.DrawLine(new Vector3(startVertex.x, 0, startVertex.y) * scale, new Vector3(endVertex.x, 0, endVertex.y) * scale);
		}
	}

	// Update is called once per frame
	void Update()
	{

	}
}

[System.Serializable]
public class MapData
{
	public string name;
	public Thing[] things;
	public Linedef[] linedefs;
	public Sidedef[] sidedefs;
	public Vertex[] vertexes;
	public Seg[] segs;
	public SubSector[] subsectors;
	public Node[] nodes;
	public Sector[] sectors;

	[System.Serializable]
	public struct Thing { public int x, y, angle, type, options; }
	[System.Serializable]
	public struct Linedef { public int start, end, flags, types, tag, right, left; }
	[System.Serializable]
	public struct Sidedef { public int xofs, yofs, sector; public string texUpper, texLower, texMiddle; }
	[System.Serializable]
	public struct Vertex { public int x, y; }
	[System.Serializable]
	public struct Seg { public int start, end, angle, linedef, direction, offset; }
	[System.Serializable]
	public struct SubSector { public int num, start; }
	[System.Serializable]
	public struct Node { public int x, y, dx, dy, yul, yll, xul, xll, yur, ylr, xur, xlr, lft, rgt; }
	[System.Serializable]
	public struct Sector { public int floor, ceiling, light, special, tag; public string flatFloor, flatCeiling; }
}