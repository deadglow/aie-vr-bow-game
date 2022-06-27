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

    //===================================================== Dont touch
    public float m_StoppingDistance = 0;
    public float m_FleeDistance = 0;
    public float m_StunDistance = 0;
    float m_StunTime = 5;

    //Transform m_RetreatZone = null;
    float m_StunTimer = 0;
    public bool m_IsStuned = false;

    EnemyStates m_EnemyStates;
    Rigidbody m_Rigidbody = null;
    NavMeshAgent m_EnemyAgent = null;

    bool m_IsAlive = true;
    bool m_IsFleeing = false;
    bool m_StunCooldown = false;

    float m_StunCooldownAmount;
    float m_StunCooldownTimer;
    bool m_CheckerVersion = true;

    float m_ShootCooldownAmount = 5f;
    float m_ShootCooldown = 0f;
    bool m_IsShooting = false;



    //==============================================================
    void Awake()
    {
        m_EnemyAgent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

        m_StunTimer = m_StunTime;

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
                    m_StunCooldown = true;
                    Flee();
                }

                break;

            case EnemyStates.SHOOT:

                m_ShootCooldown -= Time.deltaTime;

                if (m_ShootCooldown <= 0)
                {
                    FireAtPlayer();
                    m_ShootCooldown = m_ShootCooldownAmount;
                }

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
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
                break;
        }

        CheckDistanceToPlayer();
        UpdateStunTimer();
    }

    void FireAtPlayer()
    {
        Debug.Log(gameObject.name + " Fired at player");
    }

    void UpdateStunTimer()
    {
        if (m_StunCooldown)
        {
            m_StunCooldownTimer -= Time.deltaTime;
            if (m_StunCooldownTimer <= 0)
            {
                m_StunCooldown = false;
                m_StunCooldownTimer = m_StunCooldownAmount;
            }
        }
        else
        {
            return;
        }
    }

    void CheckDistanceToPlayer()
    {
        if (m_IsStuned || !m_IsAlive)
        {
            return;
        }

        float DistanceBetween = Vector3.Distance(m_PlayerTarget.transform.position, transform.position);

        if (DistanceBetween > m_StoppingDistance) // checks the distance between the target & AI.
        {
            Move();
            if (m_IsFleeing)
            {
                m_IsFleeing = false;
            }
        }
        else if (DistanceBetween < m_StoppingDistance && DistanceBetween > m_FleeDistance && !m_IsFleeing) // Stops the player & should change to the shoot state.
        {
            Vector3 Direction = m_PlayerTarget.transform.position - transform.position;
            Direction.y = 0;
            Direction = Direction.normalized;

            m_EnemyAgent.transform.rotation = Quaternion.LookRotation(Direction, Vector2.up);

            if (!m_EnemyAgent.isStopped)
            {
                Shoot();
                m_EnemyAgent.isStopped = true;
            }
        }
        else if (DistanceBetween < m_FleeDistance && DistanceBetween > m_StunDistance) // checks the flee distance.
        {
            Flee();
        }
        else if (DistanceBetween < m_StunDistance && !m_IsStuned && !m_StunCooldown)
        {
            Stun();
        }
    }

    //==================================================
    // Stun the Enemy for a short period 
    public void Stun() // puts the AI into the stun state.
    {
        if (m_StunCooldown) return;

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

    private void Move() // Set the Ai state to find the target.
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

    private void Flee() //changes the Ai to the flee state & runs from the target.
    {
        if (m_EnemyStates != EnemyStates.FLEE && !m_IsStuned)
        {
            m_EnemyStates = EnemyStates.FLEE;
            m_EnemyAgent.isStopped = false;

            m_IsFleeing = true;
        }
    }

    private void Shoot() // TODO change to the shoot state.
    {
        if (!m_IsStuned && m_EnemyStates != EnemyStates.SHOOT)
        {

            m_EnemyStates = EnemyStates.SHOOT;
        }
    }

    //========================================
    public void Kill() // Kills the AI.
    {
        m_OnDeath.Invoke();
        m_IsAlive = false;
        m_EnemyStates = EnemyStates.DEATH;

        gameObject.GetComponent<NavMeshAgent>().enabled = false;
    }

    //========================================
    public bool IsDead() // return whether the Ai is dead or alive.
    {
        return m_IsAlive;
    }

    public void SetStopDistance(float _amount) // the distance the Ai will stay from the target.
    {
        m_StoppingDistance = _amount;
        SortChecker();
    }
    public void SetPlayerTarget(GameObject _player) // Set what the Ai should attack/ follow.
    {
        m_PlayerTarget = _player;
    }

    public void SetStunTimer(float _amount) // Sets how long the stun timer will last for.
    {
        m_StunTime = _amount;
    }

    public void SetPriority(int _index) // the priority importance in avoiding other AIs.
    {
        m_EnemyAgent.avoidancePriority = _index;
    }

    public void SetAvoidanceRadius(float _amount) // The distance for the Ai to avoid.
    {
        m_EnemyAgent.radius = _amount;
    }

    public void SetSpeed(float _amount) // set the speed of the Ai.
    {
        m_EnemyAgent.speed = _amount;
    }

    public void Revive(Transform _positon) //revive the AI.
    {
        m_IsAlive = true;
        transform.position = _positon.position;
    }

    public void SetStunCooldown(float _amount)
    {
        Debug.Log("Stun Cooldown passed");
        m_StunCooldownAmount = _amount;
        m_StunCooldownTimer = m_StunCooldownAmount;
        Debug.Log(m_StunCooldownAmount);
    }

    public void SetAngularDistance(float _amount)
    {
        m_EnemyAgent.angularSpeed = _amount;
    }

    public void AvoidanceHeight(float _amount)
    {
        m_EnemyAgent.height = _amount;
    }

    public void ProtectedAtStart(bool _state)
    {
        m_StunCooldown = _state;
    }
    public void SetCheckerVersion(bool _state)
    {
        m_CheckerVersion = _state;
        SortChecker();
    }

    void SortChecker()
    {
        if (m_CheckerVersion)
        {
            m_FleeDistance = m_StoppingDistance / 2;
            m_StunDistance = m_FleeDistance / 2;
        }
        else
        {
            m_FleeDistance = m_StoppingDistance - 5;
            m_StunDistance = m_FleeDistance - 4;
        }
    }

    public void SetAcceleration(float _amount)
    {
        m_EnemyAgent.acceleration = _amount;
    }

    public void SetShootCooldown(float _amount)
    {
        m_ShootCooldownAmount = _amount;
    }
}