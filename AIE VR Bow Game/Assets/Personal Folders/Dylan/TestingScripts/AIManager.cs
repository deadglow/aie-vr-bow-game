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

    [Header("Player Assignment")]

    [SerializeField, Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
    AIsearchMode m_PlayerAssignment = AIsearchMode.MANUALLY;
    [SerializeField] GameObject m_Target = null;

    //============================================================
    [Space()]
    [Header("AI Settings")] //==================================================  Main

    [Tooltip("The checker is responsible for calulating the flee & stun distances from stopping distance.")]
    [SerializeField] CheckerEdition m_CheckerVersion = CheckerEdition.NEW;

    [Tooltip("The speed the AI will move at.")]
    [SerializeField, Range(0.5f, 5)] float m_AiSpeed = 2f;

    [Tooltip("How far the AI will stay away from the target.")]
    [SerializeField, Range(10, 50)] float m_StoppingDistance = 10;

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
    [SerializeField] bool m_ProtectedAtStart = false;

    [Tooltip("How long the stun will last.")]
    [SerializeField, Range(0, 10)] float m_StunTime = 5;

    [Tooltip("How long the cooldown for a stun will be.")]
    [SerializeField, Range(3, 10)] float m_StunCooldown = 3;

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
                ApplyTargets(m_Target); // Once it has theplayer object it assignes it as the target to the other AIs.
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
            Debug.LogError("The AI list failed to be assigned.");
        }

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetStopDistance(m_StoppingDistance);
            m_AiList[i].SetStunTimer(m_StunTime);
            m_AiList[i].SetAcceleration(m_Acceleration);
            if (m_ProtectedAtStart)
            {
                m_AiList[i].ProtectedAtStart(m_ProtectedAtStart);
            }
            m_AiList[i].SetStunCooldown(m_StunCooldown);
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
        for (int i = 0; i < m_AiList.Length; i++)
        {
            if (!m_AiList[i].enabled)
            {
                m_AiList[i].gameObject.GetComponent<NavMeshAgent>().enabled = true;
                m_AiList[i].enabled = true;
            }
        }
    }
}