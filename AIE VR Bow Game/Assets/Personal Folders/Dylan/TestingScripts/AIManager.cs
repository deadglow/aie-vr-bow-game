using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIManager : MonoBehaviour
{
    enum AIsearchMode
    {
        MANUALLY = 0,
        TAG,
    }

    enum ApplyHeight
    {
        NONE = 0,
        ALL,
    }

    enum CheckerEdition
    {
        OLD = 0,
        NEW,
    }

    enum ProtectStart
    {
        ON = 0,
        OFF,
    }

    [Header("Player Settings")]

    [SerializeField, Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
    AIsearchMode m_PlayerAssignment = AIsearchMode.MANUALLY;
    [SerializeField] GameObject m_Target = null;

    //============================================================
    [Space()]
    [Header("AI Settings")] //==================================================  Main

    [Tooltip("The checker is responsible for calculating the flee & stun distances from stopping distance.")]
    [SerializeField] CheckerEdition m_CheckerVersion = CheckerEdition.NEW;

    [Tooltip("The speed the AI will move at.")]
    [SerializeField, Range(0.5f, 5)] float m_AiSpeed = 2f;

    [Tooltip("How far the AI will stay away from the target.")]
    [SerializeField, Range(10, 25)] float m_StoppingDistance = 20;

    [Tooltip("The max turning speed when following a path.")]
    [SerializeField, Range(10, 50)] float m_AngularSpeed = 60f;

    [Tooltip("The max acceleration the AI will follow with.")]
    [SerializeField, Range(0.5f, 50)] float m_Acceleration = 8f;

    //============================================================
    [Header("Avoidance"), Space()] //==================================================  Avoidance

    [Tooltip("Apply the height to all Ai or none.")]
    [SerializeField] ApplyHeight m_ApplyHeight = ApplyHeight.ALL;

    [Tooltip("Having a lower priority means more importance for avoidance.")]
    [SerializeField, Range(0, 50)] int m_PriorityAvoid = 5;

    [Tooltip("Choose the radius of the avoidance distance between AI & objects.")]
    [SerializeField, Range(0, 4)] float m_AvoidanceRadius = 1.5f;

    [Tooltip("Choose the height of the NavMesh")]
    [SerializeField, Range(0, 4)] float m_AvoidanceHeight = 2f;

    //============================================================
    [Header("Stun Settings"), Space()] //==================================================  Stuns

    [Tooltip("When enabled the Ai will be protected from getting stunned for a few seconds.")]
    [SerializeField] ProtectStart m_ProtectedAtStart = ProtectStart.ON;

    [Tooltip("How long the stun will last.")]
    [SerializeField, Range(0, 10)] float m_StunTime = 5;

    [Tooltip("How long the cooldown for a stun will be.")]
    [SerializeField, Range(3, 10)] float m_StunCooldown = 3;

    [Header("Shoot Settings"), Space()] //==================================================  Shooting

    [Tooltip("The type of projectile the enemy will use.")]
    [SerializeField] ProjectileType m_ProType = ProjectileType.EnemyProjectile;

    [Tooltip("The duration of how long it will take for AI to shoot each shot.")]
    [SerializeField, Range(3, 10)] float m_ShootCooldown = 5;

    [Tooltip("The speed scale of the projectile shot.")]
    [SerializeField, Range(0, 1)] float m_BulletSpeedScale = 0.5f;

    [Header("Spawns")]

    [SerializeField] Transform[] m_SpawnList = null;
    public int m_CurrentSpawn = 0;
    public int m_AmountPerSpawn = 0;

    [Header("External")]
    [Tooltip("Reference to the projectile manager.")]
    [SerializeField] ProjectileManager m_Projectile = null;
    //============================================================

    AIModule[] m_AiList = null;

    //============================================================
    private void Start()
    {
        m_AiList = FindObjectsOfType<AIModule>(); // find & assign the AIs.
        if (m_AiList == null)
        {
            Debug.Break();
            Debug.LogError("No AiModules can be found!");
        }
        else // if we have some AI in the array we begin to parent them to the manager.
        {
            ParentAI();
        }

        if (m_PlayerAssignment == AIsearchMode.TAG) // Player assignment can be done manually or it can be done by the system at start up time.
        {
            GameObject Temp = GameObject.FindGameObjectWithTag("Player");
            m_Target = Temp;

            if (m_Target)
            {
                ApplyTargets(m_Target); // Once it has the player object it assigns it as the target to the other AIs.
            }
        }
        else
        {
            if (m_Target != null)
            {
                ApplyTargets(m_Target);
            }
            else
            {
                Debug.Break();
                Debug.LogError("No Target is assigned to Ai Manager!");
            }
        }

        AssignCheckerVersion();// Assign the checker versions for the AIs to use.
        AssignValues(); // begin to assign all values from the manager to other AIs.
        SetSpawns();
    }

    void AssignCheckerVersion()
    {
        if (m_CheckerVersion == CheckerEdition.NEW)
        {
            for (int i = 0; i < m_AiList.Length; i++)
            {
                m_AiList[i].SetCheckerVersion(true);
            }
        }
        else
        {
            for (int i = 0; i < m_AiList.Length; i++)
            {
                m_AiList[i].SetCheckerVersion(false);
            }
        }
    }

    //===========================================
    // All Ai modules get their values assigned.
    void AssignValues()
    {
        if (m_AiList == null)
        {
            Debug.Break();
            Debug.LogError("The AI failed to be assigned.");
        }

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetStopDistance(m_StoppingDistance);
            m_AiList[i].SetStunTimer(m_StunTime);
            m_AiList[i].SetAcceleration(m_Acceleration);

            m_AiList[i].SetStunCooldown(m_StunCooldown);
            m_AiList[i].SetShootCooldown(m_ShootCooldown);

            m_AiList[i].SetProjectileSpeed(m_BulletSpeedScale);
            m_AiList[i].SetProjectileType(m_ProType);

            if (m_ProtectedAtStart == ProtectStart.ON)
            {
                m_AiList[i].ProtectedAtStart(true);
            }
            else
            {
                m_AiList[i].ProtectedAtStart(false);
            }
        }
        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetProjectileManager(m_Projectile);
        }

        // Nav mesh agent settings get appiled
        for (int i = 0; i < m_AiList.Length; i++)
        {

            m_AiList[i].SetPriority(m_PriorityAvoid);
            m_AiList[i].SetAngularDistance(m_AngularSpeed);
            m_AiList[i].SetAvoidanceRadius(m_AvoidanceRadius);
            m_AiList[i].SetSpeed(m_AiSpeed);

            if (m_ApplyHeight == ApplyHeight.ALL)
            {
                m_AiList[i].AvoidanceHeight(m_AvoidanceHeight);
            }
        }
    }

    //===========================================
    // Parent the AIs to the manager to clean up the hierarchy.
    void ParentAI()
    {
        if (m_AiList == null) return;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].gameObject.transform.parent = gameObject.transform;
        }
    }

    //===========================================
    // Set the target to go after.
    void ApplyTargets(GameObject _target)
    {
        if (m_AiList == null) return;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetPlayerTarget(_target);
        }
    }

    //===========================================
    // Kills every AI in the scene if they aren't dead already.
    public void KillAll()
    {
        for (int i = 0; i < m_AiList.Length; i++)
        {
            if (m_AiList[i].IsDead() == false)
            {
                m_AiList[i].Kill();
            }
        }
    }

    //===========================================
    // Brings all of the AI in the scene back to life again, if not already alive.
    public void ReviveAll()
    {
        m_CurrentSpawn = 0;

        m_AmountPerSpawn = m_AiList.Length / m_SpawnList.Length;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].enabled = true;
            m_AiList[i].gameObject.GetComponent<NavMeshAgent>().enabled = true;
            m_AiList[i].Revive(m_SpawnList[m_CurrentSpawn]);

            if (i % m_AmountPerSpawn == 0)
            {
                m_CurrentSpawn++;
            }
        }

    }
    void SetSpawns()
    {
        m_AmountPerSpawn = m_AiList.Length / m_SpawnList.Length;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetPosition(m_SpawnList[m_CurrentSpawn]);

            if (i % m_AmountPerSpawn == 0)
            {
                m_CurrentSpawn++;
            }
        }
    }


    public bool AreAllDead()
    {
        for (int i = 0; i < m_AiList.Length; i++)
        {
            if (!m_AiList[i].IsDead())
            {
                return false;
            }
        }
        return true;
    }

    public int EnemyCount()
    {
        return m_AiList.Length;
    }
}