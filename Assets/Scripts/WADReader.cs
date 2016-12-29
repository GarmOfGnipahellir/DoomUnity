using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
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
	public static Dictionary<string, int> mapLumps;

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

				// Debug.Log(string.Format("LUMP {0}, POS {1}, SIZE {2}, NAME {3}, MAP {4}", i, lump.pos, lump.size, lump.name, isMap.ToString().ToUpper()));

				if (isMap)
				{
					mapLumps.Add(lump.name, i);
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

	static BinaryReader createBinaryReader(string path) { return new BinaryReader(File.Open(Application.dataPath+path, FileMode.Open)); }

	// cleaning of string required since name is padded with zeroes if less than 8 bytes
	static string parseASCII(byte[] bytes) { return new string(Encoding.ASCII.GetString(bytes).Where(char.IsLetterOrDigit).ToArray()); }

	public static bool IsMap(string name) { return Regex.IsMatch(name, @"^(E\dM\d|MAP\d\d)"); }
	public static bool IsMapEntry(string name) { return Regex.IsMatch(name, @"^(THINGS|LINEDEFS|SIDEDEFS|VERTEXES|SEGS|SSECTORS|NODES|SECTORS|REJECT|BLOCKMAP)"); }

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