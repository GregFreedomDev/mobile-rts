using UnityEngine;
using System.Collections.Generic;

public class VillagerSpriteSheetSwapper : MonoBehaviour
{
    [Header("Animator / Clip Base")]
    public Animator animator;
    public AnimationClip originalClip;
    public string clipNameToOverride = "Anim_Villager_idle";

    [Header("Sprite Sheets ( mEditor-only flow)")]
    public Texture2D spriteSheet256;
    public Texture2D spriteSheet128;
    public Texture2D spriteSheet64;

    [Header("Runtime Clips (Build flow)")]
    public AnimationClip clip256;
    public AnimationClip clip128;
    public AnimationClip clip64;

    public enum SpriteSize { Size256, Size128, Size64 }
    public SpriteSize targetSize = SpriteSize.Size128;

    void Start()
    {
        // En build: usa clips pre-horneados
#if !UNITY_EDITOR
        ApplyRuntimeOverrideForBuild();
#else
        // En Editor: mantiene el flujo original (leer sheet, duplicar clip, reemplazar sprites)
        ApplyEditorOverride();
#endif
    }

#if UNITY_EDITOR
    // ===========================
    // ====== EDITOR FLOW ========
    // ===========================
    void ApplyEditorOverride()
    {
        Texture2D selectedSheet = targetSize switch
        {
            SpriteSize.Size64  => spriteSheet64,
            SpriteSize.Size128 => spriteSheet128,
            _                  => spriteSheet256,
        };

        if (selectedSheet == null)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] No hay spriteSheet asignado para {targetSize}.");
            return;
        }

        // Cargar los sprites del sprite sheet (Editor)
        Sprite[] replacementSprites = LoadSpritesFromSheet(selectedSheet);
        if (replacementSprites == null || replacementSprites.Length == 0)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] No se pudieron cargar sprites para {targetSize}.");
            return;
        }

        // Duplicar el clip original
        if (originalClip == null)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] 'originalClip' no está asignado.");
            return;
        }

        AnimationClip runtimeClip = Instantiate(originalClip);
        runtimeClip.name = originalClip.name + "_" + targetSize;

        // Reemplazar sprites con AnimationUtility (Editor)
        ReplaceSprites(runtimeClip, replacementSprites);

        // Aplicar override al Animator
        ApplyOverride(animator, clipNameToOverride, runtimeClip);
    }

    Sprite[] LoadSpritesFromSheet(Texture2D sheet)
    {
        // Usamos UnityEditor SOLO dentro de bloques #if UNITY_EDITOR
        string path = UnityEditor.AssetDatabase.GetAssetPath(sheet);
        Object[] assets = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

        List<Sprite> sprites = new List<Sprite>();
        foreach (Object asset in assets)
        {
            if (asset is Sprite sprite)
                sprites.Add(sprite);
        }

        // Importante para mantener el orden
        sprites.Sort((a, b) => a.name.CompareTo(b.name));
        return sprites.ToArray();
    }

    void ReplaceSprites(AnimationClip clip, Sprite[] newSprites)
    {
        var bindings = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip);

        foreach (var binding in bindings)
        {
            if (binding.propertyName != "m_Sprite") continue;

            var keyframes = UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding);
            for (int i = 0; i < keyframes.Length && i < newSprites.Length; i++)
            {
                keyframes[i].value = newSprites[i];
            }

            UnityEditor.AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);
        }
    }
#endif

    // ===========================
    // ====== BUILD FLOW =========
    // ===========================
    void ApplyRuntimeOverrideForBuild()
    {
        AnimationClip selectedClip = targetSize switch
        {
            SpriteSize.Size64  => clip64,
            SpriteSize.Size128 => clip128,
            _                  => clip256,
        };

        if (selectedClip == null)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] En build necesitas asignar el clip {targetSize} en 'Runtime Clips'.");
            return;
        }

        ApplyOverride(animator, clipNameToOverride, selectedClip);
    }

    // ===========================
    // ====== COMMON UTILS =======
    // ===========================
    static void ApplyOverride(Animator anim, string clipToOverrideName, AnimationClip newClip)
    {
        if (anim == null)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] 'animator' no está asignado.");
            return;
        }

        var baseController = anim.runtimeAnimatorController;
        if (baseController == null)
        {
            Debug.LogError($"[{nameof(VillagerSpriteSheetSwapper)}] El Animator no tiene un RuntimeAnimatorController.");
            return;
        }

        var overrideController = new AnimatorOverrideController(baseController);
        overrideController[clipToOverrideName] = newClip;
        anim.runtimeAnimatorController = overrideController;
    }
}
