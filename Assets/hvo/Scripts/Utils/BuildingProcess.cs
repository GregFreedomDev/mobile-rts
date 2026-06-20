using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

public class BuildingProcess
{
    private BuildActionSO m_BuildAction;
    private WorkerUnit m_Worker;
    private StructureUnit m_Structure;
    private ParticleSystem m_ConstructionEffect;
    private float m_ProgressTimer;
    private bool m_IsFinished;
    private float m_BuildSoundFrequency = 0.7f;
    private float m_LastBuildSoundTime;

    private DateTime m_FinishTime;
    private bool m_MidSpriteApplied = false;
    private bool m_CountdownStarted;

    private WorkerUnit m_PendingWorker; // dispatched to build but not yet arrived

    private bool InProgress => HasActiveWorker && m_Worker.CurrentState == UnitState.Building;
    public bool HasActiveWorker => m_Worker != null;
    public StructureUnit Structure => m_Structure;

    // True while a worker is actively building OR is on its way here to build it.
    public bool HasAssignedBuilder
    {
        get
        {
            if (m_Worker != null) return true;
            if (m_PendingWorker == null) return false;
            return m_PendingWorker.CurrentState != UnitState.Dead
                && m_PendingWorker.CurrentTask == UnitTask.Build
                && m_PendingWorker.Target == (Unit)m_Structure;
        }
    }

    public void AssignBuilder(WorkerUnit worker)
    {
        m_PendingWorker = worker;
        worker.SendToBuild(m_Structure);
    }

    // Places the building as a foundation (no worker). Construction only starts once a worker is
    // assigned to it (see AddWorker), so the player can place first and assign a builder later.
    public BuildingProcess(
        BuildActionSO buildAction,
        Vector3 placementPosition,
        ParticleSystem constructionEffectPrefab
    )
    {
        m_BuildAction = buildAction;

        var effectOffset = new Vector3(0, -1f, 0);
        m_ConstructionEffect = Object.Instantiate(
            constructionEffectPrefab,
            placementPosition + effectOffset,
            Quaternion.identity
        );

        m_Structure = Object.Instantiate(buildAction.StructurePrefab);
        m_Structure.Renderer.sprite = m_BuildAction.FoundationSprite;
        m_Structure.transform.position = placementPosition;
        m_Structure.RegisterProcess(this);
    }

    public void Update()
    {
        if (m_IsFinished) return;

        if (InProgress)
        {
            m_ProgressTimer += Time.deltaTime;

            if (!m_ConstructionEffect.isPlaying)
            {
                m_ConstructionEffect.Play();
            }

            if (Time.time >= m_LastBuildSoundTime + m_BuildSoundFrequency)
            {
                m_Worker.PlayBuildSound();
                m_LastBuildSoundTime = Time.time;
            }
        }
    }

    // Starts the timed construction once. Called when the first worker begins building.
    private void StartCountdown()
    {
        if (m_CountdownStarted) return;
        m_CountdownStarted = true;
        m_FinishTime = TimeAPIHelper.TrustedUtcNow.AddSeconds(m_BuildAction.ConstructionTime);
        CoroutineRunner.Instance.StartCoroutine(BuildingCountdown());
    }

    private IEnumerator BuildingCountdown()
    {
        double totalDuration = (m_FinishTime - DateTime.UtcNow).TotalSeconds;
        if (totalDuration <= 0)
        {
            Debug.LogWarning("Duración inválida, completando de inmediato");
            FinalizeConstruction();
            yield break;
        }

        double midPoint = totalDuration / 2.0;

        while (true)
        {
            var remaining = m_FinishTime - DateTime.UtcNow;
            double elapsed = totalDuration - remaining.TotalSeconds;

            if (!m_MidSpriteApplied && elapsed >= midPoint)
            {
                Debug.Log("Cambiando a sprite intermedio");
                if (m_BuildAction.MidConstructionSprite != null)
                {
                    m_Structure.Renderer.sprite = m_BuildAction.MidConstructionSprite;
                }
                m_MidSpriteApplied = true;
            }

            if (remaining.TotalSeconds > 0)
            {
                Debug.Log($"Faltan: {remaining.TotalSeconds:F0} segundos");
            }
            else
            {
                Debug.Log("¡Construcción completada!");
                FinalizeConstruction();
                break;
            }
            yield return new WaitForSeconds(1);
        }
    }

    private void FinalizeConstruction()
    {
        if (m_IsFinished) return;

        m_IsFinished = true;
        m_Structure.Renderer.sprite = m_BuildAction.CompletionSprite;

        if (HasActiveWorker)
        {
            // Send the worker to a clear spot below the building so it doesn't stand on it.
            float down = (m_Structure.Collider != null ? m_Structure.Collider.bounds.extents.y : 1f) + 0.8f;
            Vector3 stepAside = m_Structure.transform.position + new Vector3(0f, -down, 0f);
            m_Worker.OnBuildingFinished(stepAside);
        }

        m_Structure.OnConstructionFinished();
        m_ConstructionEffect.Stop();
    }

    public void AddWorker(WorkerUnit worker)
    {
        if (HasActiveWorker) return;
        m_Worker = worker;
        StartCountdown(); // a builder arrived — begin the timed construction
    }

    public void RemoveWorker()
    {
        if (!HasActiveWorker) return;
        m_Worker = null;
        m_ConstructionEffect.Stop();
    }
}
