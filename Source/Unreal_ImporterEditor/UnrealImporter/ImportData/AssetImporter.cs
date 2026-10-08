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

        ModelTool.Options options = CreateImportOptions(scene.From);

        await GenerateMaterialsAsync(scene, result);

        foreach (StaticMesh mesh in scene.StaticMeshes)
        {
            string sourceFilename = Path.Combine(sceneDirectory, mesh.Name + ".fbx");

            await ImportModelAsync(sourceFilename,mesh,result,options);
        }

        await GeneratePrefabsAsync(scene, result);

        return result;
    }

    private ModelTool.Options CreateImportOptions(string from)
    {
        ModelTool.Options options = new();

        options.Scale = 1;
        options.ImportVertexColors = true;
        options.SplitObjects = true;
        options.CollisionType = CollisionDataType.ConvexMesh;

        switch (from)
        {
            case "Blender":
                options.UseLocalOrigin = true;
                options.Rotation = Quaternion.Euler(0, -90, 0);
                break;

            case "Unreal":
                break;

            default:
                Debug.LogWarning(
                    $"Unknown scene source '{from}'. " +
                    "Using default import options.");
                break;
        }

        return options;
    }

    private async Task ImportModelAsync(
        string sourceFilename,
        StaticMesh mesh,
        AssetImportResult result,
        ModelTool.Options options)
    {
        //string destinationFilename = FlaxPaths.GetUnrealMeshPath(mesh.AssetPath);

        string destinationFilename = FlaxPaths.GetMeshPath(
            result.scene.From,
            result.scene.SceneName,
            mesh.Name);

        string directory = Path.GetDirectoryName(destinationFilename)!;

        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

        //ModelTool.Options options = new ModelTool.Options();

        //options.Scale = 1;
        //options.CollisionType = CollisionDataType.ConvexMesh;
        //options.ImportVertexColors = true;
        //options.SplitObjects = true;

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

        await ApplyDefaultMaterialsAsync(mesh, destinationFilename, result.scene, result);

        await ProcessCollisionsAsync(Path.GetDirectoryName(sourceFilename)!,mesh,destinationFilename,result);
    }

    private async Task ProcessCollisionsAsync(string sceneDirectory,StaticMesh mesh,string visualAssetPath,AssetImportResult result)
    {
        switch (result.scene.From)
        {
            case "Unreal":
                await ProcessUnrealCollisionAsync(
                    sceneDirectory,
                    mesh,
                    visualAssetPath,
                    result);
                break;

            case "Blender":
                await ProcessBlenderCollisionAsync(
                    sceneDirectory,
                    mesh,
                    visualAssetPath,
                    result);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown scene source '{result.scene.From}'. " +
                    $"No collision processing performed.");
                break;
        }
    }

    private async Task ProcessUnrealCollisionAsync(string sceneDirectory, StaticMesh mesh, string visualAssetPath, AssetImportResult result)
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

    private async Task ProcessBlenderCollisionAsync(
    string sceneDirectory,
    StaticMesh mesh,
    string visualAssetPath,
    AssetImportResult result)
    {
        string collisionSourceFilename = Path.Combine(
            sceneDirectory,
            mesh.Name + "_COL.fbx");

        if (!File.Exists(collisionSourceFilename))
            return;

        string collisionFolder = Path.Combine(
            Path.GetDirectoryName(visualAssetPath)!,
            "..",
            "Collisions");

        collisionFolder = Path.GetFullPath(collisionFolder);

        if (!Directory.Exists(collisionFolder))
            Directory.CreateDirectory(collisionFolder);

        string collisionDestinationFilename = Path.Combine(
            collisionFolder,
            mesh.Name + "_COL.flax");

        ModelTool.Options options = new();
        options.Scale = 1;
        options.UseLocalOrigin = true;
        options.Rotation = Quaternion.Euler(0, -90, 0);
        options.ImportVertexColors = false;
        options.SplitObjects = true;
        options.CollisionType = CollisionDataType.ConvexMesh;

        bool failed = Editor.Import(
            FlaxPaths.NormalizeImportPath(collisionSourceFilename),
            FlaxPaths.NormalizeImportPath(collisionDestinationFilename),
            options);

        await Task.Delay(200);

        if (failed)
        {
            Debug.LogError(
                $"Unable to import Blender collision {collisionSourceFilename}");
            return;
        }

        Model collisionModel =
            Content.LoadAsync<Model>(collisionDestinationFilename);

        while (!collisionModel.IsLoaded)
            await Task.Delay(10);

        await Task.Delay(50);

        if (collisionModel == null)
        {
            Debug.LogError(
                $"Unable to load collision model {collisionDestinationFilename}");
            return;
        }

        string collisionDataPath = Path.Combine(
            collisionFolder,
            mesh.Name + "_COL_ColData.flax");

        failed = Editor.CookMeshCollision(
            collisionDataPath,
            CollisionDataType.TriangleMesh,
            collisionModel);

        if (failed)
        {
            Debug.LogError(
                $"Collision cook failed : {collisionSourceFilename}");
            return;
        }

        CollisionData collisionData =
            Content.LoadAsync<CollisionData>(collisionDataPath);

        collisionData.WaitForLoaded();

        result.RegisterCollision(
            mesh.Name,
            collisionDataPath);
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
        string prefabPath;

        switch (result.scene.From)
        {
            case "Unreal":
                prefabPath = FlaxPaths.GetUnrealPrefabPath(mesh.AssetPath);
                break;

            case "Blender":
                prefabPath = FlaxPaths.GetPrefabPath(
                    result.scene.From,
                    result.scene.SceneName,
                    mesh.Name);
                break;

            default:
                Debug.LogWarning(
                    $"Unknown scene source '{result.scene.From}'. " +
                    "Unable to determine prefab path.");
                return;
        }

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
            string flaxPath = await _materialGenerator.GenerateAsync(slot, result);
            result.RegisterMaterial(slot.AssetPath, flaxPath);
        }
    }

    private async Task ApplyDefaultMaterialsAsync(StaticMesh mesh,string modelPath,Scene scene, AssetImportResult result)
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

            string materialPath = await _materialGenerator.GenerateAsync(slot, result);

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

        if (instance.Materials == null)
        {
            return materials;
        }

        foreach (int materialIndex in instance.Materials)
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