using FlaxEditor;
using FlaxEngine;
using MaterialLab;
using System;
using System.IO;
using System.Threading.Tasks;
using Unreal_ImporterEditor;

public class MaterialGenerator
{
public async Task<string> GenerateAsync(
    Unreal_ImporterEditor.MaterialSlot slot,
    AssetImportResult result)
{
    string path;

    switch (result.scene.From)
    {
        case "Unreal":
            path = FlaxPaths.GetUnrealAssetPath(
                slot.AssetPath,
                ".flax");
            break;

        case "Blender":
            path = FlaxPaths.GetMaterialPath(
                result.scene.From,
                result.scene.SceneName,
                slot.Name);
            break;

        default:
            throw new Exception(
                $"Unknown scene source '{result.scene.From}'.");
    }

    if (File.Exists(path))
        return path;

        await CreateAssetAsync(
            slot,
            path,
            result);

    return path;
}

    private async Task CreateAssetAsync(Unreal_ImporterEditor.MaterialSlot slot, string path, AssetImportResult result)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (string.IsNullOrEmpty(slot.ParentMaterial))
        {
            CreateMaterial(path, slot.Color);
        }
        else
        {
            await CreateMaterialInstance(
                        slot,
                        path,
                        slot.Color,
                        result);
        }

        await Task.Delay(100);
    }

    private void CreateMaterial(string path, string color)
    {
        bool failed = Editor.CreateAsset("Material",path);

        Material material = Content.Load<Material>(path);
        material.WaitForLoaded();

        MatGraphTools.SetColor(material, ParseColor(color));

        if (failed)
            throw new Exception($"Unable to create material {path}");
    }

    private async Task CreateMaterialInstance(
        Unreal_ImporterEditor.MaterialSlot slot,
        string path,
        string color,
        AssetImportResult result)
    {
        bool failed =
            Editor.CreateAsset(
                "MaterialInstance",
                path);

        if (failed)
            throw new Exception(
                $"Unable to create material instance {path}");

        string parentPath;

        switch (result.scene.From)
        {
            case "Unreal":
                parentPath =
                    FlaxPaths.GetUnrealAssetPath(
                        slot.ParentMaterial,
                        ".flax");
                break;

            case "Blender":
                parentPath =
                    FlaxPaths.GetMaterialPath(
                        result.scene.From,
                        result.scene.SceneName,
                        slot.ParentMaterial);
                break;

            default:
                throw new Exception(
                    $"Unknown scene source '{result.scene.From}'.");
        }

        MaterialInstance instance =
            Content.Load<MaterialInstance>(path);

        instance.WaitForLoaded();

        MaterialBase parent =
            Content.Load<MaterialBase>(parentPath);

        parent.WaitForLoaded();

        instance.BaseMaterial = parent;
        instance.SetParameterValue(
            "BaseColor",
            ParseColor(color));

        instance.Save();

        await Task.CompletedTask;
    }

    private Color ParseColor(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex))
            return Color.White;

        hex = hex.TrimStart('#');

        if (hex.Length != 8)
            return Color.White;

        byte r = Convert.ToByte(hex.Substring(0, 2), 16);
        byte g = Convert.ToByte(hex.Substring(2, 2), 16);
        byte b = Convert.ToByte(hex.Substring(4, 2), 16);
        byte a = Convert.ToByte(hex.Substring(6, 2), 16);

        return new Color(r, g, b, a);
    }
}