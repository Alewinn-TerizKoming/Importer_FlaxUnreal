using FlaxEditor;
using FlaxEngine;
using FlaxEngine.Assertions;
using FlaxEngine.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Unreal_ImporterEditor;

public class FlaxUnrealImporter
{
    public event Action<AssetImportResult> ImportCompleted;

    private SceneReader _sceneReader;
    private AssetImporter _assetImporter;
    private SceneBuilder _sceneBuilder;

    public async Task Import()
    {
        if (FileSystem.ShowOpenFileDialog(
            Editor.Instance.Windows.MainWindow,
            null,
            "Unreal Scene (*.json)\0*.json\0All files (*.*)\0*.*\0",
            false,
            "Select Unreal scene",
            out var files))
        {
            return;
        }

        if (files == null || files.Length == 0)
            return;

        string sceneFilename = files[0];
        Debug.Log($"Importing scene : {sceneFilename}");

        _sceneReader = new SceneReader();
        Scene scene = _sceneReader.Read(sceneFilename);
        _assetImporter = new AssetImporter();

        AssetImportResult result = await _assetImporter.ImportAsync(scene, sceneFilename);

        ImportCompleted?.Invoke(result);
    }
}