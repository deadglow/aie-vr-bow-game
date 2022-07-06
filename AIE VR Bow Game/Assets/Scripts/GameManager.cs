using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
	[HideInInspector]
	public WaveController waveController;
	[HideInInspector]
	public BowInput bowInput;
	[HideInInspector]
	public PlayerMover playerMover;

	public UnityEvent OnGameStart;
	public UnityEvent OnGameOver;

	void Awake()
	{
		waveController = FindObjectOfType<WaveController>();
		bowInput = FindObjectOfType<BowInput>();
		playerMover = FindObjectOfType<PlayerMover>();
	}

	void Start()
	{
		playerMover.TeleportPlayerToStartPoint();
	}

	[ContextMenu("Start Game")]
	public void StartGame()
	{
		waveController.StartWaveIndex(0);
		waveController.automaticallySwitchWave = true;
		playerMover.TeleportPlayerToStartPoint();
		OnGameStart.Invoke();
	}

	[ContextMenu("End Game")]
	public void EndGame()
	{
		waveController.automaticallySwitchWave = false;
		waveController.EndWave();
		playerMover.TeleportPlayerToStartPoint();
		OnGameOver.Invoke();
	}
}
