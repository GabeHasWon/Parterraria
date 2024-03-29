
namespace Parterraria.Core.BoardSystem;

internal class BoardToolPlayer : ModPlayer
{
    internal enum ToolMode : byte
    {
        None = 0,
        Paint = 1,
        Link = 2,
    }

    public ToolMode Mode { get; set; } = ToolMode.None;

    private bool _mouseConsumed = false;

    public override void ResetEffects() => _mouseConsumed = false;

    public override void UpdateEquips()
    {
        if (Mode != ToolMode.None)
        {
            _mouseConsumed = true;

            if (Main.myPlayer == Player.whoAmI)
                ToolUsage.UseTool(Mode);
        }
    }

    public override bool CanUseItem(Item item) => !_mouseConsumed;
}
