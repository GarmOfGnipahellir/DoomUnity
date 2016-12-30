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

		static BinaryReader bri;

		public static string wadMode = "DOOM";
		public static string iwadPath = "/Resources/doom.wad";

		public WADReader()
		{
			bri = CreateBinaryReader(iwadPath);
		}

		public static bool TryGetLump(string name, out Entry entry, int fromIndex = 0)
		{
			bool result = false;
			entry = new Entry(iwadPath);

			using (BinaryReader br = CreateBinaryReader(iwadPath))
			{
				string type = ParseASCII(br.ReadBytes(4));
				int numLumps = br.ReadInt32();
				int dirPos = br.ReadInt32();

				br.BaseStream.Position = dirPos + fromIndex * 16;

				for (int i = fromIndex; i < numLumps; i++)
				{
					entry.index = i;
					entry.pos = br.ReadInt32();
					entry.size = br.ReadInt32();
					entry.name = ParseASCII(br.ReadBytes(8));

					if (entry.name == name)
					{
						result = true;
						break;
					}
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


					}
				}
			}
			return map;
		}

		public static Thing[] ReadToThings(Entry entry)
		{
			Thing[] result = new Thing[entry.size / 10];

			using (BinaryReader br = CreateBinaryReader(iwadPath))
			{
				br.BaseStream.Position = entry.pos;

				for (int i = 0; i < result.Length; i++)
				{
					Thing thing = new Thing();
					thing.x = br.ReadInt16();
					thing.y = br.ReadInt16();
					thing.angle = br.ReadInt16();
					thing.type = br.ReadInt16();
					thing.options = br.ReadInt16();
					result[i] = thing;
				}
			}

			return result;
		}

		public static Playpal[] ReadToPlaypal(Entry entry)
		{
			Playpal[] result = new Playpal[entry.size / 768];

			using (BinaryReader br = CreateBinaryReader(iwadPath))
			{
				br.BaseStream.Position = entry.pos;

				for (int i = 0; i < result.Length; i++)
				{
					result[i] = new Playpal(br.ReadBytes(768));
				}
			}

			return result;
		}

		public static Colormap[] ReadToColormap(Entry entry)
		{
			Colormap[] result = new Colormap[entry.size / 256];

			using (BinaryReader br = CreateBinaryReader(iwadPath))
			{
				br.BaseStream.Position = entry.pos;

				for (int i = 0; i < result.Length; i++)
				{
					result[i] = new Colormap(br.ReadBytes(256));
				}
			}

			return result;
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