using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class WADReader {
	public WADReader(string pathName) {
		loadWAD(pathName);
	}

	public void loadWAD(string pathName) {
		using (BinaryReader br = new BinaryReader(File.Open(pathName, FileMode.Open))) {
			// length
			Debug.Log("File length:\t\t"+br.BaseStream.Length);

			// header
			string wadType = Encoding.ASCII.GetString(br.ReadBytes(4));
			Debug.Log("WAD type:\t\t"+wadType);
			Int32 numLumps = BitConverter.ToInt32(br.ReadBytes(4), 0);
			Debug.Log("Number of lumps:\t"+numLumps);
			Int32 dirOffset = BitConverter.ToInt32(br.ReadBytes(4), 0);
			Debug.Log("Directory offset:\t\t"+dirOffset);

			// directory
			br.BaseStream.Position = dirOffset;
			for (int i = 0; i < numLumps; i++) {
				Debug.Log("Lump index:\t\t"+i);
				Int32 lumpOffset = BitConverter.ToInt32(br.ReadBytes(4), 0);
				Debug.Log("\tLump offset:\t\t"+lumpOffset);
				Int32 lumpSize = BitConverter.ToInt32(br.ReadBytes(4), 0);
				Debug.Log("\tLump size:\t\t"+lumpSize);
				string lumpName = Encoding.ASCII.GetString(br.ReadBytes(8));
				Debug.Log("\tLump name:\t\t"+lumpName);

				if (Regex.IsMatch(lumpName, @"E\dM\d")) {
					Debug.Log(lumpName+" recognized as map lump");
				}
			}
		}
	}
}
