using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using RootMotion.FinalIK;
using UnityEngine.Events;
using UnityEngine.Networking;

//*****************************************************************************************************************************
//*****************************************************************************************************************************
public class AITargetingBehaviour : NetworkBehaviour {

    public static Queue<AITargetingBehaviour> s_shootingQueue;

    public TargetingBehaviourParameters parameters;
    public Damageable currentTarget { get; private set; }

    protected float m_fCurrentTargetTimeStamp;
    public float CurrentTargetTimeStamp {
        get { return m_fCurrentTargetTimeStamp; }
        set { m_fCurrentTargetTimeStamp = value; }
    }

    private List<Damageable> availableTargets;
    public List<Damageable> AvailableTargets {
        get {
            if (availableTargets == null)
                availableTargets = new List<Damageable>();

            return availableTargets;
        }
    }


    public GunSystem gunSystem { get; set; }
    public bool isFiring { get; set; }
    public bool inFiringLoop { get; set; }

    [SyncVar]
    private Vector3 aimPoint;
    [SyncVar]
    private Vector3 lookPoint;

    private float aimWeight;
    private float elapsedAimTime;

    public Transform aimTarget { get; private set; }
    public Transform lookTarget { get; private set; }

    private float timeToReacquireTargets { get; set; }

    public Firing OnFiring;

    private bool WaitingForTurnToFire {
        get {
            if (s_shootingQueue == null)
                s_shootingQueue = new Queue<AITargetingBehaviour>();

            return s_shootingQueue.Contains(this);
        }
    }

    private bool IsNextInQueue {
        get {
            while (s_shootingQueue.Peek() == null)
                s_shootingQueue.Dequeue();
            return s_shootingQueue.Peek() == this;
        }
    }

    private int CurrentShooterCount {
        get {
            int shooters = FindObjectsOfType<AITargetingBehaviour>().Count(t => t.isFiring);
            return shooters;
        }
    }

    public AimIK aimIK { get; private set; }
    public LookAtIK lookIK { get; private set; }


    //-------------------------------------------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------------------------------------------
    private void Awake() {
        if (OnFiring == null)
            OnFiring = new Firing();
    }


    //-------------------------------------------------------------------------------------------------------------------------
    private void Start() {
        if (aimIK = GetComponent<AimIK>()) {
            aimTarget = new GameObject("[" + this.name + "] Aim Target").transform;
            aimTarget.position = aimPoint = transform.forward * parameters.targetingRange;
            aimIK.solver.target = aimTarget;
            aimIK.solver.IKPositionWeight = aimWeight = 0;
        }

        if (lookIK = GetComponent<LookAtIK>()) {
            lookTarget = new GameObject("[" + this.name + "] Look Target").transform;
            lookTarget.position = lookPoint = transform.forward * parameters.targetingRange;
            lookIK.solver.target = lookTarget;
            lookIK.solver.IKPositionWeight = 1;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private void OnDestroy() {
        if (aimTarget) {
            Destroy(aimTarget.gameObject);
            aimTarget = null;
        }

        if (lookTarget) {
            Destroy(lookTarget.gameObject);
            lookTarget = null;
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private void Update() {
        if (isServer) {
            timeToReacquireTargets -= Time.deltaTime;

            if (timeToReacquireTargets < 0) {
                timeToReacquireTargets = parameters.acquireTargetsFrequency;
                AcquireTargets();
            }

            LookAtTarget();
        }

        if (aimTarget)
            aimTarget.position = aimPoint;

        if (lookTarget)
            lookTarget.position = lookPoint;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public void StartShootingLoop() {
        StartCoroutine(ShootingCoroutine());
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [Server]
    public void StopShootingLoop() {
        gunSystem.ToggleTrigger(false);
        StopCoroutine(ShootingCoroutine());
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private bool Fire(int shots) {
        return shots > 0;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private IEnumerator ShootingCoroutine() {
        inFiringLoop = true;

        if (!isFiring && !WaitingForTurnToFire)
            JoinShootingQueue();

        while (WaitingForTurnToFire) {
            yield return new WaitForSeconds(parameters.waitToFireCheckFrequency);

            if (CurrentShooterCount < parameters.maxConcurrantShooters && IsNextInQueue) {
                s_shootingQueue.Dequeue();
            }
        }

        // Taking aim
        int shotsToFire = gunSystem.FiredCount + SelectBurstSize();        
        isFiring = true;
        RpcUpdateAimWeight(true);

        yield return UpdateAimWeight(true);

        // Holding firing stance

        float firingDelay = parameters.pauseDurationBetweenBurst;
        float elapsedTime = 0;

        while (elapsedTime < parameters.firingDuration || isFiring) {
            elapsedTime += Time.deltaTime;

            while (isFiring) {
                elapsedTime += Time.deltaTime;
                gunSystem.TriggerDown = isFiring = Fire(shotsToFire - gunSystem.FiredCount);
                //Spaces.LBE.DebugLog.Log("AI", transform.gameObject.name + " elapsedTime: " + elapsedTime.ToString() + " DeltaTime: " + Time.deltaTime);
                yield return null;
            }

            firingDelay -= Time.deltaTime;

            if (firingDelay <= 0) {
                isFiring = true;
                firingDelay = parameters.pauseDurationBetweenBurst;
                shotsToFire = gunSystem.FiredCount + SelectBurstSize();
            }

            yield return null;
        }

        // At ease
        RpcUpdateAimWeight(false);
        yield return UpdateAimWeight(false);

        aimWeight = 0;
        inFiringLoop = false;
        currentTarget = null;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public IEnumerator UpdateAimWeight(bool aiming) {

        float elapsedTime = 0;
        float start = aiming ? 0 : 1;
        float end = aiming ? 1 : 0;

        while (elapsedTime < parameters.targetAimTime)
        {
            elapsedTime += Time.deltaTime;
            aimWeight = Mathf.Lerp(start, end, elapsedTime / parameters.targetAimTime);

            if (aimIK)
            {
                aimIK.solver.IKPositionWeight = aimWeight;
            }

            OnFiring.Invoke(aimWeight);

            yield return null;
        }
    }

    [ClientRpc]
    private void RpcUpdateAimWeight(bool aim)
    {
        StartCoroutine(UpdateAimWeight(aim));
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public void AcquireTargets() {
        if (availableTargets == null)
            availableTargets = new List<Damageable>();
        else
            availableTargets.Clear();

        var targets = Physics.OverlapSphere(transform.position, parameters.targetingRange);

        foreach (var target in targets) {
            var damageable = target.GetComponentInParent<Damageable>();

            if (damageable && damageable is Damageable.IEnemyTargetable && damageable.Alive) {
                if (!availableTargets.Contains(damageable))
                    availableTargets.Add(damageable);
            }
        }
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public Damageable SelectTarget(bool los = true) {
        Damageable target = null;

        if (availableTargets != null && availableTargets.Count > 0) {
            if (parameters.randomTargetSelection) {
                do
                    target = availableTargets[Random.Range(0, availableTargets.Count)];
                while (TargetPopularity(target) > parameters.maxTargetPopularity);
            } else {
                List<Damageable> targets = availableTargets.Where(t => t != null).OrderBy(t => t.AttackerNum).ToList<Damageable>();
                if (los) {
                    RaycastHit hit;
                    Vector3 startP;
                    Vector3 endP;
                    Vector3 dir;
                    float rayTestLength = 0.0f;
                    //Animator animator;
                    //Transform headT;
                    foreach (Damageable candidate in targets) {
                        //animator = GetComponent<Animator>();
                        //headT = animator.GetBoneTransform(HumanBodyBones.Head);
                        startP = transform.position;
                        startP.y += 1.2f; // add the AI height
                        startP += transform.forward * 0.1f;

                        endP = ((Damageable.IEnemyTargetable)candidate).GetHeadTransform().position;
                        dir = endP - startP;
                        rayTestLength = dir.magnitude - 0.2f;
                        // FENG_TODO: Comment it out before checking in
                        // Debugging lines
                        Debug.DrawLine(startP, endP, Color.red, 3);
                        if (!Physics.Raycast(startP, dir, out hit, rayTestLength)) {
                            target = candidate;
                            Debug.DrawLine(startP, endP, Color.green, 3);
                            break;
                        } else
                            Debug.DrawLine(startP, endP, Color.red, 3);
                    }
                } else {
                    if (targets.Count > 0)
                        target = targets[0];
                }
            }
        }
        return target;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public void SetTarget(Damageable target) {
        currentTarget = target;
        CurrentTargetTimeStamp = Time.time; // recording the target timestamp for making AI moves more 

        if (currentTarget) {
            aimPoint = ((Damageable.IEnemyTargetable)currentTarget).GetHeadTransform().position + (Vector3.down * 0.3f) + (Random.insideUnitSphere * parameters.aimVariance);
            currentTarget.AttackerNum += 1;
        } else {
            aimPoint = transform.forward * parameters.targetingRange;
        }

        RpcSetTarget(target ? target.gameObject : null);
    }

    //-------------------------------------------------------------------------------------------------------------------------
    [ClientRpc]
    private void RpcSetTarget(GameObject target) {
        currentTarget = target ? target.GetComponent<Damageable>() : null;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private void LookAtTarget() {
        if (lookIK && currentTarget) {
            lookPoint = Vector3.Lerp(lookPoint, currentTarget.transform.position, Time.deltaTime * parameters.lookSpeed);
        }
    }
    
    //-------------------------------------------------------------------------------------------------------------------------
    public int SelectBurstSize() {
        if (parameters.burstSizes != null && parameters.burstSizes.Length > 0)
            return parameters.burstSizes[Random.Range(0, parameters.burstSizes.Length)];
        else
            return 0;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private int TargetPopularity(Damageable target) {
        return target ? FindObjectsOfType<AITargetingBehaviour>().Count(AI => AI.currentTarget == target) : parameters.maxTargetPopularity + 1;
    }

    //-------------------------------------------------------------------------------------------------------------------------
    private void JoinShootingQueue() {
        if (s_shootingQueue == null)
            s_shootingQueue = new Queue<AITargetingBehaviour>();

        s_shootingQueue.Enqueue(this);
    }

    //-------------------------------------------------------------------------------------------------------------------------
    public class Firing : UnityEvent<float> {

    }

    //-------------------------------------------------------------------------------------------------------------------------
}