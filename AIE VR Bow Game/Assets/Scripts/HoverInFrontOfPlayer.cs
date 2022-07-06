using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoverInFrontOfPlayer : MonoBehaviour
{
	private Transform player;
	public float rotationMatchSpeed;
	public float offset;

	private Vector3 currentVector = Vector3.up;

	void Start()
	{
		player = GameObject.FindGameObjectWithTag("Player").transform;
	}

	void LateUpdate()
	{
		currentVector = Vector3.RotateTowards(currentVector, player.forward, rotationMatchSpeed * Time.deltaTime * Mathf.Deg2Rad, 0);

		Vector3 position = player.position * offset;
		transform.position = position;
		transform.rotation = Quaternion.LookRotation(-currentVector, Vector3.up);
	}
}
