using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EntityStatus : MonoBehaviour
{
	[field: SerializeField]
    public int health { get; private set; }
	[field: SerializeField]
	public int maxHealth { get; private set; }
	public bool isDead {get; private set; } = false;

	public UnityEvent OnDeathEvent;
	public UnityEvent OnReviveEvent;
	public UnityEvent<int> OnHealEvent;
	public UnityEvent<int> OnDamageEvent;

	public void Damage(int amount)
	{
		health -= amount;
		if (health <= 0)
			Kill();

		OnDamageEvent.Invoke(amount);		
	}

	public void Heal(int amount)
	{
		health += amount;
		if (health > maxHealth)
			health = maxHealth;
		
		OnHealEvent.Invoke(amount);
	}

	public void Kill()
	{
		if (isDead) return;

		health = 0;
		OnDeathEvent.Invoke();
	}

	public void Revive()
	{
		isDead = false;
		OnReviveEvent.Invoke();
	}
}
