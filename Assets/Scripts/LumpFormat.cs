﻿namespace Doom.IO
{
	[System.Serializable]
	public class Map
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
	}
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