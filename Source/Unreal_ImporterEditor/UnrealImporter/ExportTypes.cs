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

public class Vector3
{
    public float X;
    public float Y;
    public float Z;
}

public class Rotation
{
    public float Pitch;
    public float Yaw;
    public float Roll;
}

public class ActorProperties
{
    public bool Visibility;
    public bool HiddenInGame;
    public string Mobility;
    public bool CastShadow;
    public List<string> Tags;
    public string Layer;
}

public class LightSettings
{
    public string Color;
    public float Brightness;
    public float ViewDistance;
    public float MinimumRoughness;
    public float IndirectLightingIntensity;
}

public class ShadowSettings
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

public class VolumetricFogSettings
{
    public float ScatteringIntensity;
    public bool CastShadow;
}

public class LocalLightSettings
{
    public float Radius;
    public float SourceRadius;
    public float SourceLength;
    public bool UseInverseSquaredFalloff;
    public float FallOffExponent;
}

public class SpotLightSettings
{
    public float InnerConeAngle;
    public float OuterConeAngle;
}

public class SkyLightSettings
{
    public string AdditiveColor;
    public string Mode;
    public float SkyDistanceThreshold;
    public string CustomTexture;
}

public class IESSettings
{
    public string Texture;
    public bool UseBrightness;
    public float BrightnessScale;
}

public class Light
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

public class StaticMesh
{
    public string AssetPath;
    public string Name;
    public override bool Equals(object? obj)
    {
        return false;
    }
}

public class MaterialSlot
{
    public string AssetPath;
    public string Name;
    public string ParentMaterial;
    public string Color;
    public bool SceneOverride;
    public override bool Equals(object? obj)
    {
        return false;
    }
}

public class MeshInstance
{
    public string Name;
    public string Folder;
    public int Mesh;
    public Vector3 Location;
    public Rotation Rotation;
    public Vector3 Scale;
    public ActorProperties Properties;
    public List<int> Materials;
    public string AsPrefab;
}

public class Scene
{
    public string SceneName;
    public string From;
    public List<StaticMesh> StaticMeshes;
    public List<MeshInstance> MeshInstances;
    public List<Light> Lights;
    public List<MaterialSlot> Materials;
}

