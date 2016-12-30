using UnityEngine;
using Doom.IO;

public class MapBuilder : MonoBehaviour
{
	bool inited = false;
	public float scale = 1.0f / 64;
	public Map map;
	public Playpal[] playpal;
	public Colormap[] colormap;
	public Texture2D playpalTex;
	public Texture2D colormapTex;
	public Texture2D patch;

	// Use this for initialization
	void Start()
	{
		map = WADReader.LoadMap("E1M1");

		Entry playpalEntry;
		if (WADReader.TryGetLump("PLAYPAL", out playpalEntry))
		{
			playpal = WADReader.ReadToPlaypal(playpalEntry);
		}

		Entry colormapEntry;
		if (WADReader.TryGetLump("COLORMAP", out colormapEntry))
		{
			colormap = WADReader.ReadToColormap(colormapEntry);
		}

		//MapData.Patch mdp = WADReader.LoadPatch(WADReader.CreateBinaryReader(WADReader.iwadPath), "DOOR3");

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

	void OnGUI()
	{
		if (!inited) return;

		GUILayout.BeginArea(new Rect(5, 5, playpalTex.width, playpalTex.height), playpalTex);							GUILayout.EndArea();
		GUILayout.BeginArea(new Rect(10 + playpalTex.width, 5, colormapTex.width, colormapTex.height), colormapTex);	GUILayout.EndArea();
	}
}