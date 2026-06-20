
using UnityEngine;

[CreateAssetMenu(fileName = "BuildAction", menuName = "HvO/Actions/BuildAction")]
public class BuildActionSO: ActionSO
{
    [SerializeField] private StructureUnit m_StructurePrefab;
    [SerializeField] private float m_ConstructionTime;
    [SerializeField] private Sprite m_PlacementSprite;
    [SerializeField] private Sprite m_FoundationSprite;
    [SerializeField] private Sprite m_CompletionSprite;

    [SerializeField] private Vector3Int m_BuildingSize;
    [SerializeField] private Vector3Int m_OriginOffset;

    // Shifts the ghost sprite and final position without moving the tile footprint. Use (0.5, 0.5)
    // to center an odd (e.g. 1x1) building's sprite on its cell. Default (0,0) = no change.
    [SerializeField] private Vector2 m_PlacementVisualOffset;

    [SerializeField] private int m_GoldCost;
    [SerializeField] private int m_WoodCost;

    [TextArea] [SerializeField] private string m_Description; // shown in the build catalog info popup

    [SerializeField] private Sprite m_MidConstructionSprite;
    public Sprite MidConstructionSprite => m_MidConstructionSprite;
    public StructureUnit StructurePrefab => m_StructurePrefab;
    public float ConstructionTime => m_ConstructionTime;
    public Sprite PlacementSprite => m_PlacementSprite;
    public Sprite FoundationSprite => m_FoundationSprite;
    public Sprite CompletionSprite => m_CompletionSprite;

    public Vector3Int BuildingSize => m_BuildingSize;
    public Vector3Int OriginOffset => m_OriginOffset;
    public Vector2 PlacementVisualOffset => m_PlacementVisualOffset;

    public int GoldCost => m_GoldCost;
    public int WoodCost => m_WoodCost;
    public string Description => m_Description;


    public override void Execute(GameManager gameManager)
    {
        gameManager.StartBuildProcess(this);
    }
}
