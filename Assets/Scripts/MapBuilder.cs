using UnityEngine;

public class MapBuilder : MonoBehaviour
{
	bool inited = false;
	public float scale = 1.0f / 64;
	public MapData mapData;
	public Texture2D playpal;

	// Use this for initialization
	void Start()
	{
		WADReader.loadWAD();
		mapData = WADReader.loadMap("E1M1");

		int scl = 8;
		playpal = new Texture2D(16 * scl * 7, 16 * scl * 2, TextureFormat.RGBA32, false, true);
		for (int pal = 0; pal < 14; pal++)
		{
			for (int rgb = 0; rgb < 256; rgb++)
			{
				int x = rgb % 16 + (pal % 7) * 16;
				int y = Mathf.FloorToInt(rgb / 16) + Mathf.FloorToInt(pal / 7) * 16;

				Color col = new Color((float)WADReader.playpal[pal, rgb * 3 + 0] / 255, (float)WADReader.playpal[pal, rgb * 3 + 1] / 255, (float)WADReader.playpal[pal, rgb * 3 + 2] / 255);
				Color[] cols = new Color[scl * scl];

				for (int i = 0; i < cols.Length; i++)
				{
					cols[i] = col;
				}
				playpal.SetPixels(x * scl, y * scl, scl, scl, cols);
			}
		}
		playpal.Apply();

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

	void OnGUI()
	{
		if (!inited) return;

		GUILayout.Box(playpal);
	}

	// Update is called once per frame
	void Update()
	{

	}
}