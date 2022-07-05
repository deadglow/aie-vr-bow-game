using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySoundEvents : MonoBehaviour
{
	new public AudioModule audio;
	public string soundName;

	public void EnemyStep()
	{
		audio.PlaySFX(soundName);
	}
}
