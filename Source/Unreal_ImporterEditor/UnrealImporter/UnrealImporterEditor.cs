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
    private FlaxUnrealImporter importer;

    public override void InitializeEditor()
    {
        base.InitializeEditor();

        _icons = Content.Load<SpriteAtlas>("Plugins/Importer_FlaxUnreal/Content/Editor/ExpFl_64.flax");
        importer = new FlaxUnrealImporter();
        _button = Editor.UI.ToolStrip.AddButton("Import Unreal");
        _button.Icon = new SpriteHandle(_icons, 0);
        importer.ImportCompleted += OnImportCompleted;
        _button.Clicked += OnImportClicked;
    }

    public override void DeinitializeEditor()
    {
        if (_button != null)
        {
            _button.Dispose();
            _button.Clicked -= OnImportClicked; ;
            _button = null;
        }

        importer.ImportCompleted -= OnImportCompleted;
        base.DeinitializeEditor();
    }

    private void OnImportClicked()
    {
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
}
