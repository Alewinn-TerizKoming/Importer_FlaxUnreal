using System;
using System.Collections.Generic;
using FlaxEngine;
using Newtonsoft.Json;

namespace Unreal_ImporterEditor;

public struct Vector3
{
    public float X;
    public float Y;
    public float Z;
}

public struct Rotation
{
    public float Pitch;
    public float Yaw;
    public float Roll;
}

public struct ActorProperties
{
    public bool Visibility;
    public bool HiddenInGame;
    public string Mobility;
    public bool CastShadow;
    public List<string> Tags;
    public string Layer;
}

public struct StaticMesh
{
    public string AssetPath;
    public string Name;
    public override bool Equals(object? obj)
    {
        return false;
    }
}

public struct MaterialSlot
{
    public string AssetPath;
    public string Name;
    public string ParentMaterial;
    public string Color;
    public override bool Equals(object? obj)
    {
        return false;
    }
}

public struct MeshInstance
{
    public string Name;
    public string Folder;
    public int Mesh;
    public Vector3 Location;
    public Rotation Rotation;
    public Vector3 Scale;
    public ActorProperties Properties;
    public List<int> Materials;
}

public struct Scene
{
    public string SceneName;
    public List<StaticMesh> StaticMeshes;
    public List<MeshInstance> MeshInstances;
    public List<MaterialSlot> Materials;
}

