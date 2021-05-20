using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderCubemap : MonoBehaviour {

    public Camera cam;
    public Cubemap cube;

	// Use this for initialization
	void Start () {
        cube = new Cubemap(512, TextureFormat.ARGB32, false);
        cam.RenderToCubemap(cube);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
