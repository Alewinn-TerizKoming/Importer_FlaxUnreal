using System;
using System.Collections.Generic;
using FlaxEngine;
using Newtonsoft.Json;
 
namespace Unreal_ImporterEditor;

public enum LightType
{
    Directional,
    Point,
    Spot,
    Sky,
}

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

public struct LightSettings
{
    public string Color;
    public float Brightness;
    public float ViewDistance;
    public float MinimumRoughness;
    public float IndirectLightingIntensity;
}

public struct ShadowSettings
{
    public string Mode;
    public string PartitionMode;
    public int CascadeCount;
    public float CascadeSpacing;
    public float Sharpness;
    public float Strength;
    public float Distance;
    public float FadeDistance;
    public float DepthBias;
    public float NormalOffsetScale;
    public float ContactShadowLength;
    public float Resolution;
}

public struct VolumetricFogSettings
{
    public float ScatteringIntensity;
    public bool CastShadow;
}

public struct LocalLightSettings
{
    public float Radius;
    public float SourceRadius;
    public float SourceLength;
    public bool UseInverseSquaredFalloff;
    public float FallOffExponent;
}

public struct SpotLightSettings
{
    public float InnerConeAngle;
    public float OuterConeAngle;
}

public struct SkyLightSettings
{
    public string AdditiveColor;
    public string Mode;
    public float SkyDistanceThreshold;
    public string CustomTexture;
}

public struct IESSettings
{
    public string Texture;
    public bool UseBrightness;
    public float BrightnessScale;
}

public struct Light
{
    public string Name;
    public string Folder;
    public LightType Type;
    public Vector3 Location;
    public Rotation Rotation;
    public Vector3 Scale;
    public ActorProperties Properties;
    public LightSettings Settings;
    public ShadowSettings Shadow;
    public VolumetricFogSettings VolumetricFog;
    public LocalLightSettings Local;
    public SpotLightSettings Spot;
    public SkyLightSettings Sky;
    public IESSettings IES;
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
    public List<Light> Lights;
    public List<MaterialSlot> Materials;
}

