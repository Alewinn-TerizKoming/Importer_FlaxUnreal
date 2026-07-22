using FlaxEditor;
using FlaxEngine;
using FlaxEngine.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;


namespace Unreal_ImporterEditor;

/// <summary>
/// AssetImporter class.
/// </summary>
public class AssetImporter
{
    private readonly MaterialGenerator _materialGenerator = new();

    public async Task<AssetImportResult> ImportAsync(Scene scene, string sceneFilename)
    {
        string sceneDirectory = Path.GetDirectoryName(sceneFilename)!;
        string sceneName = scene.SceneName;

        AssetImportResult result = new(sceneName, scene);

        await GenerateMaterialsAsync(scene, result);

        foreach (StaticMesh mesh in scene.StaticMeshes)
        {
            string sourceFilename = Path.Combine(sceneDirectory, mesh.Name + ".fbx");

            await ImportModelAsync(sourceFilename,mesh,result);
        }

        await GeneratePrefabsAsync(scene, result);

        return result;
    }

    private async Task ImportModelAsync(string sourceFilename,StaticMesh mesh,AssetImportResult result)
    {
        string destinationFilename = FlaxPaths.GetUnrealMeshPath(mesh.AssetPath);

        string directory = Path.GetDirectoryName(destinationFilename)!;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        ModelTool.Options options = new ModelTool.Options();

        options.Scale = 1;
        options.CollisionType = CollisionDataType.ConvexMesh;
        options.ImportVertexColors = true;
        options.SplitObjects = true;

        bool failed =
            Editor.Import(
                FlaxPaths.NormalizeImportPath(sourceFilename),
                FlaxPaths.NormalizeImportPath(destinationFilename),
                options);

        await Task.Delay(200);

        if (failed)
        {
            Debug.LogError(
                $"Unable to import {sourceFilename}");

            return;
        }

        result.RegisterMesh(mesh.Name,destinationFilename);

        await ApplyDefaultMaterialsAsync(mesh, destinationFilename, result.scene);

        await ProcessCollisionsAsync(Path.GetDirectoryName(sourceFilename)!,mesh,destinationFilename,result);
    }

    private async Task ProcessCollisionsAsync(string sceneDirectory, StaticMesh mesh, string visualAssetPath, AssetImportResult result)
    {
        string meshName =  mesh.Name;

        string meshFolder = Path.GetDirectoryName(visualAssetPath)!;

        // 1. Chercher tous les UCX générés par Flax
        var collisionFiles = Directory.EnumerateFiles(meshFolder, "*.flax")
            .Where(f =>
            {
                string name = Path.GetFileNameWithoutExtension(f);

                if(name.Contains(
                    $"UCX_{meshName}",
                    StringComparison.OrdinalIgnoreCase) && !name.EndsWith("_ColData",StringComparison.OrdinalIgnoreCase))
                    return name.Contains(
                        $"UCX_{meshName}",
                        StringComparison.OrdinalIgnoreCase);
                else return false;
            })
            .ToList();

        if (collisionFiles.Count == 0)
            return;

        foreach (var collisionFile in collisionFiles)
        {
            Model Model = Content.LoadAsync<Model>(collisionFile);
            while (!Model.IsLoaded)
                await Task.Delay(10);

            await Task.Delay(50);

            if (Model == null)
            {
                Debug.LogError($"Unable to load model : {collisionFile}");
                continue;
            }

            string collisionDataPath = collisionFile.Replace(".flax", "_ColData.flax");

            bool failed = Editor.CookMeshCollision(
                collisionDataPath,
                CollisionDataType.TriangleMesh,
                Model);

            CollisionData GeneratedColl = Content.LoadAsync<CollisionData>(collisionDataPath);
            GeneratedColl.WaitForLoaded();

            if (failed)
            {
                Debug.LogError($"Collision cook failed : {collisionFile}");
                continue;
            }

            result.RegisterCollision(meshName, collisionDataPath);
        }
    }

    private async Task GeneratePrefabsAsync(Scene scene,AssetImportResult result)
    {
        foreach (StaticMesh mesh in scene.StaticMeshes)
        {
            await GeneratePrefabAsync(mesh, result);
        }
    }

    private async Task GeneratePrefabAsync(StaticMesh mesh,AssetImportResult result)
    {
        var collisions = result.GetCollisions(mesh.Name);

        if (collisions.Count == 0)
            return;

        // string prefabPath = FlaxPaths.GetPrefabPath(result.SceneName, mesh.Name);
        string prefabPath = FlaxPaths.GetUnrealPrefabPath(mesh.AssetPath);

        Prefab ExistingPfb = Content.Load<Prefab>(prefabPath);
        if (ExistingPfb == null)
        {
            string modelPath = result.GetImportedMesh(mesh.Name);

            Model model = Content.Load<Model>(modelPath);

            while (!model.IsLoaded) await Task.Delay(10);

            await Task.Delay(100);

            StaticModel visual = new StaticModel();
            visual.Model = model;

            foreach (string collisionPath in collisions)
            {
                MeshCollider collider = new MeshCollider();

                collider.CollisionData = Content.Load<CollisionData>(collisionPath);

                while (!collider.CollisionData.IsLoaded) await Task.Delay(10);

                collider.Parent = visual;
            }

            PrefabManager.CreatePrefab(visual, prefabPath, false);
        }
        else Debug.Log($"  ===========> Prefab already exist, skip creation : {prefabPath}");

        result.RegisterPrefab(mesh.Name, prefabPath);
    }

    private async Task GenerateMaterialsAsync(Scene scene, AssetImportResult result)
    {
        foreach (MaterialSlot slot in scene.Materials)
        {
            string flaxPath = await _materialGenerator.GenerateAsync(slot);
            result.RegisterMaterial(slot.AssetPath, flaxPath);
        }
    }

    private async Task ApplyDefaultMaterialsAsync(StaticMesh mesh,string modelPath,Scene scene)
    {
        Model model = Content.Load<Model>(modelPath);

        if (model == null)
        {
            Debug.LogError($"Unable to load model {modelPath}");

            return;
        }

        while (!model.IsLoaded) await Task.Delay(10);

        int meshIndex = scene.StaticMeshes.FindIndex(x => x.AssetPath == mesh.AssetPath);

        List<MaterialSlot> materials = GetDefaultMaterials(scene, meshIndex);

        for (int i = 0; i < materials.Count; i++)
        {
            MaterialSlot slot = materials[i];

            string materialPath = await _materialGenerator.GenerateAsync(slot);

            MaterialBase material = Content.Load<MaterialBase>(materialPath);

            if (material == null)
            {
                Debug.LogWarning( $"Unable to load material {materialPath}");
                continue;
            }

            if (i < model.MaterialSlots.Length)
            {
                model.MaterialSlots[i].Material = material;
            }
        }

        model.Save();

        Model test = Content.Load<Model>(modelPath);

        while (!test.IsLoaded) await Task.Delay(10);

        for (int i = 0; i < test.MaterialSlots.Length; i++)
        {
            Debug.Log( $"Slot {i}: {test.MaterialSlots[i].Material}");
        }
    }

    private List<MaterialSlot> GetDefaultMaterials(Scene scene, int meshIndex)
    {
        MeshInstance? instance = scene.MeshInstances.FirstOrDefault(x => x.Mesh == meshIndex);

        if (instance == null)
        {
            return new List<MaterialSlot>();
        }

        List<MaterialSlot> materials = new();

        if (instance.Value.Materials == null)
        {
            return materials;
        }

        foreach (int materialIndex in instance.Value.Materials)
        {
            if (materialIndex < 0 || materialIndex >= scene.Materials.Count)
            {
                continue;
            }

            materials.Add(scene.Materials[materialIndex]);
        }

        return materials;
    }
}