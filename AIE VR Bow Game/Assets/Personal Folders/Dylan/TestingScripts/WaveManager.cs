using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveManager : MonoBehaviour
{
    [Header("General")]
    [SerializeField] int m_CurrentWave = 1;
    [SerializeField] float m_RoundTimer = 5;
    [SerializeField] UnityEvent m_OnWaveStart = null;
    [SerializeField] UnityEvent m_OnWaveEnd = null;

    bool m_ChangingRound = false;
    float m_TimerForRound;

    private void Start()
    {
        m_RoundTimer = m_TimerForRound;
    }

    void Update()
    {
        if (m_ChangingRound)
        {
            m_RoundTimer -= Time.deltaTime;
            if (m_RoundTimer <= 0)
            {
                m_ChangingRound = false;
                m_RoundTimer = m_TimerForRound;
                m_CurrentWave++;
            }
        }
    }

    void ChangeRound()
    {
        if (m_OnWaveStart != null)
        {
            m_OnWaveStart.Invoke();
        }
    }
}
