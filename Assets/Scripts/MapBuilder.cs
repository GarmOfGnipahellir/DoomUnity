using UnityEngine;
using Doom.IO;
using System.Collections.Generic;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MapBuilder : MonoBehaviour
{
	MeshRenderer _meshRenderer;
	MeshRenderer meshRenderer
	{
		get
		{
			if (_meshRenderer = null)
			{
				_meshRenderer = GetComponent<MeshRenderer>();
			}
			return _meshRenderer;
		}
	}
	MeshFilter _meshFilter;
	MeshFilter meshFilter
	{
		get
		{
			if (_meshFilter = null)
			{
				_meshFilter = GetComponent<MeshFilter>();
			}
			return _meshFilter;
		}
	}
	bool inited = false;
	public float scale = 1.0f / 64;
	public Map map;
	public List<Vector3> vertices;

	void Start()
	{
		map = WADReader.LoadMap("E1M1");

		vertices = new List<Vector3>();
		SubSector ssector = map.subsectors[0];
		for (int i = 0; i < ssector.num; i++)
		{
			Seg seg = map.segs[ssector.start + i];
			Vertex vert = map.vertexes[seg.start];
			Linedef line = map.linedefs[seg.linedef];
			Sidedef sideR = map.sidedefs[line.right];
			Sector sectorR = map.sectors[sideR.sector];
			if (line.left >= 0)
			{
				Sidedef sideL = map.sidedefs[line.left];
				Sector sectorL = map.sectors[sideL.sector];
				if (sectorR.ceiling != sectorL.ceiling)
				{
					vertices.Add(new Vector3(vert.x, sectorR.ceiling, vert.y) * scale);
				}
				if (sectorR.floor != sectorL.floor)
				{
					vertices.Add(new Vector3(vert.x, sectorR.floor, vert.y) * scale);
				}
			}
			else
			{
 				vertices.Add(new Vector3(vert.x, sectorR.floor, vert.y) * scale);
			}
		}

		int[] triangles = new int[vertices.Count * 2 - 1];
		for (int i = 0; i < triangles.Length / 3; i++)
		{
			triangles[i + 0] = 0;
			triangles[i + 1] = (i + 1) % vertices.Count;
			triangles[i + 2] = (i + 2) % vertices.Count;
		}

		Mesh mesh = new Mesh();

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles;
		mesh.RecalculateNormals();

		GetComponent<MeshFilter>().mesh = mesh;

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
		foreach (Vector3 vertex in vertices)
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