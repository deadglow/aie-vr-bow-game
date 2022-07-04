using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;



public class AIManager : MonoBehaviour
{
    public enum AIsearchMode
    {
        MANUALLY = 0,
        TAG,
    }

    public enum ApplyHeight
    {
        NONE = 0,
        ALL,
    }

    public enum ProtectStart
    {
        ON = 0,
        OFF,
    }

    [System.Serializable]
    public struct Player
    {
        [Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
        public AIsearchMode m_PlayerAssignment;
        public GameObject m_Target;
    }

    [System.Serializable]
    public struct AI
    {
        [Tooltip("The speed the AI will move at.")]
        [Range(0.5f, 5)] public float m_AiSpeed;

        [Tooltip("The time it takes before the AI is allowed to attack.")]
        [Range(0, 10)] public float m_TimeTillAttack;

        [Tooltip("How far the AI will stay away from the target.")]
        [Range(10, 25)] public float m_StoppingDistance;

        [Tooltip("The max turning speed when following a path.")]
        [Range(10, 50)] public float m_AngularSpeed;

        [Tooltip("The max acceleration the AI will follow with.")]
        [Range(0.5f, 50)] public float m_Acceleration;
    }

    [System.Serializable]
    public struct Avoidance
    {
        [Tooltip("Apply the height to all Ai or none.")]
        public ApplyHeight m_ApplyHeight;

        [Tooltip("Having a lower priority means more importance for avoidance.")]
        [Range(0, 50)] public int m_PriorityAvoid;

        [Tooltip("Choose the radius of the avoidance distance between AI & objects.")]
        [Range(0, 4)] public float m_AvoidanceRadius;

        [Tooltip("Choose the height of the NavMesh")]
        [Range(0, 4)] public float m_AvoidanceHeight;
    }

    [System.Serializable]
    public struct Shooting
    {

        [Header("Shoot Settings"), Space()] //==================================================  Shooting

        [Tooltip("The type of projectile the enemy will use.")]
        public ProjectileType m_ProType;

        [Tooltip("The duration of how long it will take for AI to shoot each shot.")]
        public float m_ShootCooldown;

        [Tooltip("The speed scale of the projectile shot.")]
        [SerializeField, Range(0, 1)] public float m_BulletSpeedScale;
    }

    [System.Serializable]
    public struct Spawns
    {
        public Transform[] m_SpawnList;
        public int m_CurrentSpawn;
        public int m_AmountPerSpawn;
        public ProjectileManager m_Projectile;
    }

    //=====================================================================

    [SerializeField] Player m_PlayerSettings;

    [SerializeField] AI m_AiSettings;

    [SerializeField] Avoidance m_Avoidance;

    [SerializeField] Shooting m_Attack;

    [SerializeField] Spawns m_AiSpawns;
    AIModule[] m_AiList = null;

    //int m_PerviousPermission = 0;

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

        if (m_PlayerSettings.m_PlayerAssignment == AIsearchMode.TAG) // Player assignment can be done manually or it can be done by the system at start up time.
        {
            GameObject Temp = GameObject.FindGameObjectWithTag("Player");
            m_PlayerSettings.m_Target = Temp;

            if (m_PlayerSettings.m_Target)
            {
                ApplyTargets(m_PlayerSettings.m_Target); // Once it has the player object it assigns it as the target to the other AIs.
            }
        }
        else
        {
            if (m_PlayerSettings.m_Target != null)
            {
                ApplyTargets(m_PlayerSettings.m_Target);
            }
            else
            {
                Debug.Break();
                Debug.LogError("No Target is assigned to Ai Manager!");
            }
        }

        AssignValues(); // begin to assign all values from the manager to other AIs.
        SetSpawns();
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
            m_AiList[i].SetAcceleration(m_AiSettings.m_Acceleration);

            m_AiList[i].SetMaxCast(m_AiSettings.m_StoppingDistance);
            m_AiList[i].SetShootCooldown(m_Attack.m_ShootCooldown);

            m_AiList[i].SetProjectileSpeed(m_Attack.m_BulletSpeedScale);
            m_AiList[i].SetProjectileType(m_Attack.m_ProType);

            m_AiList[i].SetAttackTime(m_AiSettings.m_TimeTillAttack);
        }
        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetProjectileManager(m_AiSpawns.m_Projectile);
        }

        // Nav mesh agent settings get appiled
        for (int i = 0; i < m_AiList.Length; i++)
        {

            m_AiList[i].SetPriority(m_Avoidance.m_PriorityAvoid);
            m_AiList[i].SetAngularDistance(m_AiSettings.m_AngularSpeed);
            m_AiList[i].SetAvoidanceRadius(m_Avoidance.m_AvoidanceRadius);
            m_AiList[i].SetSpeed(m_AiSettings.m_AiSpeed);

            if (m_Avoidance.m_ApplyHeight == ApplyHeight.ALL)
            {
                m_AiList[i].AvoidanceHeight(m_Avoidance.m_AvoidanceHeight);
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
        m_AiSpawns.m_CurrentSpawn = 0;

        m_AiSpawns.m_AmountPerSpawn = m_AiList.Length / m_AiSpawns.m_SpawnList.Length;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].enabled = true;
            m_AiList[i].gameObject.GetComponent<NavMeshAgent>().enabled = true;
            m_AiList[i].Revive(m_AiSpawns.m_SpawnList[m_AiSpawns.m_CurrentSpawn]);

            if (i % m_AiSpawns.m_AmountPerSpawn == 0)
            {
                m_AiSpawns.m_CurrentSpawn++;
            }
        }

    }
    void SetSpawns()
    {
        m_AiSpawns.m_CurrentSpawn = 0;

        m_AiSpawns.m_AmountPerSpawn = m_AiList.Length / m_AiSpawns.m_SpawnList.Length;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetPosition(m_AiSpawns.m_SpawnList[m_AiSpawns.m_CurrentSpawn]);

            if (i % m_AiSpawns.m_AmountPerSpawn == 0)
            {
                m_AiSpawns.m_CurrentSpawn++;
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

    public void PickAI()
    {
        int RandomAI = Random.Range(0, m_AiList.Length);

        for (int i  = 0; i < m_AiList.Length;)
        {
            if (i != RandomAI)
            {
                m_AiList[i].SetPermission(false);
            }
            else
            {
                m_AiList[i].SetPermission(true);
            }
        }
    }
}