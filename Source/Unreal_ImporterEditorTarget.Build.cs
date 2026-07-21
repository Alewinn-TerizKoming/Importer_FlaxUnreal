using Flax.Build;

public class Unreal_ImporterEditorTarget : GameProjectEditorTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for editor
                Modules.Add(nameof(MaterialLab));
        Modules.Add("Unreal_Importer");
        Modules.Add("Unreal_ImporterEditor");
    }
}
