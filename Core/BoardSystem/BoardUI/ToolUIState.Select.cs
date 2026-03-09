using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal partial class ToolUIState : UIState
{
    private static int confirmTimer;
    private static UIText confirmText = null;

    private void OpenBoardList(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;

        if (_state == OpenPanelState.Select)
        {
            _state = OpenPanelState.None;
            _openPanel?.Remove();
        }
        else
        {
            _state = OpenPanelState.Select;
            _openPanel = new UIPanel()
            {
                Width = StyleDimension.FromPixels(400),
                Height = StyleDimension.FromPixels(300),
                Top = StyleDimension.FromPixelsAndPercent(0, 0.55f),
                HAlign = 0.5f,
            };
            Append(_openPanel);

            UIList list = new()
            {
                Width = StyleDimension.FromPixelsAndPercent(-26, 1f),
                Height = StyleDimension.FromPixelsAndPercent(-60, 1f),
                VAlign = 1f
            };
            _openPanel.Append(list);

            UIScrollbar bar = new()
            {
                Width = StyleDimension.FromPixels(20),
                Height = StyleDimension.FromPixelsAndPercent(-60, 1f),
                HAlign = 1f,
                VAlign = 1f
            };
            list.SetScrollbar(bar);
            _openPanel.Append(bar);

            foreach (string item in ModContent.GetInstance<WorldBoardSystem>().worldBoards.Keys)
                AddBoardNameToList(list, item);

            UICharacterNameButton namePlate = new(Text("BoardName"), Text("EmptyBoardName"), Text("BoardDescription"))
            {
                Width = StyleDimension.FromPixelsAndPercent(-4f, 1f),
                HAlign = 0f,
            };

            namePlate.OnLeftClick += (_, _) =>
            {
                if (Main.netMode == NetmodeID.MultiplayerClient && !Main.countsAsHostForGameplay[Main.myPlayer])
                {
                    Main.NewText(Text("FailedPerms").Value, CommonColors.Error);
                    return;
                }

                Main.playerInventory = false;
                BoardUISystem.OpenKeyboard((value) =>
                {
                    value = ModContent.GetInstance<WorldBoardSystem>().GetUnrepeatedKey(value);
                    AddBoardNameToList(list, value);
                    BoardUISystem.CloseKeyboard();
                    ModContent.GetInstance<WorldBoardSystem>().worldBoards.Add(value, new Board());
                }, BoardUISystem.CloseKeyboard);
            };

            _openPanel.Append(namePlate);
        }
    }

    private static void UpdateRemoveText()
    {
        confirmTimer--;

        if (confirmTimer <= 0 && confirmText is not null)
        {
            confirmText.Remove();
            confirmText = null;
        }
    }

    private void AddBoardNameToList(UIList list, string item)
    {
        var back = new UIElement()
        {
            Width = StyleDimension.FromPercent(1f),
            Height = StyleDimension.FromPixels(30)
        };

        string board = item;
        var button = new UIButton<string>(item)
        {
            Width = StyleDimension.FromPixelsAndPercent(-34f, 1f),
            Height = StyleDimension.FromPixels(30)
        };

        button.OnLeftClick += (_, _) =>
        {
            Main.NewText(Language.GetText("Mods.Parterraria.ToolUI.BoardSelected").Format(board), CommonColors.Info);
            _boardKey = board;
            _player.GetModPlayer<BoardToolPlayer>().editingBoard = board;

            if (BoardUISystem.Self.miscUI.CurrentState is EditObjectUIState) // Toggle edit UI to use the new config properly
            {
                BoardUISystem.CloseMiscUI();
                BoardUISystem.SetMiscUI(new EditObjectUIState(WorldBoardSystem.Self.worldBoards[_boardKey].config, 
                    (obj) => WorldBoardSystem.Self.worldBoards[_boardKey].config = (BoardConfig)obj));
            }
        };

        var delete = new UIButton<string>("[c/FF0000:x]")
        {
            Width = StyleDimension.FromPixels(30),
            Height = StyleDimension.FromPixels(30),
            HAlign = 1f
        };

        delete.OnLeftClick += (_, _) =>
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && !Main.countsAsHostForGameplay[Main.myPlayer])
            {
                Main.NewText(Text("FailedPerms").Value, CommonColors.Error);
                return;
            }

            if (confirmTimer <= 0)
            {
                confirmTimer = 90;

                confirmText = new UIText(Text("DeleteBoard"), 1.1f)
                {
                    HAlign = 1f,
                    VAlign = 1f,
                    Left = StyleDimension.FromPixels(0),
                    Top = StyleDimension.FromPixels(40),
                    TextColor = Color.Red
                };

                confirmText.OnUpdate += (_) => 
                {
                    confirmText.TextColor = Color.Red;
                    
                    if (confirmTimer < 30)
                    {
                        confirmText.TextColor = Color.Red * (confirmTimer / 30f);
                    }
                };
                list.Parent.Append(confirmText);
                return;
            }

            confirmText.Remove();
            confirmText = null;
            list.Remove(back);
            list.RecalculateChildren();

            ModContent.GetInstance<WorldBoardSystem>().worldBoards.Remove(board);

            if (_player.GetModPlayer<BoardToolPlayer>().editingBoard == board)
            {
                _boardKey = string.Empty;
                _player.GetModPlayer<BoardToolPlayer>().editingBoard = string.Empty;
                _player.GetModPlayer<BoardToolPlayer>().Mode = ToolMode.None;
                Main.NewText(Language.GetText("Mods.Parterraria.ToolUI.BoardDeleted").Format(board), CommonColors.Info);

                BoardUISystem.CheckCloseMiscUI<EditObjectUIState>();
                ToolUsage.ResetTool();
            }
        };

        back.Append(button);
        back.Append(delete);
        list.Add(back);
    }
}
