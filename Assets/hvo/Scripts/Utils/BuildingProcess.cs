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

    private bool InProgress => HasActiveWorker && m_Worker.CurrentState == UnitState.Building;
    public bool HasActiveWorker => m_Worker != null;

    public BuildingProcess(
        BuildActionSO buildAction,
        Vector3 placementPosition,
        WorkerUnit worker,
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

        worker.SendToBuild(m_Structure);
        m_Worker = worker;
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

    public void SetFinishTime(DateTime finishTime)
    {
        m_FinishTime = finishTime;
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
            m_Worker.OnBuildingFinished();
        }

        m_Structure.OnConstructionFinished();
        m_ConstructionEffect.Stop();
    }

    public void AddWorker(WorkerUnit worker)
    {
        if (HasActiveWorker) return;
        m_Worker = worker;
    }

    public void RemoveWorker()
    {
        if (!HasActiveWorker) return;
        m_Worker = null;
        m_ConstructionEffect.Stop();
    }
}
