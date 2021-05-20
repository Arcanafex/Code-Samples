using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereoSkybox : MonoBehaviour
{
    public bool applyAtStart;

    public Material leftSkybox;
    public Material rightSkybox;

    public bool swapped;

    public Camera leftEye;
    public Camera rightEye;

    private Skybox _leftComponent;
    private Skybox _rightComponent;

    // Use this for initialization
    void Start()
    {
        if (applyAtStart)
        {
            bool initialSwap = swapped;
            InitializeSkyboxes();
            swapped = initialSwap;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
            InitializeSkyboxes();
    }

    public void InitializeSkyboxes()
    {
        _leftComponent = leftEye.gameObject.GetComponent<Skybox>();
        _rightComponent = rightEye.gameObject.GetComponent<Skybox>();

        if (!_leftComponent)
            _leftComponent = leftEye.gameObject.AddComponent<Skybox>();

        if (!_rightComponent)
            _rightComponent = rightEye.gameObject.AddComponent<Skybox>();

        _leftComponent.material = swapped ? rightSkybox : leftSkybox;
        _rightComponent.material = swapped ? leftSkybox : rightSkybox;

        DynamicGI.UpdateEnvironment();

        swapped = !swapped;
    }
}
