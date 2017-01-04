using UnityEngine;
using Doom.IO;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapBuilder : MonoBehaviour
{
	public MeshRenderer meshRenderer;
	public MeshFilter meshFilter;
	bool inited = false;
	public float scale = 1.0f / 64;
	public Map map;
	public List<Vector3> vertices;

	void Start()
	{
		map = WADReader.LoadMap("E1M1");

		vertices = new List<Vector3>();
		List<CombineInstance> floorCIs = new List<CombineInstance>();
		foreach (SubSector ssector in map.subsectors)
		{
			List<Vector3> floorVerts = new List<Vector3>();
			// SubSector ssector = map.subsectors[0];
			for (int i = 0; i < ssector.num; i++)
			{
				Seg seg = map.segs[ssector.start + i];
				Vertex vertS = map.vertexes[seg.start];
				Vertex vertE = map.vertexes[seg.end];
				Linedef line = map.linedefs[seg.linedef];
				Sidedef sideR = map.sidedefs[line.right];
				Sector sectorR = map.sectors[sideR.sector];
				if (line.left >= 0)
				{
					Sidedef sideL = map.sidedefs[line.left];
					Sector sectorL = map.sectors[sideL.sector];
				}
				Vector3 vStart = new Vector3(vertS.x, 0, vertS.y) * scale;
				Vector3 vEnd = new Vector3(vertE.x, 0, vertS.y) * scale;
				if (!floorVerts.Contains(vStart)) floorVerts.Add(vStart);
				if (!floorVerts.Contains(vEnd)) floorVerts.Add(vEnd);
			}

			List<int> floorTris = new List<int>();
			int p0 = 0;
			int pHelper = 1;
			for (int i = 2; i < floorVerts.Count; i++)
			{
				int pTemp = i;
				floorTris.AddRange(new int[] { p0, pHelper, pTemp });
				pHelper = pTemp;
			}

			CombineInstance ci = new CombineInstance();
			Mesh mesh = new Mesh();
			mesh.vertices = floorVerts.ToArray();
			mesh.triangles = floorTris.ToArray();
			mesh.RecalculateNormals();
			ci.mesh = mesh;
			ci.transform = transform.localToWorldMatrix;
			floorCIs.Add(ci);
		}


		Mesh combinedFloor = new Mesh();

		combinedFloor.CombineMeshes(floorCIs.ToArray());

		meshFilter.sharedMesh = combinedFloor;

		inited = true;
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
		foreach (Vector3 vertex in meshFilter.sharedMesh.vertices)
		{
			Gizmos.DrawWireSphere(vertex, 5 * scale);
		}/*
		Gizmos.color = Color.white;
		foreach (Linedef linedef in map.linedefs)
		{
			Vertex startVertex = map.vertexes[linedef.start];
			Vertex endVertex = map.vertexes[linedef.end];
			Sidedef sidedefRight = map.sidedefs[linedef.right];
			Sector sectorRight = map.sectors[sidedefRight.sector];
			Gizmos.DrawLine(
				new Vector3(startVertex.x, sectorRight.floor, startVertex.y) * scale, 
				new Vector3(endVertex.x, sectorRight.floor, endVertex.y) * scale);
			if (linedef.left >= 0)
			{
				Sidedef sidedefLeft = map.sidedefs[linedef.left];
				Sector sectorLeft = map.sectors[sidedefLeft.sector];
				Gizmos.DrawLine(
					new Vector3(startVertex.x, sectorLeft.floor, startVertex.y) * scale, 
					new Vector3(endVertex.x, sectorLeft.floor, endVertex.y) * scale);
			}
		}*/
	}
}