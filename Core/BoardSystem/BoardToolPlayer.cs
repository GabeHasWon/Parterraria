using Parterraria.Content.Items.Board.Create;

namespace Parterraria.Core.BoardSystem;

internal partial class BoardToolPlayer : ModPlayer
{
    public ToolMode Mode { get; set; } = ToolMode.None;

    internal string editingBoard = "";

    private bool _mouseConsumed = false;

    public override void ResetEffects() => _mouseConsumed = false;

    public override void PreUpdateBuffs()
    {
        if (Mode != ToolMode.None)
        {
            _mouseConsumed = true;

            if (Main.myPlayer == Player.whoAmI)
                ToolUsage.UseTool(Mode);
        }
    }

    public override bool CanUseItem(Item item) => !_mouseConsumed || item.mountType >= 0;

    internal bool IsEditing() => editingBoard is not null && editingBoard != string.Empty && Player.HeldItem.type == ModContent.ItemType<BoardTool>();
}
