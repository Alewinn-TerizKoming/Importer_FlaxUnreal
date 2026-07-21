using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;
using FlaxEngine.Json;

namespace Unreal_ImporterEditor;

/// <summary>
/// SceneReader class.
/// </summary>
public class SceneReader
{
    public Scene Read(string path)
    {
        string json = File.ReadAllText(path);

        Scene scene = JsonSerializer.Deserialize<Scene>(json);

        Debug.Log($"Loaded {scene.StaticMeshes.Count} meshes");
        Debug.Log($"Loaded {scene.MeshInstances.Count} instances");

        return scene;
    }
}
