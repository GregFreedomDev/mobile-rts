using System.Collections;
using UnityEngine;

public class EnemyRangerUnit : EnemyUnit
{
    [SerializeField] private Projectile m_ProjectilePrefab;

   protected override void Start()
    {
        base.Start();
        m_SpriteRenderer.flipX = false;
    }

    protected override void OnAttackReady(Unit target)
    {
        OnPlayAttackSound();
        PerformAttackAnimation();
        StartCoroutine(ShootProjectile(0.4f, target));
    }

    protected override void PerformAttackAnimation()
    {
        if (Target == null) return;


        Vector3 direction = (Target.transform.position - transform.position).normalized;
        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
        {
            m_Animator.SetTrigger("Attack");
        }
        else
        {
            m_Animator.SetTrigger(direction.y > 0 ? "AttackUp" : "AttackDown");
        }
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