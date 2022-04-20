using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimationHandler : MonoBehaviour
{
    public delegate void AttackMiss();
    public AttackMiss OnAttackMiss_AttackMiss;

    public void AttackMissed_void()
    {
        OnAttackMiss_AttackMiss?.Invoke();
    }
}
