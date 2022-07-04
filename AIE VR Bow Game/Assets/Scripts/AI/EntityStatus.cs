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

	public float damageMultiplier = 1.0f;

	public UnityEvent OnDeathEvent;
	public UnityEvent OnReviveEvent;
	public UnityEvent<int> OnHealEvent;
	public UnityEvent<int> OnDamageEvent;
	public UnityEvent<int> OnHealthChange;
	public UnityEvent<Vector3> OnDamagePointEvent;

	public void Damage(int amount)
	{
		ChangeHealth(health - Mathf.FloorToInt(amount * damageMultiplier));
		if (health <= 0)
			Kill();

		OnDamageEvent.Invoke(amount);
	}

	public void DamageAtPoint(int amount, Vector3 point)
	{
		Damage(amount);

		OnDamagePointEvent.Invoke(point);
	}

	public void Heal(int amount)
	{
		ChangeHealth(health + amount);
		if (health > maxHealth)
			ChangeHealth(maxHealth);
		
		OnHealEvent.Invoke(amount);
	}

	public void ChangeHealth(int newAmount)
	{
		health = newAmount;
		OnHealthChange.Invoke(health);
	}

	public void Kill()
	{
		if (isDead) return;

		ChangeHealth(0);
		OnDeathEvent.Invoke();
	}

	public void Revive()
	{
		isDead = false;
		OnReviveEvent.Invoke();
	}
}
