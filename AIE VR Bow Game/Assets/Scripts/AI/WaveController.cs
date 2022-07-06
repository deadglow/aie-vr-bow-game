using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WaveController : MonoBehaviour
{
	public AIManager aiManager;
	public AISpawnManager spawnManager;
	public bool automaticallySwitchWave = true;
	public float timeBetweenWaves = 120.0f;
	public List<WaveSet> waveSets;

	public int currentWave { get; private set; } = 0;
	public float waveBreakTimer { get; private set; } = 0;
	private bool waveActive = false;

	public UnityEvent OnWaveStart;
	public UnityEvent OnWaveEnd;
	public UnityEvent OnWaveComplete;

	void Start()
	{
		EndWave();
	}

	void FixedUpdate()
	{
		// Automatically progress to the next wave
		if (automaticallySwitchWave && !waveActive)
		{
			waveBreakTimer -= Time.fixedDeltaTime;
			if (waveBreakTimer < 0)
				NextWave();
		}

		if (waveActive)
		{
			if (spawnManager.HitSpawnCap() && aiManager.m_activeAI == 0)
			{
				EndWave();
				OnWaveComplete.Invoke();
			}
		}
	}

	[ContextMenu("Start Current Wave")]
	public void StartWave()
	{
		if (waveActive)
			EndWave();
		waveActive = true;
		SetWavePropertiesByIndex(currentWave);
		// unpause spawn manager
		spawnManager.enabled = true;
		OnWaveStart.Invoke();
	}

	public void StartWaveIndex(int index)
	{
		currentWave = index;
		StartWave();
	}

	[ContextMenu("End Wave")]
	public void EndWave()
	{
		waveActive = false;
		waveBreakTimer = timeBetweenWaves;
		// reset and pause spawn manager
		spawnManager.Restart();
		spawnManager.enabled = false;
		OnWaveEnd.Invoke();
	}

	[ContextMenu("Next Wave")]
	public void NextWave()
	{
		currentWave++;
		StartWave();
	}

	[ContextMenu("Next Wave")]
	public void PreviousWave()
	{
		currentWave--;
		if (currentWave < 0)
			currentWave = 0;
		StartWave();
	}

	private void SetWavePropertiesByIndex(int waveNumber)
	{
		// If there isn't enough wave data use the last available one
		if (waveNumber >= waveSets.Count)
			waveNumber = waveSets.Count - 1;

		SetWaveProperties(waveSets[waveNumber]);
	}

	private void SetWaveProperties(WaveSet set)
	{
		// set the ai manager properties here
		spawnManager.data = set.spawnData;
		aiManager.m_AiSettings = set.aiProperties;
		aiManager.m_Attack = set.shootingProperties;
		aiManager.AssignValues();
	}

	[System.Serializable]
	public struct WaveSet
	{
		public AISpawnManager.SpawnData spawnData;

		public AIManager.AI aiProperties;
		public AIManager.Shooting shootingProperties;
	}
}
