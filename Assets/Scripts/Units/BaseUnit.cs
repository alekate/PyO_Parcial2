using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit : MonoBehaviour {
    public string UnitName;
    public Tile OccupiedTile;
    public Faction Faction;
    public bool HasActed = false;

    public int maxHealth;
    public int health;
    public int velocity;

    public int rangeAttackDistance;
    public int rangeAttackDamage;

    public int meleeAttackDistance;
    public int meleeAttackDamage;

    public int healAmount;
    public int healDistance;

    private SpriteRenderer _spriteRenderer;
    public void SetTile(Tile tile)
    {
        OccupiedTile = tile;
        transform.position = tile.transform.position;
    }

    public virtual void Init()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }
    private void UpdateVisual()
    {
        if (_spriteRenderer == null) return;

        _spriteRenderer.color = HasActed ? Color.gray : Color.white;
    }

    public void SetHasActed(bool value)
    {
        HasActed = value;
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = value ? Color.gray : Color.white;
        }
    }


    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        if (OccupiedTile != null)
            OccupiedTile.SetUnit(null);

        if (UnitManager.Instance != null)
            UnitManager.Instance.OnUnitDeath(this);
        Destroy(gameObject);
    }

    public virtual void Heal(int amount)
    {
        health = Mathf.Min(maxHealth, health + amount);
        UpdateVisual();
    }

}
