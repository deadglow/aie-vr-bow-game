using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public int healthCount = 3;
	public RectTransform healthImage;
	public float width = 6;

	void Start()
	{
		UpdateHealthVisual();
	}

	public void UpdateHealthCount(int count)
	{
		healthCount = count;
		UpdateHealthVisual();
	}

	public void UpdateHealthVisual()
	{
		Vector2 sizeDelta = healthImage.sizeDelta;
		sizeDelta.x = width * healthCount;
		healthImage.sizeDelta = sizeDelta;
	}
}
