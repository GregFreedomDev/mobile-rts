using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class AIPawn : MonoBehaviour
{
    [SerializeField]
    private float m_Speed = 5f;

    [Header("Separation")]
    [SerializeField] private float m_SeparationRadius = 3f;
    [SerializeField] private float m_SeparationForce = 3f;
    [SerializeField] private float m_CombatSeparationMultiplier = 2f;
    [SerializeField] private float m_MinimumSeparationDistance = 1.5f;
    [SerializeField] private bool m_ApplySeparation = true;

    private Vector3? m_CurrentDestination;
    private List<Vector3> m_CurrentPath = new();
    private TilemapManager m_TilemapManager;
    private int m_CurrentNodeIndex;
    private GameManager m_GameManager;
    private Unit m_Unit;
    private bool? m_IsPlayer;

    public UnityAction<Vector3> OnNewPositionSelected = delegate { };
    public UnityAction OnDestinationReached = delegate { };

    void Start()
    {
        m_GameManager = GameManager.Get();
        m_TilemapManager = TilemapManager.Get();
        
        // Inicializar la unidad y su estado de jugador al inicio
        m_Unit = GetComponent<Unit>();
        if (m_Unit != null)
        {
            m_IsPlayer = m_Unit.IsPlayer;
        }
    }

    void Update()
    {
        if (!IsPathValid())
        {
            m_CurrentDestination = null;
            return;
        }

        // Calcular la separación primero
        Vector3 separationVector = (m_ApplySeparation && m_Unit != null) ? CalculateSeparation() : Vector3.zero;
        
        // Si estamos en combate y muy cerca de otra unidad, priorizar la separación
        bool isInCombat = m_Unit != null && m_Unit.CurrentState == UnitState.Attacking;
        bool needsForcedSeparation = false;
        
        if (isInCombat)
        {
            var nearbyUnits = Physics2D.OverlapCircleAll(transform.position, m_MinimumSeparationDistance);
            foreach (var collider in nearbyUnits)
            {
                if (collider.gameObject != gameObject && collider.GetComponent<Unit>() != null)
                {
                    needsForcedSeparation = true;
                    break;
                }
            }
        }

        Vector3 targetPosition = m_CurrentPath[m_CurrentNodeIndex];
        Vector3 direction = (targetPosition - transform.position).normalized;
        Vector3 combinedDirection;

        if (needsForcedSeparation)
        {
            // Si estamos demasiado cerca, priorizar la separación
            combinedDirection = separationVector.normalized * m_SeparationForce * 2f;
        }
        else
        {
            combinedDirection = direction + separationVector;
            if (combinedDirection.magnitude > 1f)
            {
                combinedDirection.Normalize();
            }
        }

        transform.position += combinedDirection * m_Speed * Time.deltaTime;
        
        // Verificar si hemos llegado al destino
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
        // Si ya tenemos el estado guardado, lo devolvemos
        if (m_IsPlayer.HasValue)
        {
            return m_IsPlayer.Value;
        }

        // Si no tenemos la unidad, intentamos obtenerla
        if (m_Unit == null)
        {
            m_Unit = GetComponent<Unit>();
            if (m_Unit != null)
            {
                m_IsPlayer = m_Unit.IsPlayer;
                return m_IsPlayer.Value;
            }
        }

        // Si aún no tenemos la unidad, asumimos que no es jugador
        return false;
    }

    Vector3 CalculateSeparation()
    {
        if (m_GameManager == null) return Vector3.zero;

        Vector3 separationVector = Vector3.zero;
        float currentSeparationRadius = m_SeparationRadius;
        float currentSeparationForce = m_SeparationForce;

        // Aumentar la separación si la unidad está en combate
        if (m_Unit != null && m_Unit.CurrentState == UnitState.Attacking)
        {
            currentSeparationRadius *= m_CombatSeparationMultiplier;
            currentSeparationForce *= m_CombatSeparationMultiplier;
        }

        float separationRadiusSqr = currentSeparationRadius * currentSeparationRadius;
        
        bool isPlayer = GetPlayerStatus();
        List<Unit> units = m_GameManager.GetFriendlyUnits(isPlayer);
        
        if (units == null) return Vector3.zero;

        int nearbyUnits = 0;
        foreach(var unit in units)
        {
            if (unit == null || unit.gameObject == null || unit.gameObject == gameObject) continue;

            Vector3 oppositeDirection = transform.position - unit.transform.position;
            float sqrDistance = oppositeDirection.sqrMagnitude;

            if (sqrDistance < separationRadiusSqr && sqrDistance > 0)
            {
                // Dar más peso cuando están muy cerca
                float weight = (1.0f - (sqrDistance / separationRadiusSqr));
                weight = weight * weight; // Peso cuadrático para hacer la separación más fuerte cuando están muy cerca
                separationVector += oppositeDirection.normalized * weight * currentSeparationForce;
                nearbyUnits++;
            }
        }

        // Aplicar una fuerza mínima si hay unidades muy cercanas
        if (nearbyUnits > 0)
        {
            float minForce = currentSeparationForce * 0.5f;
            if (separationVector.magnitude < minForce)
            {
                separationVector = separationVector.normalized * minForce;
            }
        }

        return separationVector;
    }

    bool IsPathValid()
    {
        return m_CurrentPath.Count > 0 && m_CurrentNodeIndex < m_CurrentPath.Count;
    }
}