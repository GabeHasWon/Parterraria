using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.BoardSystem;
using System.Linq;
using Parterraria.Content.Items.Board.Create;
using Terraria.Localization;

namespace Parterraria.Core.MinigameSystem;

internal class MinigameToolPlayer : ModPlayer
{
    public enum ToolMode : byte
    {
        None,
        Place,
        Edit,
        Erase,
    }

    public Minigame SelectedMinigame => Minigame.MinigamesById[_selectedMinigameId];

    internal int _selectedMinigameId = 0;
    internal Rectangle? _minigameArea = null;
    internal Minigame _selectedWorldMinigame = null;
    internal ToolMode toolMode = ToolMode.Edit;

    private bool lastLeftClick = false;

    public override void PreUpdate()
    {
        if (Player.HeldItem.ModItem is not MinigameTool)
        {
            ClearTool();
            toolMode = ToolMode.None;
            return;
        }

        if (_minigameArea.HasValue)
        {
            Rectangle area = _minigameArea.Value;
            area.Width = (int)(Main.MouseWorld.X - area.X);
            area.Height = (int)(Main.MouseWorld.Y - area.Y);
            SelectedMinigame.ValidateRectangle(ref area);
            SelectedMinigame.playerStartLocation = area.TopLeft().ToTileCoordinates();
            _minigameArea = area;
        }

        bool leftClick = Main.mouseLeft && Main.mouseLeftRelease;
        bool rightClick = Main.mouseRight && Main.mouseRightRelease;

        if (Player.mouseInterface || Player.lastMouseInterface)
            return;

        if (rightClick)
        {
            if (!_minigameArea.HasValue)
            {
                _selectedMinigameId++;

                if (_selectedMinigameId >= Minigame.MinigamesById.Count)
                    _selectedMinigameId = 0;

                Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolUI.MinigameSelected"));
            }
            else
                _minigameArea = null;

            _selectedWorldMinigame = null;
            return;
        }

        if (lastLeftClick)
        {
            if (toolMode == ToolMode.Place)
            {
                if (_minigameArea is null)
                {
                    Point loc = Main.MouseWorld.ToTileCoordinates();
                    _minigameArea = new Rectangle(loc.X * 16, loc.Y * 16, 10, 10);
                }
                else
                {
                    WorldMinigameSystem.TryAddMinigame(SelectedMinigame.FullName, _minigameArea.Value);
                    _minigameArea = null;
                }
            }
            else if (toolMode == ToolMode.Edit)
            {
                if (_selectedWorldMinigame is not null)
                    return;

                _selectedWorldMinigame = WorldMinigameSystem.worldMinigames.FirstOrDefault(x => x.area.Contains(Main.MouseWorld.ToPoint()));

                if (_selectedWorldMinigame is not null)
                    BoardUISystem.SetMiscUI(new EditObjectUIState(_selectedWorldMinigame, (obj) => _selectedWorldMinigame = (Minigame)obj));
            }
            else if (toolMode == ToolMode.Erase)
            {
                if (_selectedWorldMinigame is null)
                {
                    _selectedWorldMinigame = WorldMinigameSystem.worldMinigames.FirstOrDefault(x => x.area.Contains(Main.MouseWorld.ToPoint()));
                    Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolUI.EraseConfirm"));
                }
                else
                {
                    WorldMinigameSystem.RemoveMinigame(_selectedWorldMinigame);
                }
            }
        }
    }

    public override void PostUpdate() => lastLeftClick = Main.mouseLeft && Main.mouseLeftRelease && !Player.lastMouseInterface;
    
    public void ClearTool()
    {
        _selectedWorldMinigame = null;
        _selectedMinigameId = 0;
        _minigameArea = null;
    }
}
