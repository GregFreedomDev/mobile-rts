using UnityEngine;

/// <summary>
/// Something a villager can be assigned to via the floating "assign worker" button:
/// a foundation (build it), a farm (tend/plant it), a tree (chop it), etc.
/// </summary>
public interface IWorkerAssignable
{
    /// <summary>Whether the assign button should currently be offered for this target.</summary>
    bool NeedsWorker { get; }

    /// <summary>Button caption (e.g. "Asignar trabajador", "Cortar leña").</summary>
    string AssignLabel { get; }

    /// <summary>World transform the button hovers under.</summary>
    Transform AnchorTransform { get; }

    /// <summary>Dispatch the given worker to this target's job.</summary>
    void AssignWorker(WorkerUnit worker);
}
