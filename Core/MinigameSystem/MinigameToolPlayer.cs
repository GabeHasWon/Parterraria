using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.Synchronization.MinigameSyncing;
using System;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Core.MinigameSystem;

internal class MinigameToolPlayer : ModPlayer
{
    public enum ToolMode : byte
    {
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
            return;
        }

        if (Main.myPlayer != Player.whoAmI)
            return;

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
                    WorldMinigameSystem.TryAddMinigame(SelectedMinigame.FullName, _minigameArea.Value, null, null, true);
                    _minigameArea = null;
                }
            }
            else if (toolMode == ToolMode.Edit)
            {
                Minigame newMinigame = WorldMinigameSystem.worldMinigames.FirstOrDefault(x => x.area.Contains(Main.MouseWorld.ToPoint()));

                if (newMinigame is not null && _selectedWorldMinigame is not null && newMinigame.netId == _selectedWorldMinigame.netId)
                    return;

                _selectedWorldMinigame = newMinigame;

                if (_selectedWorldMinigame is not null)
                {
                    BoardUISystem.SetMiscUI(new EditObjectUIState(_selectedWorldMinigame, (obj, changed) =>
                    {
                        if (!changed)
                            return;

                        _selectedWorldMinigame = (Minigame)obj;

                        if (Main.netMode == NetmodeID.MultiplayerClient)
                            new UpdateMinigameModule((byte)Main.myPlayer, (short)_selectedWorldMinigame.netId, _selectedWorldMinigame.GetNetBytes()).Send();

                        string postfix = Main.netMode == NetmodeID.MultiplayerClient ? "AndSynced" : "";
                        string date = DateTime.Now.ToString("H:mm:ss");
                        Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Minigame.Updated" + postfix, date), CommonColors.Success);
                    }));
                }
                else
                {
                    BoardUISystem.CloseMiscUI();
                    _selectedWorldMinigame = null;
                }
            }
            else if (toolMode == ToolMode.Erase)
            {
                if (_selectedWorldMinigame is null)
                {
                    _selectedWorldMinigame = WorldMinigameSystem.worldMinigames.FirstOrDefault(x => x.area.Contains(Main.MouseWorld.ToPoint()));

                    if (_selectedWorldMinigame is not null)
                        Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolUI.EraseConfirm"), Color.Pink);
                }
                else
                {
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        WorldMinigameSystem.RemoveMinigame(_selectedWorldMinigame);
                    else
                        new RemoveMinigameModule((short)_selectedWorldMinigame.netId).Send(-1, -1, false);
                }
            }
        }
    }

    public override void PostUpdate() => lastLeftClick = Main.mouseLeft && Main.mouseLeftRelease && !Player.lastMouseInterface;
    
    public void ClearTool()
    {
        _selectedWorldMinigame = null;
        _minigameArea = null;
    }
}
