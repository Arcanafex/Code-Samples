using UnityEngine;
using System.IO;
using System.Collections;

[CreateAssetMenu(menuName = "AI/Targeting Behaviour")]
public class TargetingBehaviourParameters : ScriptableObject
{
    private const string CONFIG_DIR = "C:/BUILDS/config";
    private string CONFIG_PATH
    {
        get
        {
            return CONFIG_DIR + "/" + this.name + ".json";
        }
    }

    public float acquireTargetsFrequency = 0.5f;
    public float waitToFireCheckFrequency = 1;
    public int maxConcurrantShooters = 2;
    public int maxTargetPopularity = 2;

    public bool randomTargetSelection = false;

    public float aimVariance = 0.2f;
    public int[] burstSizes;

    public float targetingRange = 20;
    public float firingDuration = 3;
    public float pauseDurationBetweenBurst = 2;

    public float targetAimTime = 1;
    public float targetFocusTime = 1;
    public float lookSpeed = 1;

    private void OnEnable()
    {
        Load();
    }

    public void Save()
    {
        string json = JsonUtility.ToJson(this);

        if (!Directory.Exists(Path.GetDirectoryName(CONFIG_DIR)))
            Directory.CreateDirectory(CONFIG_DIR);

        File.WriteAllText(CONFIG_PATH, json);
    }

    public bool Load()
    {
        bool success = false;

        if (File.Exists(CONFIG_PATH))
        {
            string json = File.ReadAllText(CONFIG_PATH);

            try
            {
                JsonUtility.FromJsonOverwrite(json, this);
            }
            catch
            { }

            success = true;
        }
        //else
        //{
        //    Debug.Log("File not found: " + CONFIG_PATH);
        //}

        return success;
    }
}