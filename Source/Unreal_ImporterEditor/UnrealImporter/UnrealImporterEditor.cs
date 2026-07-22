using FlaxEditor;
using FlaxEditor.GUI;
using FlaxEditor.Scripting;
using FlaxEditor.Surface;
using FlaxEditor.Surface.Archetypes;
using FlaxEngine;
using MaterialLab;
using System;
using System.IO;
using System.Threading.Tasks;
namespace Unreal_ImporterEditor;

/// <summary>
/// UnrealImporterEditor class.
/// </summary>
public class UnrealImporterEditor : EditorPlugin
{
    private ToolStripButton _button;
    private SpriteAtlas _icons;

    // private ToolStripButton _TestButton;
    private FlaxUnrealImporter importer;

    //private FlaxUnrealImporter _unrealImporter;

    public override void InitializeEditor()
    {
        base.InitializeEditor();

        _icons = Content.Load<SpriteAtlas>("Plugins/Importer_FlaxUnreal/Content/Editor/ExpFl_64.flax");

        importer = new FlaxUnrealImporter();

        _button = Editor.UI.ToolStrip.AddButton("Import Unreal");
        _button.Icon = new SpriteHandle(_icons, 0);
       
        // _TestButton = Editor.UI.ToolStrip.AddButton("Create Material");

        importer.ImportCompleted += OnImportCompleted;
        _button.Clicked += OnImportClicked;
        //_TestButton.Clicked += OnTestClicked;
    }

    public override void DeinitializeEditor()
    {
        if (_button != null)
        {
            _button.Dispose();
            _button.Clicked -= OnImportClicked; ;
            _button = null;
        }

        //if (_TestButton != null)
        //{
        //    _TestButton.Dispose();
        //    _TestButton.Clicked -= OnTestClicked;
        //    _TestButton = null;
        //}

        importer.ImportCompleted -= OnImportCompleted;
        base.DeinitializeEditor();
    }

    private void OnImportClicked()
    {
        FlaxEngine.Debug.Log("Click thread : " + System.Threading.Thread.CurrentThread.ManagedThreadId);

        _ = importer.Import();
    }

    private void OnImportCompleted(AssetImportResult assets)
    {
        Scripting.InvokeOnUpdate(() =>
        {
            var builder = new SceneBuilder();
            builder.Build(assets);
        });
    }

    //private bool _isGeneratingMaterial;
    //private async void OnTestClicked()
    //{
    //    if (_isGeneratingMaterial)
    //    {
    //        Debug.Log("Material generation already running...");
    //        return;
    //    }

    //    try
    //    {
    //        _isGeneratingMaterial = true;

    //        await Task.Run(() =>
    //        {
    //            TestMaterial();
    //        });
    //    }
    //    finally
    //    {
    //        _isGeneratingMaterial = false;
    //    }
    //}

    //public async Task TestMaterial() 
    //{ 
    //    string path = Path.Combine(Globals.ProjectContentFolder, "TestMaterial.flax");

    //    if (File.Exists(path)) File.Delete(path);
    //    Editor.CreateAsset("Material", path);
    //    await Task.Delay(1000);

    //    Material material = Content.Load<Material>(path);
    //    material.WaitForLoaded();

    //    MatGraphTools.SetColor(material, Color.Red);
    //}

}
