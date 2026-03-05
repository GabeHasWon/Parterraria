using Parterraria.Common;
using Parterraria.Content.Items.Board.Create;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class PopperUI(PartyPopper popper) : UIState
{
    const string LocStart = "Mods.Parterraria.ToolInfo.";

    private readonly PartyPopper _popper = popper;

    public override void OnInitialize()
    {
        UIPanel listPanel = new()
        {
            Width = StyleDimension.FromPixels(400),
            Height = StyleDimension.FromPixels(300),
            Top = StyleDimension.FromPixelsAndPercent(0, 0.55f),
            HAlign = 0.5f,
        };

        listPanel.OnUpdate += self =>
        {
            if (self.ContainsPoint(Main.MouseWorld))
                Main.LocalPlayer.mouseInterface = true;
        };

        Append(listPanel);

        UIList list = new()
        {
            Width = StyleDimension.FromPixelsAndPercent(-26, 1f),
            Height = StyleDimension.FromPixelsAndPercent(-30, 1f),
            VAlign = 1f
        };
        listPanel.Append(list);

        UIScrollbar bar = new()
        {
            Width = StyleDimension.FromPixels(20),
            Height = StyleDimension.FromPixelsAndPercent(-30, 1f),
            HAlign = 1f,
            VAlign = 1f
        };
        list.SetScrollbar(bar);
        listPanel.Append(bar);

        foreach (string item in ModContent.GetInstance<WorldBoardSystem>().worldBoards.Keys)
            AddBoardNameToList(list, item);

        UIText currentBoard = new("") { HAlign = 0f };
        currentBoard.OnUpdate += _ =>
        {
            string key = _popper.selectedBoard;
            currentBoard.SetText(Language.GetTextValue(LocStart + "Current") + (key == string.Empty ? $"[c/888888:{Language.GetTextValue(LocStart + "None")}]" : key));
        };
        listPanel.Append(currentBoard);

        UIButton<string> start = new(Language.GetTextValue(LocStart + "Start")) 
        { 
            HAlign = 1f, Width = StyleDimension.FromPixels(50), Height = StyleDimension.FromPixels(24), Top = StyleDimension.FromPixels(-6) 
        };

        start.OnLeftClick += (_, _) => _popper.TryStartBoard(Main.LocalPlayer);
        listPanel.Append(start);
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
            Width = StyleDimension.FromPixelsAndPercent(-8f, 1f),
            Height = StyleDimension.FromPixels(30)
        };

        button.OnLeftClick += (_, _) =>
        {
            Main.NewText(Language.GetText("Mods.Parterraria.ToolUI.BoardSelected").Format(board), CommonColors.Info);
            _popper.selectedBoard = board;
        };

        back.Append(button);
        list.Add(back);
    }
}
