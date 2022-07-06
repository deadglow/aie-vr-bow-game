using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AISpawnManager : MonoBehaviour
{
	[System.Serializable]
	public struct SpawnData
	{
		public int maxSpawnsTotal;
		public int maxActiveSpawns;
		public int spawnsPerEvent;

		public float spawnDelayDuration;
	}

	[HideInInspector]
    public AIManager manager = null;
	public SpawnData data = new SpawnData();

	private float spawnDelayTimer = 0.0f;

	private int currentSpawnCount = 0;

	public UnityEvent OnSpawn;

	void Start()
	{
		manager = FindObjectOfType<AIManager>();
		Restart();
	}

	public void Restart()
	{
		currentSpawnCount = 0;
		spawnDelayTimer = data.spawnDelayDuration;
		manager.KillAll();
	}

	public bool HitSpawnCap()
	{
		return currentSpawnCount >= data.maxSpawnsTotal;
	}

	void FixedUpdate()
	{
		spawnDelayTimer -= Time.fixedDeltaTime;

		if (spawnDelayTimer <= 0)
		{
			for (int i = 0; i < data.spawnsPerEvent; ++i)
			{
				if (currentSpawnCount >= data.maxSpawnsTotal) break;
				if (manager.m_activeAI >= data.maxActiveSpawns) break;
				
				if (manager.Spawn())
				{
					OnSpawn.Invoke();
					currentSpawnCount++;
				}
			}
			spawnDelayTimer = data.spawnDelayDuration;
		}
	}

}
