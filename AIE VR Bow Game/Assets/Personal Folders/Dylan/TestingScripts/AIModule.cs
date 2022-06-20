using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[DisallowMultipleComponent]
[RequireComponent(typeof(NavMeshAgent))]
public class AIModule : MonoBehaviour
{
    enum PlayerAssignSettings
    {
        MANUALLY = 0,
        TAG,
    }

    enum EnemyStates
    {
        MOVETOPLAYER = 0,
        SHOOT,
        FLEE,
        STUN,
        DEATH,
    }

    //==============================================================
    [Header("General")]
    GameObject m_PlayerTarget = null; // The player in the scene.

    [Header("AI Events")]
    [SerializeField, Tooltip("Is called when the stun state starts.")] UnityEvent m_OnEnterStun;
    [SerializeField, Tooltip("Is called when the stun state ends.")] UnityEvent m_OnExitStun;

    [Space()]

    [SerializeField, Tooltip("Is called when the move state starts.")] UnityEvent m_OnMove;
    [SerializeField, Tooltip("Is called when the AI dies.")] UnityEvent m_OnDeath;

    public float m_StoppingDistance = 10;
    public float m_FleeDistance = 5;

    //=====================================================
    float m_StunTime = 5;

    Transform m_RetreatZone = null;
    float m_StunTimer = 0;
    bool m_IsStuned = false;

    EnemyStates m_EnemyStates;
    Rigidbody m_Rigidbody = null;
    NavMeshAgent m_EnemyAgent = null;

    bool m_IsAlive = true;
    public bool m_IsFleeing = false;

    //==============================================================
    void Start()
    {
        m_EnemyAgent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_StunTimer = m_StunTime;
        m_FleeDistance = m_StoppingDistance - 5;

        GameObject Zone = GameObject.FindGameObjectWithTag("RetreatZone");
        m_RetreatZone = Zone.transform;

        if (m_RetreatZone == null)
        {
            Debug.LogError("No Retreat Zone Found!");
        }

        if (gameObject.GetComponent<NavMeshAgent>())
        {
            gameObject.GetComponent<NavMeshAgent>().stoppingDistance = 0;
        }
    }

    void Update()
    {
        switch (m_EnemyStates)
        {
            case EnemyStates.MOVETOPLAYER:
                m_EnemyAgent.SetDestination(m_PlayerTarget.transform.position);
                break;

            case EnemyStates.STUN:

                m_StunTimer -= Time.deltaTime;
                if (m_StunTimer <= 0)
                {
                    m_IsStuned = false;
                    m_StunTimer = m_StunTime;

                    if (m_OnExitStun != null)
                    {
                        m_OnExitStun.Invoke();
                    }
                }

                break;

            case EnemyStates.SHOOT:

                //TODO fire weapons at player
                break;

            case EnemyStates.FLEE:

                Transform startTransform = transform;

                transform.rotation = Quaternion.LookRotation(transform.position - m_PlayerTarget.transform.position);

                Vector3 FleePosition = transform.position + transform.forward * 20;

                NavMeshHit hit;

                NavMesh.SamplePosition(FleePosition, out hit, 5, 1 << NavMesh.GetAreaFromName("Walkable"));

                transform.position = startTransform.position;
                transform.rotation = startTransform.rotation;

                m_EnemyAgent.SetDestination(hit.position);

                break;

            case EnemyStates.DEATH:

                gameObject.GetComponent<AIModule>().enabled = false;
                break;
        }
        CheckDistanceToPlayer();
    }

    void CheckDistanceToPlayer()
    {
        if (m_IsStuned || !m_IsAlive)
        {
            return;
        }

        float DistanceBetween = Vector3.Distance(m_PlayerTarget.transform.position, transform.position);

        if (DistanceBetween > m_StoppingDistance)
        {
            Move();
            if (m_IsFleeing)
            {
                m_IsFleeing = false;
            }
        }
        else if (DistanceBetween < m_StoppingDistance && DistanceBetween > m_FleeDistance && !m_IsFleeing)
        {
            m_EnemyAgent.transform.LookAt(m_PlayerTarget.transform.position, Vector3.up);

            if (!m_EnemyAgent.isStopped)
            {
                m_EnemyAgent.isStopped = true;
            }
        }
        else if (DistanceBetween < m_FleeDistance)
        {
            Flee();
        }
    }

    //==================================================
    // Stun the Enemy for a short period 
    public void Stun()
    {
        if (!m_IsStuned && m_IsAlive)
        {
            m_IsStuned = true;
            if (m_OnEnterStun != null)
            {
                m_OnEnterStun.Invoke();
            }

            m_EnemyAgent.isStopped = true;
            m_StunTimer = m_StunTime;
            m_EnemyStates = EnemyStates.STUN;
        }
    }

    private void Move()
    {
        if (!m_IsStuned && m_IsAlive)
        {
            if (m_OnMove != null)
            {
                m_EnemyAgent.isStopped = false;
                m_OnMove.Invoke();
            }

            m_EnemyStates = EnemyStates.MOVETOPLAYER;
        }
    }

    private void Flee()
    {
        if (m_EnemyStates != EnemyStates.FLEE && !m_IsStuned)
        {
            m_EnemyStates = EnemyStates.FLEE;
            m_EnemyAgent.isStopped = false;

            m_IsFleeing = true;
        }
    }

    private void Shoot()
    {
        //TODO change state to shoot
    }

    public void Kill()
    {
        m_OnDeath.Invoke();
        m_IsAlive = false;
        m_EnemyStates = EnemyStates.DEATH;
    }

    public bool IsDead()
    {
        return m_IsAlive;
    }

    public void SetStopDistance(float _amount)
    {
        m_StoppingDistance = _amount;
        m_FleeDistance = m_StoppingDistance - 5;
    }

    public void SetPlayerTarget(GameObject _player)
    {
        m_PlayerTarget = _player;
    }

    public void SetStunTimer(float _amount)
    {
        m_StunTime = _amount;
    }

    public void SetPriority(int _index)
    {
        m_EnemyAgent.avoidancePriority = _index;
    }

    public void SetAvoidanceRadius(float _amount    )
    {
        m_EnemyAgent.radius = _amount;
    }

    public void SetSpeed(float _amount)
    {
        m_EnemyAgent.speed = _amount;
    }
}
