using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
        MOVEAWAY,
        STUN,
    }

    //==============================================================
    [Header("General")]
    [SerializeField, Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
    PlayerAssignSettings m_PlayerAssignment = PlayerAssignSettings.MANUALLY;

    [SerializeField] GameObject m_PlayerTarget = null; // The player in the scene.
    NavMeshAgent m_EnemyAgent = null;

    //=====================================================
    [Space()]
    [SerializeField, Range(0, 10)] float m_StunnedTimer = 5;

    public float m_StunTimer = 0;
    bool m_IsStuned = false;
    [SerializeField] float m_Range;

    EnemyStates m_EnemyStates;
    Rigidbody m_Rigidbody = null;

    //==============================================================
    void Start()
    {
        m_EnemyAgent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_StunTimer = m_StunnedTimer;

        if (m_PlayerAssignment == PlayerAssignSettings.TAG)
        {
            GameObject Temp = GameObject.FindGameObjectWithTag("Player");
            m_PlayerTarget = Temp;
        }
        Stun();
    }

    void Update()
    {
        switch (m_EnemyStates)
        {
            case EnemyStates.MOVETOPLAYER:
                m_EnemyAgent.SetDestination(m_PlayerTarget.transform.position);
                break;

            case EnemyStates.STUN: //Stop attacking
                m_StunTimer -= Time.deltaTime;
                if (m_StunTimer <= 0)
                {
                    m_IsStuned = false;
                    m_StunTimer = m_StunnedTimer;
                    CheckDistanceToPlayer();
                }

                break;

            case EnemyStates.SHOOT: // shoot at the player

                break;

            case EnemyStates.MOVEAWAY:
                m_EnemyAgent.SetDestination(-m_PlayerTarget.transform.position);
                break;
        }
        CheckDistanceToPlayer();
    }

    void CheckDistanceToPlayer()
    {
        if (m_IsStuned) return;

        float DistanceBetween = Vector3.Distance(m_PlayerTarget.transform.position, transform.position);

        if (DistanceBetween > m_Range)
        {
            m_EnemyStates = EnemyStates.MOVETOPLAYER;
        }
        else
        {
            if (!m_EnemyAgent.isStopped)
            {
                m_EnemyAgent.Stop();
                m_EnemyAgent.transform.LookAt(m_PlayerTarget.transform.position, Vector3.up);
            }
        }
    }

    public void Stun()
    {
        if (!m_IsStuned)
        {
            m_IsStuned = true;
            m_EnemyAgent.Stop();
            m_StunTimer = m_StunnedTimer;
            m_EnemyStates = EnemyStates.STUN;
        }
    }
}
