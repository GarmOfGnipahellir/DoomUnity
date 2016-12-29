using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System;

public class WADReader
{
	private static WADReader _instance;
	public static WADReader instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new WADReader();
			}
			return _instance;
		}
	}

	public static string wadMode = "DOOM";
	public static string iwadPath = "/Resources/DOOM.WAD";

	public static LumpPointer[] lumpPointers;
	public static int[,] playpal;
	public static Dictionary<string, int> mapLumps;
	public static Dictionary<string, int> textureLumps;

	public static void loadWAD()
	{
		using (BinaryReader br = createBinaryReader(iwadPath))
		{

			string type = parseASCII(br.ReadBytes(4));
			int numLumps = br.ReadInt32();
			int dirPos = br.ReadInt32();

			Debug.Log(string.Format("LOADING {0}, SIZE {1}, POS {2}", type, numLumps, dirPos));

			lumpPointers = new LumpPointer[numLumps];
			mapLumps = new Dictionary<string, int>();
			textureLumps = new Dictionary<string, int>();

			int playpalLump = -1;

			// populate lump pointer array
			br.BaseStream.Position = dirPos;
			for (int i = 0; i < lumpPointers.Length; i++)
			{
				LumpPointer lump = new LumpPointer(iwadPath);
				lump.pos = br.ReadInt32();
				lump.size = br.ReadInt32();
				// cleaning of string required since name is padded with zeroes if less than 8 bytes
				lump.name = parseASCII(br.ReadBytes(8));
				lumpPointers[i] = lump;

				bool isMap = IsMap(lump.name);

				//Debug.Log(string.Format("LUMP {0}, POS {1}, SIZE {2}, NAME {3}, MAP {4}", i, lump.pos, lump.size, lump.name, isMap.ToString().ToUpper()));

				if (IsPlaypal(lump.name)) playpalLump = i;
				if (isMap) mapLumps.Add(lump.name, i);
				if (IsTexture(lump.name)) textureLumps.Add(lump.name, i);
			}

			// load playpal
			br.BaseStream.Position = lumpPointers[playpalLump].pos;
			int numPals = lumpPointers[playpalLump].size / 768;
			playpal = new int[numPals, 768];
			for (int pal = 0; pal < playpal.GetLength(0); pal++)
			{
				for (int rgb = 0; rgb < playpal.GetLength(1) / 3; rgb++)
				{
					playpal[pal, rgb * 3 + 0] = br.ReadByte();
					playpal[pal, rgb * 3 + 1] = br.ReadByte();
					playpal[pal, rgb * 3 + 2] = br.ReadByte();

					// string hex = BitConverter.ToString(new byte[] { (byte)playpal[pal, rgb * 3 + 0], (byte)playpal[pal, rgb * 3 + 1], (byte)playpal[pal, rgb * 3 + 2] }).Replace("-", string.Empty);
					// Debug.Log(string.Format("Adding <color=#{3}>COLOR</color> {0} {1} {2} to PLAYPAL", playpal[pal, rgb * 3 + 0], playpal[pal, rgb * 3 + 1], playpal[pal, rgb * 3 + 2], hex));
				}
			}
		}
	}

	public static MapData loadMap(string mapName)
	{
		using (BinaryReader br = createBinaryReader(iwadPath))
		{
			MapData md = new MapData();

			int index = -1;
			if (mapLumps.TryGetValue(mapName, out index))
			{
				md.name = mapName;

				index++;
				while (IsMapEntry(lumpPointers[index].name))
				{
					LumpPointer lump = lumpPointers[index];
					br.BaseStream.Position = lump.pos;

					switch (lump.name)
					{
						case "THINGS":
							md.things = new MapData.Thing[lump.size / 10];
							for (int i = 0; i < md.things.Length; i++)
							{
								MapData.Thing thing = new MapData.Thing();
								thing.x = br.ReadInt16();
								thing.y = br.ReadInt16();
								thing.angle = br.ReadInt16();
								thing.type = br.ReadInt16();
								thing.options = br.ReadInt16();
								md.things[i] = thing;
							}
							break;
						case "LINEDEFS":
							md.linedefs = new MapData.Linedef[lump.size / 14];
							for (int i = 0; i < md.linedefs.Length; i++)
							{
								MapData.Linedef linedef = new MapData.Linedef();
								linedef.start = br.ReadInt16();
								linedef.end = br.ReadInt16();
								linedef.flags = br.ReadInt16();
								linedef.types = br.ReadInt16();
								linedef.tag = br.ReadInt16();
								linedef.right = br.ReadInt16();
								linedef.left = br.ReadInt16();
								md.linedefs[i] = linedef;
							}
							break;
						case "SIDEDEFS":
							md.sidedefs = new MapData.Sidedef[lump.size / 30];
							for (int i = 0; i < md.sidedefs.Length; i++)
							{
								MapData.Sidedef sidedef = new MapData.Sidedef();
								sidedef.xofs = br.ReadInt16();
								sidedef.yofs = br.ReadInt16();
								sidedef.texUpper = parseASCII(br.ReadBytes(8));
								sidedef.texLower = parseASCII(br.ReadBytes(8));
								sidedef.texMiddle = parseASCII(br.ReadBytes(8));
								sidedef.sector = br.ReadInt16();
								md.sidedefs[i] = sidedef;
							}
							break;
						case "VERTEXES":
							md.vertexes = new MapData.Vertex[lump.size / 4];
							for (int i = 0; i < md.vertexes.Length; i++)
							{
								MapData.Vertex vertex = new MapData.Vertex();
								vertex.x = br.ReadInt16();
								vertex.y = br.ReadInt16();
								md.vertexes[i] = vertex;
							}
							break;
						case "SEGS":
							md.segs = new MapData.Seg[lump.size / 12];
							for (int i = 0; i < md.segs.Length; i++)
							{
								MapData.Seg seg = new MapData.Seg();
								seg.start = br.ReadInt16();
								seg.end = br.ReadInt16();
								seg.angle = br.ReadInt16();
								seg.linedef = br.ReadInt16();
								seg.direction = br.ReadInt16();
								seg.offset = br.ReadInt16();
								md.segs[i] = seg;
							}
							break;
						case "SSECTORS":
							md.subsectors = new MapData.SubSector[lump.size / 4];
							for (int i = 0; i < md.subsectors.Length; i++)
							{
								MapData.SubSector subsector = new MapData.SubSector();
								subsector.num = br.ReadInt16();
								subsector.start = br.ReadInt16();
								md.subsectors[i] = subsector;
							}
							break;
						case "SECTORS":
							md.sectors = new MapData.Sector[lump.size / 26];
							for (int i = 0; i < md.sectors.Length; i++)
							{
								MapData.Sector sector = new MapData.Sector();
								sector.floor = br.ReadInt16();
								sector.ceiling = br.ReadInt16();
								sector.flatFloor = parseASCII(br.ReadBytes(8));
								sector.flatCeiling = parseASCII(br.ReadBytes(8));
								sector.light = br.ReadInt16();
								sector.special = br.ReadInt16();
								sector.tag = br.ReadInt16();
								md.sectors[i] = sector;
							}
							break;
					}

					index++;
				}
			}

			return md;
		}
	}

	public static MapData.Patch loadPatch(BinaryReader br, string texName)
	{
		MapData.Patch patch = new MapData.Patch();



		return patch;
	}

	static BinaryReader createBinaryReader(string path) { return new BinaryReader(File.Open(Application.dataPath+path, FileMode.Open)); }

	// cleaning of string required since name is padded with zeroes if less than 8 bytes
	static string parseASCII(byte[] bytes) { return new string(Encoding.ASCII.GetString(bytes).Where(char.IsLetterOrDigit).ToArray()); }

	public static bool IsPlaypal(string name) { return Regex.IsMatch(name, @"^PLAYPAL"); }
	public static bool IsMap(string name) { return Regex.IsMatch(name, @"^(E\dM\d|MAP\d\d)"); }
	public static bool IsMapEntry(string name) { return Regex.IsMatch(name, @"^(THINGS|LINEDEFS|SIDEDEFS|VERTEXES|SEGS|SSECTORS|NODES|SECTORS|REJECT|BLOCKMAP)"); }
	public static bool IsTexture(string name) { return Regex.IsMatch(name, @"^TEXTURE\d"); }

	public struct LumpPointer
	{
		public string wad;
		public int pos;
		public int size;
		public string name;

		public LumpPointer(string wad)
		{
			this.wad = wad;

			pos = -1;
			size = -1;
			name = "NULL";
		}
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

	public Dictionary<string, Flat> flats;
	public Dictionary<string, Patch> patches;

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

	[System.Serializable]
	public struct Flat {  }
	[System.Serializable]
	public struct Patch {  }
}