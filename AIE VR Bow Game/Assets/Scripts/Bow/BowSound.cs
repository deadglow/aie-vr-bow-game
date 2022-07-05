using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowSound : MonoBehaviour
{
	public BowVisual bowVisual;
	public AudioSource source;

	private float previousDraw = 0;

	public float minDelta = 0.01f;
	public float maxDelta = .3f;
	public float minPitch = 0.5f;
	public float maxPitch = 1.5f;
	public float minVolume = 0.05f;
	public float maxVolume = 1.0f;

	public float volumeFalloff = 0.1f;

	// Start is called before the first frame update
	void Start()
	{
		previousDraw = bowVisual.currentVisDrawPercent;
	}

	// Update is called once per frame
	void LateUpdate()
	{
		float delta = Mathf.Abs(bowVisual.currentVisDrawPercent - previousDraw);

		source.volume = Mathf.MoveTowards(source.volume, 0, volumeFalloff * Time.deltaTime);
		if (delta > minDelta)
		{
			float t = (delta - minDelta) / (maxDelta - minDelta);
			source.volume = Mathf.Max(Mathf.Lerp(minVolume, maxVolume, t), source.volume);
			source.pitch = Mathf.Lerp(minPitch, maxPitch, previousDraw);
		}

		previousDraw = bowVisual.currentVisDrawPercent;
	}
}
