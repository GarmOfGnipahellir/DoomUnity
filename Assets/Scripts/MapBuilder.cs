using UnityEngine;
using System.Collections;

public class MapBuilder : MonoBehaviour {
	public WADReader wadReader;
	// Use this for initialization
	void Start () {
		wadReader = new WADReader(Application.dataPath+"/Resources/DOOM.WAD");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
