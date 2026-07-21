using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;

namespace Unreal_ImporterEditor;

public class AssetImportResult
{
    public string SceneName { get; }

    public Scene scene { get; }


    private readonly Dictionary<string, string> _meshAssets = new();
    private readonly Dictionary<string, List<string>> _collisionAssets = new();
    private readonly Dictionary<string, string> _prefabAssets = new();
    private Dictionary<string, string> _materials = new();

    public AssetImportResult(string sceneName, Scene scene  )
    {
        SceneName = sceneName;
        this.scene = scene;
    }

    public void RegisterMesh(string meshName, string assetPath)
    {
        _meshAssets[meshName] = assetPath;
    }

   public string GetImportedMesh(string meshName)
    {
        return _meshAssets[meshName];
    }

    public void RegisterCollision(string meshName, string collisionAssetPath)
    {
        if (!_collisionAssets.TryGetValue(meshName, out var list))
        {
            list = new List<string>();
            _collisionAssets[meshName] = list;
        }

        list.Add(collisionAssetPath);
    }

    public IReadOnlyList<string> GetCollisions(string meshName)
    {
        return _collisionAssets.TryGetValue(meshName, out var list)
            ? list : Array.Empty<string>();
    }

    public void RegisterPrefab(string meshName, string prefabPath)
    {
        _prefabAssets[meshName] = prefabPath;
    }

    public bool HasPrefab(string meshName)
    {
        return _prefabAssets.ContainsKey(meshName);
    }

    public string GetPrefab(string meshName)
    {
        return _prefabAssets[meshName];
    }

    public void RegisterMaterial(string unrealPath, string flaxPath)
    {
        _materials[unrealPath] = flaxPath;
    }

    public string? GetMaterial(string unrealPath)
    {
        return _materials.TryGetValue(unrealPath, out var path)
            ? path
            : null;
    }
}