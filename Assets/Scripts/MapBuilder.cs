using UnityEngine;

public class MapBuilder : MonoBehaviour
{
	public WAD wad;

	// Use this for initialization
	void Start()
	{
		wad = new WAD();
		wad.loadWAD(Application.dataPath + "/Resources/DOOM.WAD");
		wad.loadMap("E1M1", Application.dataPath + "/Resources/DOOM.WAD");
	}

	// Update is called once per frame
	void Update()
	{

	}
}