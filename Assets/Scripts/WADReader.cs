using UnityEngine;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

[System.Serializable]
public static class WADReader
{
	static readonly string[] mapNamePatterns = { @"E\dM\d", @"MAP\d\d" };
	static readonly string[] mapEntryNames = { "THINGS", "LINEDEFS", "SIDEDEFS", "VERTEXES", "SEGS", "SSECTORS", "NODES", "SECTORS", "REJECT", "BLOCKMAP" };

	public static void loadWAD(this WAD wad, string pathName)
	{
		using (BinaryReader br = new BinaryReader(File.Open(pathName, FileMode.Open)))
		{
			// header
			wad.type = Encoding.ASCII.GetString(br.ReadBytes(4));
			wad.numLumps = br.ReadInt32();
			wad.infoTableOfs = br.ReadInt32();

			// directory
			wad.directory = new WAD.LumpInfo[wad.numLumps];
			br.BaseStream.Position = wad.infoTableOfs;
			for (int i = 0; i < wad.directory.Length; i++)
			{
				wad.directory[i].filePos = br.ReadInt32();
				wad.directory[i].size = br.ReadInt32();
				wad.directory[i].name = Encoding.ASCII.GetString(br.ReadBytes(8));
			}
		}
	}

	public static void loadMap(this WAD wad, string mapName, string pathName)
	{
		BinaryReader br = new BinaryReader(File.Open(pathName, FileMode.Open));
		bool foundMap = false;
		int index = 0;
		for (int i = 0; i < wad.directory.Length; i++)
		{
			if (string.Compare(wad.directory[i].name, mapName) == 0)
			{
				wad.map.name = wad.directory[i].name;
				foundMap = true;
				index = i;
				break;
			}
		}
		if (foundMap)
		{
			bool foundNonMapEntry = false;
			while (!foundNonMapEntry)
			{
				index++;
				if (IsMapEntry(wad.directory[index].name))
				{
					br.BaseStream.Position = wad.directory[index].filePos;
					switch (wad.directory[index].name)
					{
						case "THINGS":
							wad.map.things = new WAD.Map.Thing[wad.directory[index].size / 10];
							for (int i = 0; i < wad.map.things.Length; i++)
							{
								wad.map.things[i].x = br.ReadInt16();
								wad.map.things[i].y = br.ReadInt16();
								wad.map.things[i].angle = br.ReadInt16();
								wad.map.things[i].type = br.ReadInt16();
								wad.map.things[i].options = br.ReadInt16();
							}
							break;
						case "LINEDEFS":
							break;
						case "VERTEXES":
							break;
					}
				}
				else
				{
					foundNonMapEntry = true;
				}
			}
		}
	}

	public static bool IsMapEntry(string lumpName)
	{
		bool result = false;
		for (int i = 0; i < mapEntryNames.Length; i++)
		{
			if (lumpName == mapEntryNames[i])
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static bool IsMap(string lumpName)
	{
		bool result = false;
		for (int i = 0; i < mapNamePatterns.Length; i++)
		{
			if (Regex.IsMatch(lumpName, mapNamePatterns[i]))
			{
				result = true;
				break;
			}
		}
		return result;
	}
}