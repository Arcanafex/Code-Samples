using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spaces.Core;

public class ComponentSubclassingTest : MonoBehaviour {

    public GameObject testObject;
    private AssetHandlerWidget widget;
    private ModelWidget model;

	// Use this for initialization
	void Start () {
        testObject = new GameObject("Test Object");
        widget = testObject.AddComponent<AssetHandlerWidget>();
	}
	
	// Update is called once per frame
	void Update () {
		
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //model = testObject.AddComponent<ModelWidget>();
            //JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(widget), model);
            //Destroy(widget);
            //widget = widget as ModelWidget;
        }
	}
}
