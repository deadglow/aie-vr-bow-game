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
        RETREAT,
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
    float m_StoppingDistance = 10;
    float m_FleeDistance = 5;
    float m_StunDistance = 2;
    float m_StunTime = 5;

    //Transform m_RetreatZone = null;
    float m_StunTimer = 0;
    public bool m_IsStuned = false;

    EnemyStates m_EnemyStates;
    Rigidbody m_Rigidbody = null;
    NavMeshAgent m_EnemyAgent = null;

    bool m_IsAlive = true;
    bool m_IsFleeing = false;
    public bool m_StunCooldown = false;

    public float m_StunCooldownAmount;
    public float m_StunCooldownTimer;


    //==============================================================

	void Awake()
	{
        m_EnemyAgent = GetComponent<NavMeshAgent>();
        m_Rigidbody = GetComponent<Rigidbody>();

	}
    void Start()
    {

        m_StunTimer = m_StunTime;
        m_FleeDistance = m_StoppingDistance - 5;
        m_StunDistance = m_FleeDistance - 4;

        //GameObject Zone = GameObject.FindGameObjectWithTag("RetreatZone");
        //m_RetreatZone = Zone.transform;

        //if (m_RetreatZone == null)
        //{
        //    Debug.LogError("No Retreat Zone Found!");
        //}

        if (m_EnemyAgent)
        {
            m_EnemyAgent.stoppingDistance = 0;
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
                    Flee();
                    m_StunCooldown = true;
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
                gameObject.GetComponent<NavMeshAgent>().enabled = false;
                break;

            case EnemyStates.RETREAT: // SUBJECT TO CHANGE
                //retreat to certain zone.

                break;
        }
        CheckDistanceToPlayer();
        UpdateStunTimer();
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
            m_EnemyAgent.transform.LookAt(m_PlayerTarget.transform.position);

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
        m_FleeDistance = m_StoppingDistance - 5;
        m_StunDistance = m_FleeDistance - 4;
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
        m_StunCooldownAmount = _amount;
        m_StunCooldownTimer = m_StunCooldownAmount;
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
}