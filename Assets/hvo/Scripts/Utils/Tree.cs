

using UnityEngine;

public class Tree: MonoBehaviour, IWorkerAssignable
{
    [SerializeField] private CapsuleCollider2D m_Collider;
    [SerializeField] private Animator m_Animator;

    public bool m_Claimed = false;
    public bool Claimed => m_Claimed;

    // IWorkerAssignable — assign a villager to chop this tree.
    public Transform AnchorTransform => transform;
    public string AssignLabel => "Cortar leña";
    public bool NeedsWorker => !m_Claimed;
    public void AssignWorker(WorkerUnit worker) => worker.SendToChop(this);

    public bool TryToClaim()
    {
        if (!m_Claimed)
        {
            m_Claimed = true;
            return true;
        }

        return false;
    }

    public void Release()
    {
        m_Claimed = false;
    }

    public void Hit()
    {
        m_Animator.SetTrigger("Hit");
    }

    public Vector3 GetBottomPosition()
    {
        return m_Collider.bounds.min;
    }

}
