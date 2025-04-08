using System.Collections;
using UnityEngine;

public class RangerUnit: SoldierUnit
{
    [SerializeField] private Projectile m_ProjectilePrefab;

    protected override void PerformAttackAnimation()
    {
        if (Target == null) return;

        Vector3 direction = (Target.transform.position - transform.position).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            m_Animator.SetTrigger("AttackHorizontal");
        }
        else
        {
            m_Animator.SetTrigger(direction.y > 0 ? "AttackUp" : "AttackDown");
        }
    }

    protected override void OnAttackReady(Unit target)
    {
        OnPlayAttackSound();
        PerformAttackAnimation();
        StartCoroutine(ShootProjectile(0.4f, target));
    }

    private IEnumerator ShootProjectile(float delay, Unit target)
    {
        yield return new WaitForSeconds(delay);

        if (CurrentState == UnitState.Dead) yield return null;

        if (target != null && target.CurrentState != UnitState.Dead)
        {
            var projectile = Instantiate(m_ProjectilePrefab, transform.position, Quaternion.identity);
            projectile.Initialize(this, target, m_AutoAttackDamage);
        }
    }
}
