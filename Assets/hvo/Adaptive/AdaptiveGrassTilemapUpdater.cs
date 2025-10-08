using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class AdaptativeGrassTilemapUpdater : MonoBehaviour
{
    [Header("Tilemap to Update")]
    public Tilemap targetTilemap;

    [Header("Sprite Folder in Resources")]
    public string folderName = "AdaptiveGrass";

    [Header("Prefix of tiles to replace (e.g. 'Tilemap_Flat_')")]
    public string tilePrefix = "Tilemap_Flat_";

    void Start()
    {
        SwapTileSprites();
    }

    void SwapTileSprites()
    {
        if (targetTilemap == null)
        {
            Debug.LogError("Tilemap is not assigned.");
            return;
        }

        // Obtener el sufijo por calidad: 64 / 128 / 256
        var tier = DeviceTierDetector.GetDeviceTier();
        string suffix = tier.ToSuffix();

        // Recorrer el tilemap
        BoundsInt bounds = targetTilemap.cellBounds;

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            Tile tile = targetTilemap.GetTile<Tile>(pos);

            if (tile != null && tile.sprite != null)
            {
                string originalName = tile.sprite.name;

                // Solo reemplazar si el nombre del sprite empieza con el prefijo esperado
                if (originalName.StartsWith(tilePrefix))
                {
                    // Construir nombre del sprite adaptativo (ej: grass_64)
                    string newSpriteName = $"grass_{suffix}";

                    Sprite newSprite = Resources.Load<Sprite>($"{folderName}/{newSpriteName}");

                    if (newSprite != null)
                    {
                        // Crear un nuevo tile con el sprite actualizado
                        Tile newTile = ScriptableObject.CreateInstance<Tile>();
                        newTile.sprite = newSprite;
                        newTile.colliderType = tile.colliderType;

                        targetTilemap.SetTile(pos, newTile);
                    }
                }
            }
        }
    }
}
