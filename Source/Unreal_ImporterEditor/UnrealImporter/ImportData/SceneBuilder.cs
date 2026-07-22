using FlaxEditor;
using FlaxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Unreal_ImporterEditor;

public class SceneBuilder
{
    public async Task Build(AssetImportResult assets)
    {
        Dictionary<string, EmptyActor> folders = new();

        Level.UnloadAllScenes();

        Editor.Instance.Scene.CloseAllScenes();

        Editor.Instance.ContentDatabase.Rebuild(true);

        string scenePath = Path.Combine(
            Globals.ProjectFolder,
            "Content",
            "Scenes",
            assets.SceneName + ".scene");

        var scene = new FlaxEngine.Scene();

        foreach (MeshInstance instance in assets.scene.MeshInstances)
        {
            StaticMesh mesh = assets.scene.StaticMeshes[instance.Mesh];

            Actor actor;

            Debug.Log($"Instance name => {instance.Name}");
            Debug.Log($"Mesh name => {mesh.Name}");

            if (assets.HasPrefab(mesh.Name))
            {

                Prefab prefab = Content.Load<Prefab>(assets.GetPrefab(mesh.Name));

                actor = PrefabManager.SpawnPrefab(prefab);

                StaticModel staticModel = actor as StaticModel;

                if (staticModel == null)
                {
                    staticModel = actor.GetChild<StaticModel>();
                }

                if (staticModel == null)
                    Debug.LogError("==========> Can't find mesh in prefab...");

                actor = staticModel;
            }
            else
            {

                Model model = Content.Load<Model>(assets.GetImportedMesh(mesh.Name));

                StaticModel staticModel = scene.AddChild<StaticModel>();

                staticModel.Model = model;

                actor = staticModel;

            }

            SetActorParent(actor, instance, scene, folders);
            actor.Name = instance.Name;

            ApplyTransform(actor, instance.Location, instance.Rotation, instance.Scale);

            ApplyProperties(actor, instance.Properties);

            ApplyMaterials(actor, instance, assets);
        }
        DirectionalLight light = scene.AddChild<DirectionalLight>();
        light.EulerAngles = new Float3(65, -100, 0);

        SkyLight sklght = scene.AddChild<SkyLight>();
        sklght.AdditiveColor = new Color([0.25f, 0.25f, 0.25f, 1]);

        scene.AddChild<Sky>();


        byte[] bytes = Level.SaveSceneToBytes(scene);

        if (bytes == null || bytes.Length == 0)
        {
            Debug.LogError("Scene serialization failed");
            return;
        }

        string sceneDirectory = Path.GetDirectoryName(scenePath);

        Directory.CreateDirectory(sceneDirectory);

        File.WriteAllBytes(scenePath, bytes);

        Debug.Log("Scene written.");

        FlaxEngine.Object.Destroy(scene);       // <= Avoid spamming log with already registered assets from the in-memory scene

        Editor.Instance.ContentDatabase.Rebuild(true);
    }

    private void ApplyTransform(Actor actor, Vector3 unrealPosition, Rotation unrealEuler, Vector3 unrealScale)
    {
        actor.ResetLocalTransform();

        actor.LocalPosition = new FlaxEngine.Vector3(
            unrealPosition.Y,
            unrealPosition.Z,
            unrealPosition.X
        );

        Rotation FlaxRotation = new Rotation();
        FlaxRotation.Pitch = -unrealEuler.Pitch;
        FlaxRotation.Roll = unrealEuler.Yaw; 
        FlaxRotation.Yaw = -unrealEuler.Roll; 

        actor.Orientation =  Quaternion.Euler(FlaxRotation.Pitch,FlaxRotation.Roll,FlaxRotation.Yaw);

        actor.LocalScale = new FlaxEngine.Vector3(
                unrealScale.Y,
                unrealScale.Z,
                unrealScale.X);
    }

    private void ApplyProperties(Actor actor, ActorProperties properties)
    {
        // Visibility / HiddenInGame
        actor.IsActive =
            properties.Visibility &&
            !properties.HiddenInGame;

        ApplyShadowProperties(actor, properties);
    }

    private void ApplyShadowProperties(
        Actor actor,
        ActorProperties properties)
    {
        if (properties.CastShadow)
            return;

        StaticModel staticModel = actor as StaticModel;

        if (staticModel == null)
            return;

        for (int i = 0; i < staticModel.MaterialSlots.Length; i++)
        {
            FlaxEngine.MaterialSlot slot = staticModel.MaterialSlots[i];

            slot.ShadowsMode = ShadowsCastingMode.None;

            staticModel.MaterialSlots[i] = slot;
        }
    }

    private EmptyActor GetOrCreateFolder(
    string folderPath,
    FlaxEngine.Scene scene,
    Dictionary<string, EmptyActor> folders)
    {
        if (string.IsNullOrEmpty(folderPath))
            return null;

        if (folders.TryGetValue(folderPath, out EmptyActor existing))
            return existing;


        string[] parts = folderPath.Split('/');

        string currentPath = "";
        Actor parent = scene;

        foreach (string part in parts)
        {
            if (string.IsNullOrEmpty(part))
                continue;

            if (!string.IsNullOrEmpty(currentPath))
                currentPath += "/";

            currentPath += part;


            if (!folders.TryGetValue(currentPath, out EmptyActor folder))
            {
                folder = new EmptyActor
                {
                    Name = part
                };

                folder.Parent = parent;

                folders.Add(currentPath, folder);
            }

            parent = folders[currentPath];
        }

        return folders[folderPath];
    }

    private void SetActorParent(
    Actor actor,
    MeshInstance instance,
    FlaxEngine.Scene scene,
    Dictionary<string, EmptyActor> folders)
    {
        EmptyActor folder = GetOrCreateFolder(
            instance.Folder,
            scene,
            folders);

        if (folder == null) actor.Parent = scene;
        else actor.Parent = folder;
    }

    private void ApplyMaterials(Actor actor, MeshInstance instance, AssetImportResult assets)
    {
        StaticModel staticModel = actor as StaticModel;

        if (staticModel == null)
        {
            staticModel = actor.GetChild<StaticModel>();
        }

        if (staticModel == null) return;

        if (instance.Materials == null) return;

        int count = Math.Min(instance.Materials.Count, staticModel.MaterialSlots.Length);

        for (int i = 0; i < count; i++)
        {
            int materialIndex = instance.Materials[i];

            if (materialIndex < 0 || materialIndex >= assets.scene.Materials.Count)
            {
                continue;
            }

            MaterialSlot slot = assets.scene.Materials[materialIndex];

            string materialPath = assets.GetMaterial(slot.AssetPath);
            Debug.Log($"=========> Material path : {materialPath}");

            MaterialBase material = Content.Load<MaterialBase>(materialPath);
            material.WaitForLoaded();

            if (material == null)
            {
                Debug.LogWarning($"Unable to load material {materialPath}");
                continue;
            }

            staticModel.SetMaterial(i, material);

            Debug.Log(
                $"+++++++++++==> Override material slot {i} with {slot.Name} at {staticModel.MaterialSlots[i].Material.Path}");
        }
    }
}