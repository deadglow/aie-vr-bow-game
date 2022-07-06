using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowFireSFXManager : MonoBehaviour
{
	public string lethalArrowSFX;
	public string teleArrowSFX;
	public AudioModule module;

	public void OnFire(ProjectileType type)
	{
		switch(type)
		{
			case ProjectileType.Arrow:
				module.PlaySFX(lethalArrowSFX);
			break;
			
			case ProjectileType.TeleportArrow:
				module.PlaySFX(teleArrowSFX);
			break;

			default:
				break;
		}
	}
}
