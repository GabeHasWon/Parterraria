using Parterraria.Core.BoardSystem;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.Create;

class BoardTool : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.noMelee = false;
        Item.useTurn = true;
        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.useStyle = ItemUseStyleID.Swing;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (player.altFunctionUse == 2)
        {
            if (!BoardUISystem.ToolUIOpen())
                BoardUISystem.OpenToolUI();
            return true;
        }

        return true;
    }
}
