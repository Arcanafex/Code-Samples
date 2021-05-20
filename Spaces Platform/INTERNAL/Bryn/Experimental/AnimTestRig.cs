using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AnimTestRig : MonoBehaviour
{
    public List<Spaces.Core.AnimatorWidget> animators;

    void Update()
    {
        animators = new List<Spaces.Core.AnimatorWidget>(FindObjectsOfType<Spaces.Core.AnimatorWidget>());
    }
    
    public void Play()
    {
        Debug.Log("Play");
        animators.ForEach(anim => anim.Play());
    }

    public void Pause()
    {
        Debug.Log("Pause");
        animators.ForEach(anim => anim.Pause());
    }

    public void Restart()
    {
        Debug.Log("Restart");
        animators.ForEach(anim => anim.Restart());
    }

    public void Stop()
    {
        Debug.Log("Stop");
        animators.ForEach(anim => anim.Stop());
    }
}