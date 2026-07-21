using Flax.Build;

public class Unreal_ImporterTarget : GameProjectTarget
{
    /// <inheritdoc />
    public override void Init()
    {
        base.Init();

        // Reference the modules for game
        Modules.Add("Unreal_Importer");
    }
}
