using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] Transform[] m_Spawns = null;

    [SerializeField] int m_CurrentWave = 1;
    [SerializeField] float m_RoundTimer = 5;

    [SerializeField] UnityEvent m_OnWaveStart = null;
    [SerializeField] UnityEvent m_OnWaveEnd = null;

    bool m_WaitForRound = false;
    float m_TimerForRound;

    //=====================================
    AIManager m_AIManager = null;

    private void Start()
    {
        m_AIManager = FindObjectOfType<AIManager>();
        m_RoundTimer = m_TimerForRound;
    }

    void Update()
    {
        if (m_WaitForRound)
        {
            m_RoundTimer -= Time.deltaTime;
            if (m_RoundTimer <= 0)
            {
                m_WaitForRound = false;
                m_RoundTimer = m_TimerForRound;
                m_CurrentWave++;
                SpawnAllEnemys();
            }
        }
        else
        {
            if (m_AIManager.AreAllDead())
            {
                ChangeRound();
            }
        }
    }

    void ChangeRound()
    {
        if (m_OnWaveStart != null)
        {
            m_OnWaveStart.Invoke();
        }
        m_WaitForRound = true;
    }

    void SpawnAllEnemys()
    {
        if (m_OnWaveEnd != null)
        {
            m_OnWaveEnd.Invoke();
        }

        //TODO call spawn all function in AI manager
        m_AIManager.ReviveAll();
    }
}
