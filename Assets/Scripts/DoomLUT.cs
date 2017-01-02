using UnityEngine;
using Doom.IO;
using UnityStandardAssets.ImageEffects;

[ExecuteInEditMode]
public class DoomLUT : MonoBehaviour
{
	byte[,] playpal;
	byte[,] colormap;
	public int textureSize = 16;
	public Texture3D LUTTexture;

	[Range(0, 13)]
	public int CurrentPalette;

	void Start()
	{
		playpal = WADReader.GetPlaypal();
		colormap = WADReader.GetColormap();

		LUTTexture = new Texture3D(textureSize, textureSize, textureSize, TextureFormat.RGB24, false);
		LUTTexture.filterMode = FilterMode.Point;
		Color[] colors = new Color[textureSize * textureSize * textureSize];
		for (int r = 0; r < textureSize; r++)
		{
			for (int g = 0; g < textureSize; g++)
			{
				for (int b = 0; b < textureSize; b++)
				{
					Vector4 source = new Vector4(r, g, b) / textureSize;
					Vector4 color = Vector4.zero;
					for (int i = 0; i < playpal.GetLength(1) / 3; i++)
					{
						Vector4 target = new Vector4(playpal[0, i * 3], playpal[0, i * 3 + 1], playpal[0, i * 3 + 2]) / 255;
						if (Vector4.Distance(source, target) < Vector4.Distance(source, color))
						{
							color = target;
						}
					}
					color.w = 1;
					colors[r + g * textureSize + b * textureSize * textureSize] = color;
				}
			}
		}
		LUTTexture.SetPixels(colors);
		LUTTexture.Apply();

		ColorCorrectionLookup lookup = GetComponent<ColorCorrectionLookup>();
		lookup.converted3DLut = LUTTexture;
	}
}
