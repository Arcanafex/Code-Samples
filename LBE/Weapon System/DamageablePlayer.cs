using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class DamageablePlayer : Damageable, Damageable.IEnemyTargetable
{
    [Server]
    public override void TakeDamage(int damage)
    {
        totalDamageTaken += damage;

        if (!isClient)
            OnTakeDamage.Invoke();

        // TODO: Add reaction seen in avatar by other players

        base.RpcTakeDamage(damage);
    }
    

    public Transform GetHeadTransform()
    {
        var player = GetComponent<Player>();

        if (player && player.m_TrackedPlayer && player.m_TrackedPlayer.m_Head != null && player.m_TrackedPlayer.m_Head.m_TrackedObject != null)
        {
            return player.m_TrackedPlayer.m_Head.m_TrackedObject.transform;
        }
        else
        {
            return transform;
        }
    }


}
