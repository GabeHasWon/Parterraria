using Parterraria.Common;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem;
using Parterraria.Core.MinigameSystem.MinigameUI;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.Create;

class MinigameTool : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.noMelee = false;
        Item.useTurn = true;
        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.rare = ItemRarityID.Blue;
    }

    public override bool AltFunctionUse(Player player) => true;

    public override bool CanUseItem(Player player)
    {
        if (Main.netMode == NetmodeID.Server)
            return false;

        if (player.altFunctionUse == 2)
        {
            if (BoardUISystem.Self.toolUI.CurrentState is not MinigameEditUI)
            {
                BoardUISystem.OpenToolUI(true);
            }
            else
            {
                BoardUISystem.CloseToolUI();
            }

            return false;
        }

        return true;
    }

    public static void DrawTool()
    {
        var toolPlayer = Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>();

        if (toolPlayer._minigameArea.HasValue)
        {
            Rectangle rect = toolPlayer._minigameArea.Value;
            rect.Location -= Main.screenPosition.ToPoint();

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.White * 0.6f);
            DrawCommon.CenteredString(FontAssets.DeathText.Value, Main.ScreenSize.ToVector2() / 2f, toolPlayer.SelectedMinigame.DisplayName.Value, Color.White);
        }
    }

    public override void AddRecipes() => CreateRecipe().AddIngredient(ItemID.Wood, 5).AddIngredient(ItemID.Sapphire).AddTile(TileID.Anvils).Register();
}
