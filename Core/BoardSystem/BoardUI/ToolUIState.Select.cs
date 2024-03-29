using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal partial class ToolUIState : UIState
{
    private void OpenBoardList(UIMouseEvent evt, UIElement listeningElement)
    {
        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;

        if (_state == OpenPanelState.Select)
        {
            _state = OpenPanelState.None;
            _openPanel.Remove();
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

            foreach (var item in ModContent.GetInstance<WorldBoardSystem>().worldBoards.Keys)
                AddBoardNameToList(list, item);

            UICharacterNameButton namePlate = new(Text("BoardName"), Text("EmptyBoardName"), Text("BoardDescription"))
            {
                Width = StyleDimension.FromPixelsAndPercent(-4f, 1f),
                HAlign = 0f,
            };

            namePlate.OnLeftClick += (_, _) =>
            {
                Main.playerInventory = false;
                WorldBoardSystem.OpenKeyboard((value) =>
                {
                    value = ModContent.GetInstance<WorldBoardSystem>().GetUnrepeatedKey(value);
                    AddBoardNameToList(list, value);
                    WorldBoardSystem.CloseKeyboard();
                    ModContent.GetInstance<WorldBoardSystem>().worldBoards.Add(value, new Board());
                }, WorldBoardSystem.CloseKeyboard);
            };

            _openPanel.Append(namePlate);
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
            Main.NewText(Language.GetText("Mods.Parterraria.ToolUI.BoardSelected").Format(board));
            _boardKey = board;
            _player.GetModPlayer<BoardToolPlayer>().editingBoard = board;
        };

        var delete = new UIButton<string>("[c/FF0000:x]")
        {
            Width = StyleDimension.FromPixels(30),
            Height = StyleDimension.FromPixels(30),
            HAlign = 1f
        };

        delete.OnLeftClick += (_, _) =>
        {
            list.Remove(button);
            list.Remove(delete);
            ModContent.GetInstance<WorldBoardSystem>().worldBoards.Remove(board);

            if (_player.GetModPlayer<BoardToolPlayer>().editingBoard == board)
            {
                _boardKey = string.Empty;
                _player.GetModPlayer<BoardToolPlayer>().editingBoard = string.Empty;
                _player.GetModPlayer<BoardToolPlayer>().Mode = BoardToolPlayer.ToolMode.None;
                Main.NewText(Language.GetText("Mods.Parterraria.ToolUI.BoardDeleted").Format(board));
            }
        };

        back.Append(button);
        back.Append(delete);
        list.Add(back);
    }
}
