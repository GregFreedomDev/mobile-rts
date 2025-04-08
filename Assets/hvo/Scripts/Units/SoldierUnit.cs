using UnityEngine;

public class SoldierUnit : HumanoidUnit
{
    private bool m_IsRetreating = false;

    protected override void Start()
    {
        base.Start();
        m_SpriteRenderer.flipX = false;
    }
    
    public override void SetStance(UnitStanceActionSO stanceActionSO)
    {
        base.SetStance(stanceActionSO);

        if (CurrentStance == UnitStance.Defensive)
        {
            SetState(UnitState.Idle);
            StopMovement();
            m_IsRetreating = false;
        }
    }

    protected override void OnSetState(UnitState oldState, UnitState newState)
    {
        if (newState == UnitState.Attacking)
        {
            m_NextAutoAttackTime = Time.time + m_AutoAttackFrequency / 2f;
        }

        base.OnSetState(oldState, newState);
    }

    protected override void OnSetTask(UnitTask oldTask, UnitTask newTask)
    {
        if (newTask == UnitTask.Attack && HasTarget)
        {
            MoveTo(Target.transform.position);
        }

        base.OnSetTask(oldTask, newTask);
    }

    protected override void OnSetDestination(DestinationSource source)
    {
        base.OnSetDestination(source);

        if (
            HasTarget
            && source == DestinationSource.PlayerClick
            && (CurrentTask == UnitTask.Attack || CurrentState == UnitState.Attacking))
        {
            m_IsRetreating = true;
            SetTarget(null);
            SetTask(UnitTask.None);
        }
    }

    protected override void OnDestinationReached()
    {
        if (m_IsRetreating)
        {
            m_IsRetreating = false;
        }
    }

    protected override void UpdateBehaviour()
    {
        if(!m_GameManager.BattleController.BattleStarted)
        {
            return;
        }

        if (CurrentState == UnitState.Idle || CurrentState == UnitState.Moving)
        {
            if (HasTarget)
            {
                if (IsTargetInRange(Target))
                {
                    StopMovement();
                    SetState(UnitState.Attacking);
                }
                else if (CurrentStance == UnitStance.Offensive)
                {
                    MoveTo(Target.transform.position);
                }
            }
            else
            {
                if (CurrentStance == UnitStance.Offensive)
                {
                    if (!m_IsRetreating)
                    {
                        // Buscar el objetivo más cercano en todo el tablero
                        Unit closestFoe = m_GameManager.FindClosestUnit(transform.position, float.MaxValue, !IsPlayer);
                        if (closestFoe != null)
                        {
                            SetTarget(closestFoe);
                            SetTask(UnitTask.Attack);
                            MoveTo(closestFoe.transform.position);
                        }
                    }
                }
            }
        }
        else if (CurrentState == UnitState.Attacking)
        {
            if (HasTarget)
            {
                if (IsTargetInRange(Target))
                {
                    TryAttackCurrentTarget();
                    StopMovement();
                }
                else
                {
                    if (CurrentStance == UnitStance.Defensive)
                    {
                        SetTarget(null);
                        SetState(UnitState.Idle);
                    }
                    else
                    {
                        MoveTo(Target.transform.position);
                    }
                }
            }
            else
            {
                SetState(UnitState.Idle);
            }
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
