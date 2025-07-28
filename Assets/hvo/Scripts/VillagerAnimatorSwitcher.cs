using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class VillagerSpriteSheetSwapper : MonoBehaviour
{
    public Animator animator;
    public AnimationClip originalClip;
    public string clipNameToOverride = "Anim_Villager_idle";

    public Texture2D spriteSheet256;
    public Texture2D spriteSheet128;
    public Texture2D spriteSheet64;

    public enum SpriteSize { Size256, Size128, Size64 }
    public SpriteSize targetSize = SpriteSize.Size128;

    void Start()
    {
        Texture2D selectedSheet = targetSize switch
        {
            SpriteSize.Size64 => spriteSheet64,
            SpriteSize.Size128 => spriteSheet128,
            _ => spriteSheet256,
        };

        // Cargar los sprites del sprite sheet
        Sprite[] replacementSprites = LoadSpritesFromSheet(selectedSheet);

        // Duplicar el clip original
        AnimationClip runtimeClip = Instantiate(originalClip);
        runtimeClip.name = originalClip.name + "_" + targetSize;

        // Reemplazar sprites
        ReplaceSprites(runtimeClip, replacementSprites);

        // Aplicar override al Animator
        AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
        overrideController[clipNameToOverride] = runtimeClip;
        animator.runtimeAnimatorController = overrideController;
    }

    Sprite[] LoadSpritesFromSheet(Texture2D sheet)
    {
#if UNITY_EDITOR
        string path = AssetDatabase.GetAssetPath(sheet);
        Object[] assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

        List<Sprite> sprites = new List<Sprite>();
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
                sprites.Add(sprite);
        }

        sprites.Sort((a, b) => a.name.CompareTo(b.name)); // Importante para mantener el orden
        return sprites.ToArray();
#else
        Debug.LogError("LoadSpritesFromSheet only works in the Editor.");
        return null;
#endif
    }

    void ReplaceSprites(AnimationClip clip, Sprite[] newSprites)
    {
        var bindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);

        foreach (var binding in bindings)
        {
            if (binding.propertyName != "m_Sprite") continue;

            var keyframes = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < keyframes.Length && i < newSprites.Length; i++)
            {
                keyframes[i].value = newSprites[i];
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }
    }
}