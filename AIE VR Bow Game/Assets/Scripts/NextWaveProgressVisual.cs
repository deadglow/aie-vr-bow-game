using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NextWaveProgressVisual : MonoBehaviour
{
	[HideInInspector]
	public WaveController wave;
	public Image image;

	void Awake()
	{
		wave = FindObjectOfType<WaveController>();
		wave.OnWaveStart.AddListener(Deactivate);
		wave.OnWaveEnd.AddListener(Reactivate);
		gameObject.SetActive(false);
	}

	void Reactivate()
	{
		image.fillAmount = 0;
		gameObject.SetActive(true);
	}

	void Deactivate()
	{
		gameObject.SetActive(false);
	}

	void LateUpdate()
	{
		image.fillAmount = Mathf.Clamp01(wave.waveBreakTimer / wave.timeBetweenWaves);
	}
}
