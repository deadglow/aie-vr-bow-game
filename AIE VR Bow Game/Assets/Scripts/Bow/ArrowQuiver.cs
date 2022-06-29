using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowQuiver : MonoBehaviour
{
    public ProjectileType type = ProjectileType.Arrow;
	public bool useCount = false;
	public int count = 0;
	public int capacity = 0;

	public ProjectileType TryGetArrow()
	{
		if (!useCount)
			return type;

		if (count > 0)
		{
			count--;
			return type;
		}
		else
			return ProjectileType.None;
	}

	public void ReplenishArrow(int amount)
	{
		count += amount;
		if (count > capacity)
			count = capacity;
		
		if (count < 0)
			count = 0;
	}
}
