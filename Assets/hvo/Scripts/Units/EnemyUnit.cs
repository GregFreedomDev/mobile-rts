using UnityEngine;

public class EnemyUnit : HumanoidUnit
{
    private float m_AttackCommitmentTime = 1f;
    private float m_CurrentAttackCommitmentTime = 0;
    public override bool IsPlayer => false;

    protected override void Start()
    {
        base.Start();
        m_SpriteRenderer.flipX = false;
    }

    protected override void UpdateBehaviour()
    {
        if(!m_GameManager.BattleController.BattleStarted)
        {
            return;
        }

        switch (CurrentState)
        {
            case UnitState.Idle:
            case UnitState.Moving:
                if (HasTarget)
                {
                    if (IsTargetInRange(Target))
                    {
                        SetState(UnitState.Attacking);
                        StopMovement();
                    }
                    else
                    {
                        MoveTo(Target.transform.position);
                    }
                }
                else
                {
                    // Buscar el objetivo más cercano en todo el tablero
                    Unit closestFoe = m_GameManager.FindClosestUnit(transform.position, float.MaxValue, !IsPlayer);
                    if (closestFoe != null)
                    {
                        SetTarget(closestFoe);
                        MoveTo(closestFoe.transform.position);
                    }
    
                }

                break;
            
            case UnitState.Attacking:
                if (HasTarget)
                {
                    if (IsTargetInRange(Target))
                    {
                        m_CurrentAttackCommitmentTime = m_AttackCommitmentTime;
                        TryAttackCurrentTarget();
                    }
                    else
                    {
                        m_CurrentAttackCommitmentTime -= Time.deltaTime;
                        if (m_CurrentAttackCommitmentTime <= 0)
                        {
                            SetState(UnitState.Moving);
                        }
                    }
                }
                else
                {
                    SetState(UnitState.Idle);
                }
                break;
        }
    }

    protected override void UpdateMovementAnimation()
    {
        base.UpdateMovementAnimation();
    }

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
}
