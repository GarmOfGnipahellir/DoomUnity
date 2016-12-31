using UnityEngine;
using System.Collections;
using Doom.IO;

[ExecuteInEditMode]
public class DoomLUT : MonoBehaviour
{
	byte[,] playpal;
	byte[,] colormap;
	Texture2D LUTTexture;
	Material material;

	[Range(0, 13)]
	public int CurrentPalette;

	void Start()
	{
		playpal = WADReader.GetPlaypal();
		colormap = WADReader.GetColormap();

		LUTTexture = new Texture2D(colormap.GetLength(0) * playpal.GetLength(0), colormap.GetLength(1));
		LUTTexture.filterMode = FilterMode.Point;
		for (int k = 0; k < playpal.GetLength(0); k++)
		{
			for (int i = 0; i < colormap.GetLength(0); i++)
			{
				for (int j = 0; j < colormap.GetLength(1); j++)
				{
					float r = (float)playpal[k, colormap[i, j] * 3 + 0] / 255;
					float g = (float)playpal[k, colormap[i, j] * 3 + 1] / 255;
					float b = (float)playpal[k, colormap[i, j] * 3 + 2] / 255;
					Color color = new Color(r, g, b);
					LUTTexture.SetPixel(i + k * colormap.GetLength(0), j, color);
				}
			}
		}
		LUTTexture.Apply();

		material = new Material(Shader.Find("Hidden/DoomLUT"));
		material.SetTexture("_LUTTex", LUTTexture);
		material.SetInt("_NumPals", playpal.GetLength(0));
		material.SetInt("_NumMaps", colormap.GetLength(0));
		material.SetInt("_NumCols", colormap.GetLength(1));
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		material.SetInt("_CurPal", CurrentPalette);
		Graphics.Blit(source, destination, material);
	}

	void OnGUI()
	{
		GUILayout.BeginArea(new Rect(5, 5, LUTTexture.width, LUTTexture.height), LUTTexture); GUILayout.EndArea();
	}
}
