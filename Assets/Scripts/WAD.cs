using System;
using System.Collections.Generic;

[Serializable]
public class WAD
{
	public string type;
	public int numLumps;
	public int infoTableOfs;
	public LumpInfo[] directory;
	public Map map;

	[Serializable]
	public struct LumpInfo
	{
		public string name;
		public int filePos, size;
	}

	[Serializable]
	public struct Map
	{
		public string name;
		public Thing[] things;
		public Linedef[] linedefs;
		public Vertex[] vertexes;

		[Serializable]
		public struct Thing
		{
			public int x, y, angle, type, options;
		}

		[Serializable]
		public struct Linedef
		{
			public int vertexStart, vertexEnd, flags, function, tag, sidedefRight, sidedefLeft;
		}

		[Serializable]
		public struct Sidedef
		{

		}

		[Serializable]
		public struct Vertex
		{
			public int x, y;
		}

		[Serializable]
		public struct Segment
		{

		}

		[Serializable]
		public struct Subsector
		{

		}

		[Serializable]
		public struct Node
		{

		}

		[Serializable]
		public struct Sector
		{

		}
	}
}