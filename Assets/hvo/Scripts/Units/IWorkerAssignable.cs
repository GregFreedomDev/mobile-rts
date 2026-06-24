using UnityEngine;

/// <summary>
/// Something a villager can be assigned to via the floating "assign worker" button:
/// a foundation (build it), a farm (tend/plant it), a tree (chop it), etc.
/// </summary>
public interface IWorkerAssignable
{
    /// <summary>Whether the "assign worker" button should be offered for this target.</summary>
    bool NeedsWorker { get; }

    /// <summary>Whether a worker is currently assigned and can be released ("Detener trabajo").</summary>
    bool HasWorker { get; }

    /// <summary>Assign-button caption (e.g. "Asignar trabajador", "Cortar leña").</summary>
    string AssignLabel { get; }

    /// <summary>World transform the button hovers under.</summary>
    Transform AnchorTransform { get; }

    /// <summary>Dispatch the given worker to this target's job.</summary>
    void AssignWorker(WorkerUnit worker);

    /// <summary>Un-assign the current worker (and send it walking off).</summary>
    void ReleaseWorker();
}
