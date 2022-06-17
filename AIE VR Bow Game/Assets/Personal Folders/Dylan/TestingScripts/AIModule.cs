using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Obsolete]
public class AIModule : MonoBehaviour
{
    enum PlayerAssignSettings
    {
        MANUALLY = 0,
        TAG,
    }

    enum EnemyStates
    {
        SHOOT = 0,
        MOVETOPLAYER,
        RETREAT,
        STUN,
        DEATH,
    }

    //==============================================================
    [Header("General")]
    [SerializeField, Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
    PlayerAssignSettings m_PlayerAssignment = PlayerAssignSettings.MANUALLY;

    [SerializeField] GameObject m_PlayerTarget = null; // The player in the scene.

    [Header("AI Events")]
    [SerializeField] UnityEvent m_OnEnterStun;
    [SerializeField] UnityEvent m_OnExitStun;

    [Space()]

    [SerializeField] UnityEvent m_OnMove;
    [SerializeField] UnityEvent m_OnDeath;

    [SerializeField, Range(10, 50)] float m_StoppingDistance = 10;
    [SerializeField] float m_FleeDistance = 5;

    //=====================================================
    [Space()]
    [SerializeField, Range(0, 10)] float m_StunTime = 5;

    float m_StunTimer = 0;
    bool m_IsStuned = false;

    EnemyStates m_EnemyStates;
    Rigidbody m_Rigidbody = null;
    NavMeshAgent m_EnemyAgent = null;

    bool m_IsAlive = true;

    //==============================================================
    void Start()
    {
        m_EnemyAgent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_StunTimer = m_StunTime;

        if (m_PlayerAssignment == PlayerAssignSettings.TAG)
        {
            GameObject Temp = GameObject.FindGameObjectWithTag("Player");
            m_PlayerTarget = Temp;
        }

        m_FleeDistance = m_StoppingDistance - 5;
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

            case EnemyStates.RETREAT:

                //TODO flee to a position 
                break;

            case EnemyStates.DEATH:

                //TODO do stuff when agent dies
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
        }
        else if(DistanceBetween < m_StoppingDistance && DistanceBetween > m_FleeDistance)
        {
            if (!m_EnemyAgent.isStopped)
            {
                Debug.Log("Too Close!");

                m_EnemyAgent.Stop();
                m_EnemyAgent.transform.LookAt(m_PlayerTarget.transform.position, Vector3.up);
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

            m_EnemyAgent.Stop();
            m_StunTimer = m_StunTime;
            m_EnemyStates = EnemyStates.STUN;
        }
    }

    private void Move()
    {
        if (!m_IsStuned && m_EnemyStates != EnemyStates.MOVETOPLAYER && m_IsAlive)
        {
            if (m_OnMove != null)
            {
                m_OnMove.Invoke();
            }

            m_EnemyStates = EnemyStates.MOVETOPLAYER;
        }
    }

    private void Flee()
    {
        if (m_EnemyStates != EnemyStates.RETREAT)
        {
            //  TODO change state to flee
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

    public bool GetAliveState()
    {
        return m_IsAlive;
    }
}
