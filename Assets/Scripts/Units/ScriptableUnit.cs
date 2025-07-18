using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Unit",menuName = "Scriptable Unit")]
public class ScriptableUnit : ScriptableObject { 
    public Faction Faction;
    public BaseUnit UnitPrefab;
    public string UnitName;

    public int health;
    public int velocity;

    public int rangeAttackDistance;
    public int rangeAttackDamage;

    public int meleeAttackDamage;

    public int healAmount;
    public int healDistance;
}

public enum Faction {
    Hero = 0,
    Enemy = 1
}