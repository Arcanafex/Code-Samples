using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkTonic.MasterAudio.Multiplayer;
using DarkTonic.MasterAudio;

public class BoomFX : MonoBehaviour
{
    public string DeathParticleName;

    ServerSoundEffectTrigger soundEffectComponent;

    private void Start() {
        soundEffectComponent = gameObject.GetComponent<ServerSoundEffectTrigger>();
    }

    public void GoBoom()
    {
        Spaces.LBE.EventParam heatLampEventParams = new Spaces.LBE.EventParam();
        heatLampEventParams.m_fParam = 1.0f;
        Spaces.LBE.SpacesEventManager.TriggerEvent(Spaces.LBE.SpacesEventType.HeatLamps, heatLampEventParams);

        if (PlayEnemyDeathParticle.instance) {
            PlayEnemyDeathParticle.instance.PlayDeathParticle(DeathParticleName, gameObject.transform);
            if (MasterAudio.Instance) {
                MasterAudio.PlaySound3DAtTransformAndForget("ExplosionGroup", transform);
            }

            if (soundEffectComponent) {
                soundEffectComponent.PlaySoundToServer();
            }
        }
    }
}
