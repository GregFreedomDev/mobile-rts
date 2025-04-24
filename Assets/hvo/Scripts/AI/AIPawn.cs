using System.Collections.Generic;
using hvo.Scripts.Managers;
using UnityEngine;
using UnityEngine.Events;

public class AIPawn : MonoBehaviour
{
    [SerializeField] private float m_Speed = 5f;

    [Header("Separation")]
    [SerializeField] private float m_SeparationRadius = 1f;
    [SerializeField] private float m_SeparationForce = 0.5f;
    [SerializeField] private bool m_ApplySeparation = true;

    private Vector3? m_CurrentDestination;
    private List<Vector3> m_CurrentPath = new();
    private TilemapManager m_TilemapManager;
    private int m_CurrentNodeIndex;
    private BaseGameManager _mGameGameManager;
    private Unit m_Unit;
    private Vector3 m_ExternalPushVelocity;

    public UnityAction<Vector3> OnNewPositionSelected = delegate { };
    public UnityAction OnDestinationReached = delegate { };

    void Start()
    {
        InitializeComponents();
    }

    void Update()
    {
        if (!IsPathValid())
        {
            m_CurrentDestination = null;
            return;
        }

        Vector3 targetPosition = m_CurrentPath[m_CurrentNodeIndex];
        Vector3 combinedDirection = GetCombinedDirection(targetPosition);
        MoveAlongPath(combinedDirection);
        HandleNodeArrival(targetPosition);
    }

    private void InitializeComponents()
    {
        _mGameGameManager = FindObjectOfType<GameManager>() ?? (BaseGameManager)FindObjectOfType<BattleGameManager>();
        m_TilemapManager = TilemapManager.Get();
        m_Unit = GetComponent<Unit>();
    }

    private Vector3 GetCombinedDirection(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        Vector3 separationVector = m_ApplySeparation ? CalculateSeparation() : Vector3.zero;
        float separationWeight = Mathf.Clamp01(distanceToTarget / 1.5f);
        Vector3 combinedDirection = direction + separationVector * separationWeight;

        ApplyPushback(ref combinedDirection);

        if (combinedDirection.magnitude > 1f)
        {
            combinedDirection.Normalize();
        }

        return combinedDirection;
    }

    private void ApplyPushback(ref Vector3 combinedDirection)
    {
        var overlaps = Physics2D.OverlapCircleAll(transform.position, 0.3f);
        if (overlaps.Length <= 3) return;

        Vector3 pushVector = Vector3.zero;
        foreach (var col in overlaps)
        {
            if (col.gameObject == gameObject) continue;
            Vector3 away = transform.position - col.transform.position;
            if (away.sqrMagnitude > 0.001f)
            {
                pushVector += away.normalized / away.magnitude;
            }
        }

        if (pushVector == Vector3.zero) return;

        Vector3 normalizedPush = pushVector.normalized;
        combinedDirection += normalizedPush * 0.2f;

        foreach (var col in overlaps)
        {
            if (col.gameObject == gameObject) continue;

            AIPawn otherPawn = col.GetComponent<AIPawn>();
            if (otherPawn != null)
            {
                Vector3 away = col.transform.position - transform.position;
                if (away.sqrMagnitude > 0.01f)
                {
                    Vector3 pushDir = away.normalized * 0.05f;
                    otherPawn.ApplyExternalPush(pushDir);
                }
            }
        }
    }

    private void MoveAlongPath(Vector3 direction)
    {
        transform.position += direction * m_Speed * Time.deltaTime;

        if (m_ExternalPushVelocity != Vector3.zero)
        {
            transform.position += m_ExternalPushVelocity * Time.deltaTime;
            m_ExternalPushVelocity = Vector3.MoveTowards(m_ExternalPushVelocity, Vector3.zero, 2f * Time.deltaTime);
        }
    }

    private void HandleNodeArrival(Vector3 targetPosition)
    {
        if (Vector3.Distance(transform.position, targetPosition) <= 0.15f)
        {
            if (m_CurrentNodeIndex == m_CurrentPath.Count - 1)
            {
                OnDestinationReached.Invoke();
                m_CurrentPath = new();
            }
            else
            {
                m_CurrentNodeIndex++;
                OnNewPositionSelected.Invoke(m_CurrentPath[m_CurrentNodeIndex]);
            }
        }
    }

    public void SetDestination(Vector3 destination)
    {
        if (m_CurrentDestination.HasValue && Vector3.Distance(m_CurrentDestination.Value, destination) < 0.1f)
        {
            return;
        }

        m_CurrentDestination = destination;
        m_CurrentPath = m_TilemapManager.FindPath(transform.position, destination);
        m_CurrentNodeIndex = 0;

        if (m_CurrentPath.Count > 0)
        {
            OnNewPositionSelected.Invoke(m_CurrentPath[m_CurrentNodeIndex]);
        }
    }

    public void Stop()
    {
        m_CurrentPath.Clear();
        m_CurrentNodeIndex = 0;
    }

    protected virtual bool GetPlayerStatus()
    {
        if (m_Unit != null) return m_Unit.IsPlayer;
        m_Unit = GetComponent<Unit>();
        return m_Unit.IsPlayer;
    }

    Vector3 CalculateSeparation()
    {
        Vector3 separationVector = Vector3.zero;
        float separationRadiusSqr = m_SeparationRadius * m_SeparationRadius;
        List<Unit> units = _mGameGameManager.GetFriendlyUnits(GetPlayerStatus());

        foreach (var unit in units)
        {
            if (unit == null || unit.gameObject == null || unit.gameObject == gameObject) continue;

            Vector3 opositeDirection = transform.position - unit.transform.position;
            float sqrDistance = opositeDirection.sqrMagnitude;

            if (sqrDistance < separationRadiusSqr && sqrDistance > 0)
            {
                separationVector += opositeDirection.normalized / sqrDistance;
            }
        }

        return separationVector * m_SeparationForce;
    }

    bool IsPathValid()
    {
        return m_CurrentPath.Count > 0 && m_CurrentNodeIndex < m_CurrentPath.Count;
    }

    public void ApplyExternalPush(Vector3 pushDirection)
    {
        m_ExternalPushVelocity += pushDirection;

        float maxPushMagnitude = 1f;
        if (m_ExternalPushVelocity.magnitude > maxPushMagnitude)
        {
            m_ExternalPushVelocity = m_ExternalPushVelocity.normalized * maxPushMagnitude;
        }
    }
}