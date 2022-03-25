using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationHandler : MonoBehaviour
{
    public delegate void AttackMiss();
    public AttackMiss OnAttackMiss;

    public void AttackMissed()
    {
        OnAttackMiss?.Invoke();
    }
}
