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
		public string m_tagName;
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
        [Range(0, 25)] public float m_RaycastDistance;

        [Tooltip("The max turning speed when following a path.")]
        [Range(10, 250)] public float m_AngularSpeed;

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
		public Transform m_SpawnListParent;
		[HideInInspector]
        public List<Transform> m_SpawnList;
        [Range(5, 50)] public float m_SpawnRadius;
        public int m_CurrentSpawn;
        public int m_AmountPerSpawn;
        public ProjectileManager m_Projectile;
    }

    //=====================================================================

    [SerializeField] Player m_PlayerSettings;

    [SerializeField] public AI m_AiSettings;

    [SerializeField] Avoidance m_Avoidance;

    [SerializeField] public Shooting m_Attack;

    [SerializeField] Spawns m_AiSpawns;

    [SerializeField] AIModule[] m_AiList = null;
	[SerializeField] Queue<AIModule> m_deadAiQueue = new Queue<AIModule>();
	public int m_activeAI { get; private set; }

    int m_PerviousSpawn = 0;
	AIModule m_lastShootAI = null;

	public List<Transform> m_SafeSpawns = null;
    int m_PreviousAi;
    bool m_AiBusy = false;
	private float m_ShootTimer = 0;
	//============================================================
	private void Awake()
    {
		m_AiSpawns.m_SpawnList = new List<Transform>();
		// Populate spawn list from parent
		for (int i = 0; i < m_AiSpawns.m_SpawnListParent.childCount; ++i)
		{
			m_AiSpawns.m_SpawnList.Add(m_AiSpawns.m_SpawnListParent.GetChild(i));
		}

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
            GameObject Temp = GameObject.FindGameObjectWithTag(m_PlayerSettings.m_tagName);
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

		// THIS BROKEY EVERYTHING LMAO
        //AssignValues(); // begin to assign all values from the manager to other AIs.
        //PickFiringAI();
        //AssignSpawnPoints();

		m_ShootTimer = m_Attack.m_ShootCooldown;
	}

	void FixedUpdate()
	{
		AssignSpawnPoints();

		List<AIModule> shootingAi = new List<AIModule>();

		foreach (AIModule ai in m_AiList)
		{
			if (!ai.IsDead() && ai.m_EnemyStates == AIModule.EnemyStates.SHOOT)
			{
				shootingAi.Add(ai);
			}
		}

		if (shootingAi.Count > 0)
		{
			m_ShootTimer -= Time.fixedDeltaTime;
			if (m_ShootTimer <= 0)
			{
				AIModule ai = null;
				while (!ai)
				{
					int randomIndex = Random.Range(0, shootingAi.Count);
					
					if (shootingAi.Count == 1 || shootingAi[randomIndex] != m_lastShootAI)
						ai = shootingAi[randomIndex];
				}

				m_lastShootAI = ai;
				ai.FireAtPlayer();
				m_ShootTimer = m_Attack.m_ShootCooldown;
			}
		}
	}

    //===========================================
    // All Ai modules get their values assigned.
    public void AssignValues()
    {
        if (m_AiList == null)
        {
            Debug.Break();
            Debug.LogError("The AI failed to be assigned.");
        }

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetAcceleration(m_AiSettings.m_Acceleration);

            m_AiList[i].SetMaxCast(m_AiSettings.m_RaycastDistance);
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

    public void PickFiringAI()
    {
        bool GotID = false;
        int RandomNumber = 0;
        if (m_AiList.Length > 0)
        {
            while (!GotID)
            {
                RandomNumber = Random.Range(0, m_AiList.Length);
                if ((m_AiList.Length == 1 || RandomNumber != m_PreviousAi) && !m_AiList[RandomNumber].IsDead())
                {
                    m_PerviousSpawn = RandomNumber;
                    GotID = true;
                }
            }
            m_AiList[RandomNumber].GivePermission();
            GotID = false;
        }
    }

    public void SetPermission(bool _state)
    {
        m_AiBusy = _state;
    }

    public bool AskPermission()
    {
        return m_AiBusy;
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

		m_activeAI = 0;
	}

    //===========================================
    // Brings all of the AI in the scene back to life again, if not already alive.
    public void ReviveAll()
    {
		while (m_deadAiQueue.Count > 0)
		{
			Spawn();
		}
    }

	public bool Spawn()
	{
		if (m_deadAiQueue.Count == 0)
		{
			print("No available AI.");
			return false;
		}
		//=========================================

        int SpawnPoint = 0;
        bool GotID = false;

		// Get an AI that is dead.

        if (m_SafeSpawns.Count > 0)
        {
			while (!GotID)
			{
				SpawnPoint = Random.Range(0, m_SafeSpawns.Count);
				if (SpawnPoint != m_PerviousSpawn && m_SafeSpawns.Count > 2 || SpawnPoint == m_PerviousSpawn && m_SafeSpawns.Count < 2)
				{
					m_PerviousSpawn = SpawnPoint;
					GotID = true;
				}
			}

			AIModule ai = m_deadAiQueue.Dequeue();
			ai.Revive(m_SafeSpawns[SpawnPoint]);
			GotID = false;

			m_activeAI++;

			return true;
        }

		return false;
	}

	public void OnAIKill(AIModule ai)
	{
		m_deadAiQueue.Enqueue(ai);
		m_activeAI--;
	}

    void AssignSpawnPoints()  //TODO assign all AIs to Random Spawns.
    {
        float Distance;
        m_AiSpawns.m_CurrentSpawn = 0;

        m_SafeSpawns.Clear();

        for (int i = 0; i < m_AiSpawns.m_SpawnList.Count; i++)
        {
            Distance = Vector3.Distance(m_AiSpawns.m_SpawnList[i].position, m_PlayerSettings.m_Target.transform.position);

            if (Distance >= m_AiSpawns.m_SpawnRadius)
            {
                if (m_AiSpawns.m_SpawnList[i].gameObject.activeSelf == false)
                {
                    m_AiSpawns.m_SpawnList[i].gameObject.SetActive(true);
                }

                m_SafeSpawns.Add(m_AiSpawns.m_SpawnList[i]);   // ERROR HERE

                m_AiSpawns.m_CurrentSpawn++;
            }
            else
            {
                if (m_AiSpawns.m_SpawnList[i].gameObject.activeSelf == true)
                {
                    m_AiSpawns.m_SpawnList[i].gameObject.SetActive(false);
                }
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