using Parterraria.Common;
using Parterraria.Core.MinigameSystem;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.Create;

class MinigameTool : ModItem
{
    private string SelectedMinigame => Minigame.MinigamesById[_selectedMinigameId].FullName;

    private int _selectedMinigameId = 0;
    private Rectangle? _minigameArea = null;

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
            _selectedMinigameId++;

            if (_selectedMinigameId >= Minigame.MinigamesById.Count)
                _selectedMinigameId = 0;

            Main.NewText($"Minigame {Minigame.MinigamesByModAndName[SelectedMinigame].DisplayName.Value} selected.");
            return false;
        }

        return true;
    }

    public override void HoldItem(Player player)
    {
        if (_minigameArea.HasValue)
        {
            Rectangle area = _minigameArea.Value;
            area.Width = (int)(Main.MouseWorld.X - area.X);
            area.Height = (int)(Main.MouseWorld.Y - area.Y);
            Minigame.MinigamesByModAndName[SelectedMinigame].ValidateRectangle(ref area);
            _minigameArea = area;
        }
    }

    public override bool? UseItem(Player player)
    {
        if (_minigameArea is null)
        {
            Point loc = Main.MouseWorld.ToTileCoordinates();
            _minigameArea = new Rectangle(loc.X * 16, loc.Y * 16, 10, 10);
        }
        else
        {
            WorldMinigameSystem.TryAddMinigame(SelectedMinigame, _minigameArea.Value);
            _minigameArea = null;
        }

        return true;
    }

    public void DrawTool()
    {
        if (_minigameArea.HasValue)
        {
            Rectangle rect = _minigameArea.Value;
            rect.Location -= Main.screenPosition.ToPoint();

            Main.spriteBatch.Draw(TextureAssets.MagicPixel.Value, rect, Color.White * 0.6f);

            DrawCommon.CenteredString(FontAssets.DeathText.Value, Main.ScreenSize.ToVector2() / 2f, "Megg", Color.White);
        }
    }
}
