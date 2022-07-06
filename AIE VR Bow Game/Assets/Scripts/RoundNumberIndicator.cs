using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RoundNumberIndicator : MonoBehaviour
{
	[HideInInspector]
	public WaveController waveController;
	public TMP_Text text;

	void Awake()
	{
		waveController = FindObjectOfType<WaveController>();
		waveController.OnWaveStart.AddListener(UpdateWaveText);
		waveController.OnWaveEnd.AddListener(ClearWaveText);
	}

	public void UpdateWaveText()
	{
		text.text = waveController.currentWave.ToString();
	}

	public void ClearWaveText()
	{
		text.text = "-";
	}
}
