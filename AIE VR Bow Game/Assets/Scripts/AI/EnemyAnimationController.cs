using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
	public Animator animator;
	public AIModule aIModule;

	private NavMeshAgent nvAgent;

	void Start()
	{
		nvAgent = aIModule.GetComponent<NavMeshAgent>();
		aIModule.m_OnShoot.AddListener(Fire);
	}

	void LateUpdate()
	{
		animator.SetFloat("Move Speed", nvAgent.velocity.magnitude);

		animator.SetBool("Stunned", aIModule.m_IsStuned);

		animator.SetBool("Charging", aIModule.m_EnemyStates == AIModule.EnemyStates.SHOOT);
	}

	public void Fire()
	{
		animator.SetTrigger("Attack");
	}
}
