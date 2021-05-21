using ReboundCG.Tennis.Simulation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TMPC
{
    public class CrowdManager : MonoBehaviour, IMatchEventsListener
    {
        public CrowdSection[] Sections;
        public CrowdMember[] CrowdPrefabs;
        [Range(0f, 1f)]
        public float CrowdSize = 1f;
        public FloatRange CrowdEnthusiasm = new FloatRange(0, 1);

        public float SecBeforeSleep = 10f;
        public Vector2 CullingMargin;


        private List<Animator> m_crowd { get; set; } = new List<Animator>();
        private MatchSimulationManager m_matchSimulationManager;

        private float m_sleepTimer;

        private void LateUpdate()
        {
            if (m_sleepTimer > 0)
            {
                m_sleepTimer -= Time.deltaTime;

                if (m_sleepTimer <= 0)
                {
                    var activeAnims = m_crowd.Where(member => member.gameObject.activeSelf);
                    bool notAllIdle = false;

                    foreach (var anim in activeAnims)
                    {
                        var info = anim.GetCurrentAnimatorStateInfo(0);

                        if (info.IsName("Idle"))
                            anim.enabled = false;
                        else
                            notAllIdle = true;
                    }

                    if (notAllIdle)
                    {
                        m_sleepTimer = SecBeforeSleep;
                    }
                    else
                    {
                        Debug.Log($"{this}.CrowdManager - Crowd Animation Going Idle");
                    }
                }
            }
        }
        
        //// For Debug purposes only
        //private void OnGUI()
        //{
        //    if (GUI.Button(new Rect(25, 400, 100, 30), "Toggle"))
        //    {
        //        ToggleAllAnimators();
        //    }

        //    //if (GUI.Button(new Rect(25, 25, 100, 30), "Spawn"))
        //    //{
        //    //    SpawnCrowd(CrowdSize);
        //    //}

        //    //if (GUI.Button(new Rect(25, 55, 100, 30), "Clap"))
        //    //{
        //    //    Clap();
        //    //}

        //    //if (GUI.Button(new Rect(25, 85, 100, 30), "Standing Clap"))
        //    //{
        //    //    StandingClap();
        //    //}
        //}

        public void ClearCrowd()
        {
            foreach (var person in m_crowd)
            {
                person.gameObject.SetActive(false);
                Destroy(person);
            }

            m_crowd.Clear();
        }

        //private void ToggleAllAnimators(bool active)
        //{
        //    if (active)
        //        m_crowd.ForEach(anim => anim.enabled = true);
        //    else
        //        m_crowd.Where(member => member.gameObject.activeInHierarchy).ToList().ForEach(anim => anim.enabled = false);
        //}

        private IEnumerator EnableAnimator(Animator animator, string lateTrigger = null)
        {
            var delay = animator.GetFloat("Offset");
            yield return new WaitForSeconds(delay);
            animator.enabled = true;

            if (!string.IsNullOrEmpty(lateTrigger))
            {
                yield return new WaitForSeconds(delay);
                animator.SetTrigger(lateTrigger);
            }
        }

        public void Clap(bool player1 = true)
        {
            var threshold = Random.value;

            foreach (var anim in m_crowd.Where(member => member.gameObject.activeInHierarchy))
            {
                var enthusiasm = anim.GetFloat("Enthusiasm");
                bool isFan = player1 ? anim.GetBool("Player1 Fan") : anim.GetBool("Player2 Fan");

                if (isFan)
                {
                    if (enthusiasm > threshold)
                    {
                        if ((enthusiasm - threshold) > 0.9)
                            anim.SetTrigger("Standing Clap");
                        else
                            anim.SetTrigger("Clap");

                        if ((enthusiasm - threshold) > 0.75)
                            StartCoroutine(EnableAnimator(anim, "Standing Clap"));
                        else
                            StartCoroutine(EnableAnimator(anim));
                    }
                    else
                    {
                        if ((threshold - enthusiasm) < 0.75)
                            StartCoroutine(EnableAnimator(anim, "Clap"));
                    }
                }
                else
                {
                    if (enthusiasm > threshold && (enthusiasm - threshold) > 0.75)
                    {
                        anim.SetTrigger("Clap");

                        if ((enthusiasm - threshold) > 0.9)
                            StartCoroutine(EnableAnimator(anim, "Standing Clap"));
                        else
                            StartCoroutine(EnableAnimator(anim));
                    }
                }
            }

            m_sleepTimer = SecBeforeSleep;
        }

        public void StandingClap(bool player1 = true)
        {
            var threshold = Random.value;

            foreach (var anim in m_crowd.Where(member => member.gameObject.activeInHierarchy))
            {
                var enthusiasm = anim.GetFloat("Enthusiasm");
                bool isFan = player1 ? anim.GetBool("Player1 Fan") : anim.GetBool("Player2 Fan");

                if (isFan)
                {
                    if (enthusiasm > threshold)
                    {
                        anim.SetTrigger("Standing Clap");
                        StartCoroutine(EnableAnimator(anim));
                    }
                    else
                    {
                        anim.SetTrigger("Clap");

                        if ((enthusiasm - threshold) < 0.25)
                            StartCoroutine(EnableAnimator(anim, "Standing Clap"));
                        else
                            StartCoroutine(EnableAnimator(anim));
                    }
                }
                else
                {
                    if (enthusiasm > threshold && (enthusiasm - threshold) > 0.75)
                    {
                        anim.SetTrigger("Clap");

                        if ((enthusiasm - threshold) > 0.9)
                            StartCoroutine(EnableAnimator(anim, "Standing Clap"));
                        else
                            StartCoroutine(EnableAnimator(anim));
                    }
                }
            }

            m_sleepTimer = SecBeforeSleep;
        }

        public void SpawnCrowd(float fill)
        {
            if (m_crowd.Count > 0)
                ClearCrowd();

            if (CrowdPrefabs.Length > 0)
            {
                foreach (var section in Sections)
                {
                    var queue = new Queue<Transform>();

                    if (section.isActiveAndEnabled)
                    {
                        queue.Clear();

                        foreach (var spot in section.Spots)
                        {
                            if (Random.value < fill)
                            {
                                var crowdMember = CrowdPrefabs[Random.Range(0, CrowdPrefabs.Length)].Spawn(section.transform, spot);
                                queue.Enqueue(crowdMember.transform);

                                while (queue.Count > 0)
                                {
                                    var current = queue.Dequeue();
                                    current.gameObject.layer = section.gameObject.layer;

                                    for (int i = 0; i < current.childCount; i++)
                                    {
                                        queue.Enqueue(current.GetChild(i));
                                    }
                                }

                                crowdMember.layer = section.gameObject.layer;

                                var animator = crowdMember.GetComponent<Animator>();

                                if (animator)
                                {
                                    animator.keepAnimatorControllerStateOnDisable = true;
                                    animator.SetBool("Player1 Fan", Random.value >= 0.5f);
                                    animator.SetBool("Player2 Fan", Random.value <= 0.5f);
                                    animator.SetFloat("Enthusiasm", Random.Range(CrowdEnthusiasm.Min, CrowdEnthusiasm.Max));
                                    m_crowd.Add(animator);
                                }
                                else
                                {
                                    Debug.LogError($"[{this.name}.CrowdManager - prefab instance {crowdMember.name} has no Animator component!");
                                }
                            }
                        }
                    }
                }
            }

            StartCoroutine(IsCrowdVisible());
            m_sleepTimer = SecBeforeSleep;
        }

        public void SetMatchSimulationManager(MatchSimulationManager matchSimulationManager)
        {
            if (matchSimulationManager != null)
            {
                m_matchSimulationManager = matchSimulationManager;
            }

            m_matchSimulationManager?.Attach(this);
        }

        public void OnMatchEvent(EventMatch matchEvent)
        {
            switch(matchEvent.MatchEventType)
            {
                case EventMatchType.BreakGameWon:
                case EventMatchType.LoveGameWon:
                case EventMatchType.GameWon:
                    StandingClap(matchEvent.PlayerType == MatchPlayerType.PLAYER1);
                    break;
                case EventMatchType.DebreakPointWon:
                case EventMatchType.SetWon:
                    Clap(matchEvent.PlayerType == MatchPlayerType.PLAYER1);
                    break;
                default:
                    break;
            }
        }

        public IEnumerator IsCrowdVisible()
        {
            var cam = Camera.main;

            while (m_crowd.Count > 0)
            {
                foreach(var member in m_crowd)
                {
                    var screenPos = cam.ScreenToViewportPoint(cam.WorldToScreenPoint(member.transform.position));
                    bool visible = screenPos.z > 0 && screenPos.x >= (0 - CullingMargin.x) && screenPos.x <= (1 + CullingMargin.x) && screenPos.y >= (0 - CullingMargin.y) && screenPos.y <= (1 + CullingMargin.y);

                    member.gameObject.SetActive(visible);
                }

                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
