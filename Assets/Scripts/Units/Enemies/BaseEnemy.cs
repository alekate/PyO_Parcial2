using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseEnemy : BaseUnit
{
public void Attack(BaseHero target)
{
    target.TakeDamage(meleeAttackDamage);
}


}
