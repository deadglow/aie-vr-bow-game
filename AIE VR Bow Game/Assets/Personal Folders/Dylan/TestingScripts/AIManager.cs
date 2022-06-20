using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    public enum AIsearchMode
    {
        MANUALLY = 0,
        TAG,
    }

    [Header("Player Assignment")]

    [SerializeField, Tooltip("MANUALLY: Assign the player yourself. | TAG: Search for object with Player tag.")]
    AIsearchMode m_PlayerAssignment = AIsearchMode.TAG;

    [SerializeField] GameObject m_Target = null;

    [Space()]
    [Header("AI Settings")]

    [Tooltip("The speed the AI will move at.")]
    [SerializeField, Range(0.5f, 5)] float m_AiSpeed = 2f;

    [Tooltip("How far the AI will stay away from the target.")]
    [SerializeField, Range(10, 50)] float m_StoppingDistance = 10;

    [Space()]

    [Tooltip("Having a lower priority means more importance for avoidance.")]
    [SerializeField, Range(0, 50)] int m_PriorityAvoid = 5;

    [Tooltip("Choose the radius of the avoidance distance between AI & objects.")]
    [SerializeField, Range(0, 4)] float m_AvoidanceRadius = 1.5f;

    AIModule[] m_AiList = null;

    [Tooltip("How long the stun will last.")]
    [SerializeField, Range(0, 10)] float m_StunTime = 5;

    private void Start()
    {
        m_AiList = FindObjectsOfType<AIModule>();
        if (m_AiList == null)
        {
            Debug.LogError("No AiModules can be found!");
        }
        else
        {
            ParentAI();
        }

        if (m_PlayerAssignment == AIsearchMode.TAG)
        {
            GameObject Temp = GameObject.FindGameObjectWithTag("Player");
            m_Target = Temp;
            ApplyTargets(m_Target);
        }
        else
        {
            if (m_Target != null)
            {
                ApplyTargets(m_Target);
            }
            else
            {
                Debug.LogError("Player has not been assigned!");
            }
        }
        AssignValues();
    }

    void AssignValues()
    {
        if (m_AiList == null) return;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetStopDistance(m_StoppingDistance);
            m_AiList[i].SetStunTimer(m_StunTime);
            m_AiList[i].SetPriority(m_PriorityAvoid);
            m_AiList[i].SetAvoidanceRadius(m_AvoidanceRadius);
            m_AiList[i].SetSpeed(m_AiSpeed);
        }
    }
    void ParentAI()
    {
        if (m_AiList == null) return;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].gameObject.transform.parent = gameObject.transform;
        }
    }

    void ApplyTargets(GameObject _target)
    {
        if (m_AiList == null) return;

        for (int i = 0; i < m_AiList.Length; i++)
        {
            m_AiList[i].SetPlayerTarget(_target);
        }
    }

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

    public void ReviveAll()
    {
        for (int i = 0; i < m_AiList.Length; i++)
        {
            if (!m_AiList[i].enabled)
            {
                m_AiList[i].enabled = true;
            }
        }
    }

}
