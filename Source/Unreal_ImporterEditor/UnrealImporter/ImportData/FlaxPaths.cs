using FlaxEngine;
using System;
using System.IO;

namespace Unreal_ImporterEditor;

public static class FlaxPaths
{
    public static string Normalize(string path)
    {
        return Path
            .GetFullPath(path)
            .TrimEnd('\\', '/')
            .ToLowerInvariant();
    }

    public static string NormalizeImportPath(string path)
    {
        return Path.GetFullPath(path).Replace('\\', '/');
    }

    public static bool PathsEqual(string a, string b)
    {
        return Normalize(a) == Normalize(b);
    }

    public static string ToContentRelativePath(string absolutePath)
    {
        string contentRoot =
            Path.Combine(
                Globals.ProjectFolder,
                "Content");

        string relative =
            Path.GetRelativePath(
                contentRoot,
                absolutePath);

        return Path.Combine(
                "Content",
                relative)
            .Replace('\\', '/');
    }

    public static string GetSceneFolder(string sceneName)
    {
        return Path.Combine(
            Globals.ProjectFolder,
            "Content",
            "Scenes",
            sceneName);
    }


    public static string GetMeshPath(string sceneName,string meshName)
    {
        return Path.Combine(
            GetSceneFolder(sceneName),
            "Meshes",
            meshName + ".flax");
    }


    public static string GetCollisionMeshPath(string sceneName,string meshName)
    {
        return Path.Combine(
            GetSceneFolder(sceneName),
            "Collisions",
            meshName + ".flax");
    }


    public static string GetCollisionDataPath(string sceneName,string meshName)
    {
        return Path.Combine(
            GetSceneFolder(sceneName),
            "Collisions",
            meshName + "_ColData.flax");
    }


    public static string GetPrefabPath(string sceneName,string meshName)
    {
        return Path.Combine(
            GetSceneFolder(sceneName),
            meshName + ".prefab");
    }

    public static string GetUnrealAssetPath(string unrealAssetPath,string extension)
    {
        string relative;

        if (unrealAssetPath.StartsWith("/Game/"))
        {
            relative = unrealAssetPath.Substring("/Game/".Length);
            relative = Path.Combine( "Game", relative);
        }
        else if (unrealAssetPath.StartsWith("/Engine/"))
        {
            relative = unrealAssetPath.Substring("/Engine/".Length);
            relative = Path.Combine("Engine",relative);
        }
        else
        {
            throw new Exception( $"Unsupported Unreal asset path : {unrealAssetPath}");
        }

        int dot = relative.LastIndexOf('.');

        if (dot >= 0) { relative = relative.Substring(0, dot); }

        return Path.Combine( Globals.ProjectFolder, "Content", "Unreal", relative + extension);
    }

    public static string GetUnrealMeshPath(string unrealAssetPath)
    {
        return GetUnrealAssetPath(unrealAssetPath, ".flax");
    }

    public static string GetUnrealPrefabPath(string unrealAssetPath)
    {
        return GetUnrealAssetPath( unrealAssetPath, ".prefab");
    }

    public static string GetUnrealCollisionDataPath(string unrealAssetPath)
    {
        return GetUnrealAssetPath(unrealAssetPath, "_ColData.flax");
    }

}