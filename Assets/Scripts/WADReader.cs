using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Doom.IO
{
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

		static readonly string[] mapEntryNames = { "THINGS", "LINEDEFS", "SIDEDEFS", "VERTEXES", "SEGS", "SSECTORS", "NODES", "SECTORS", "REJECT", "BLOCKMAP" };

		static BinaryReader _bri;
		static BinaryReader bri
		{
			get
			{
				if (_bri == null)
				{
					_bri = CreateBinaryReader(iwadPath);
				}
				return _bri;
			}
		}

		public static string wadMode = "DOOM";
		public static string iwadPath = "/Resources/doom.wad";

		public static bool TryGetLump(string name, out Entry entry, int fromIndex = 0)
		{
			bool result = false;
			entry = new Entry(iwadPath);

			bri.BaseStream.Position = 0;

			string type = ParseASCII(bri.ReadBytes(4));
			int numLumps = bri.ReadInt32();
			int dirPos = bri.ReadInt32();

			bri.BaseStream.Position = dirPos + fromIndex * 16;

			for (int i = fromIndex; i < numLumps; i++)
			{
				entry.index = i;
				entry.pos = bri.ReadInt32();
				entry.size = bri.ReadInt32();
				entry.name = ParseASCII(bri.ReadBytes(8));

				if (entry.name == name)
				{
					result = true;
					break;
				}
			}

			return result;
		}

		public static Map LoadMap(string mapName)
		{
			Map map = new Map();
			Entry entry = new Entry();
			if (TryGetLump(mapName, out entry))
			{
				Debug.Log(entry);

				map.name = entry.name;

				for (int i = 0; i < mapEntryNames.Length; i++)
				{
					Entry mapEntry = new Entry();
					if (TryGetLump(mapEntryNames[i], out mapEntry, entry.index))
					{
						Debug.Log(mapEntry);

						switch (mapEntry.name)
						{
							case "THINGS":
								map.things = ReadToThings(mapEntry);
								break;
							case "LINEDEFS":
								map.linedefs = ReadToLinedefs(mapEntry);
								break;
							case "SIDEDEFS":
								map.sidedefs = ReadToSidedefs(mapEntry);
								break;
							case "VERTEXES":
								map.vertexes = ReadToVertexes(mapEntry);
								break;
							case "SEGS":
								map.segs = ReadToSegs(mapEntry);
								break;
							case "SSECTORS":
								map.subsectors = ReadToSubSectors(mapEntry);
								break;
							case "NODES":
								break;
							case "SECTORS":
								map.sectors = ReadToSectors(mapEntry);
								break;
						}
					}
				}
			}
			return map;
		}

		public static Thing[] ReadToThings(Entry entry)
		{
			Thing[] result = new Thing[entry.size / 10];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Thing thing = new Thing();
				thing.x = bri.ReadInt16();
				thing.y = bri.ReadInt16();
				thing.angle = bri.ReadInt16();
				thing.type = bri.ReadInt16();
				thing.options = bri.ReadInt16();
				result[i] = thing;
			}

			return result;
		}

		public static Linedef[] ReadToLinedefs(Entry entry)
		{
			Linedef[] result = new Linedef[entry.size / 14];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Linedef linedef = new Linedef();
				linedef.start = bri.ReadInt16();
				linedef.end = bri.ReadInt16();
				linedef.flags = bri.ReadInt16();
				linedef.types = bri.ReadInt16();
				linedef.tag = bri.ReadInt16();
				linedef.right = bri.ReadInt16();
				linedef.left = bri.ReadInt16();
				result[i] = linedef;
			}

			return result;
		}

		public static Sidedef[] ReadToSidedefs(Entry entry)
		{
			Sidedef[] result = new Sidedef[entry.size / 30];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Sidedef sidedef = new Sidedef();
				sidedef.xofs = bri.ReadInt16();
				sidedef.yofs = bri.ReadInt16();
				sidedef.texUpper = ParseASCII(bri.ReadBytes(8));
				sidedef.texLower = ParseASCII(bri.ReadBytes(8));
				sidedef.texMiddle = ParseASCII(bri.ReadBytes(8));
				sidedef.sector = bri.ReadInt16();
				result[i] = sidedef;
			}

			return result;
		}

		public static Vertex[] ReadToVertexes(Entry entry)
		{
			Vertex[] result = new Vertex[entry.size / 4];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Vertex vertex = new Vertex();
				vertex.x = bri.ReadInt16();
				vertex.y = bri.ReadInt16();
				result[i] = vertex;
			}

			return result;
		}

		public static Seg[] ReadToSegs(Entry entry)
		{
			Seg[] result = new Seg[entry.size / 12];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Seg seg = new Seg();
				seg.start = bri.ReadInt16();
				seg.end = bri.ReadInt16();
				seg.angle = bri.ReadInt16();
				seg.linedef = bri.ReadInt16();
				seg.direction = bri.ReadInt16();
				seg.offset = bri.ReadInt16();
				result[i] = seg;
			}

			return result;
		}

		public static SubSector[] ReadToSubSectors(Entry entry)
		{
			SubSector[] result = new SubSector[entry.size / 4];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				SubSector subsector = new SubSector();
				subsector.num = bri.ReadInt16();
				subsector.start = bri.ReadInt16();
				result[i] = subsector;
			}

			return result;
		}

		public static Sector[] ReadToSectors(Entry entry)
		{
			Sector[] result = new Sector[entry.size / 26];

			bri.BaseStream.Position = entry.pos;

			for (int i = 0; i < result.Length; i++)
			{
				Sector sector = new Sector();
				sector.floor = bri.ReadInt16();
				sector.ceiling = bri.ReadInt16();
				sector.flatFloor = ParseASCII(bri.ReadBytes(8));
				sector.flatCeiling = ParseASCII(bri.ReadBytes(8));
				sector.light = bri.ReadInt16();
				sector.special = bri.ReadInt16();
				sector.tag = bri.ReadInt16();
				result[i] = sector;
			}

			return result;
		}

		public static byte[,] GetPlaypal()
		{
			Entry entry;
			if (TryGetLump("PLAYPAL", out entry))
			{
				byte[,] result = new byte[entry.size / 768, 768];

				bri.BaseStream.Position = entry.pos;

				for (int i = 0; i < result.GetLength(0); i++)
				{
					for (int j = 0; j < result.GetLength(1); j++)
					{
						result[i, j] = bri.ReadByte();
					}
				}
				return result;
			}
			return null;
		}

		public static byte[,] GetColormap()
		{
			Entry entry;
			if (TryGetLump("COLORMAP", out entry))
			{
				byte[,] result = new byte[entry.size / 256, 256];

				bri.BaseStream.Position = entry.pos;

				for (int i = 0; i < result.GetLength(0); i++)
				{
					for (int j = 0; j < result.GetLength(1); j++)
					{
						result[i, j] = bri.ReadByte();
					}
				}
				return result;
			}
			return null;
		}

		public static BinaryReader CreateBinaryReader(string path) { return new BinaryReader(File.Open(Application.dataPath + path, FileMode.Open)); }
		// cleaning of string required since name is padded with zeroes if less than 8 bytes
		static string ParseASCII(byte[] bytes) { return new string(Encoding.ASCII.GetString(bytes).Where(char.IsLetterOrDigit).ToArray()); }
	}

	public struct Entry
	{
		public string wad;
		public int index, pos, size;
		public string name;

		public Entry(string wad)
		{
			this.wad = wad;

			index = -1;
			pos = -1;
			size = -1;
			name = "NULL";
		}

		override public string ToString()
		{
			return string.Format("DirEntry(WAD: {0}, Index: {1}, Pos: {2}, Size: {3}, Name: {4})", wad, index, pos, size, name);
		}
	}
}