using UnityEngine;
using UnityEngine.Tilemaps;

public class AdaptiveGrassTilemapUpdater : MonoBehaviour
{
    [Header("Tilemap to Update")]
    public Tilemap targetTilemap;

    [Header("Grass Sprite Base Name (without _64/_128/_256)")]
    public string grassBaseName = "grass";

    void Start()
    {
        UpdateGrassTiles();
    }

    public void UpdateGrassTiles()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("No Tilemap assigned to AdaptiveGrassTilemapUpdater.");
            return;
        }

        Sprite grassSprite = AdaptiveSpriteLoader.Load(grassBaseName);

        // Loop through all positions in the tilemap's bounds
        BoundsInt bounds = targetTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            TileBase tile = targetTilemap.GetTile(pos);
            if (tile != null)
            {
                // Create a new tile with the correct sprite
                Tile newTile = ScriptableObject.CreateInstance<Tile>();
                newTile.sprite = grassSprite;
                targetTilemap.SetTile(pos, newTile);
            }
        }
    }
} 