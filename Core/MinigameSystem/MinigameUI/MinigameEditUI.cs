using Parterraria.Content.Items.Board.Create;
using Parterraria.Core.BoardSystem;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using ReLogic.Content;
using System;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.MinigameSystem.MinigameUI;

internal partial class MinigameEditUI(Player player) : UIState
{
    private enum OpenPanelState : byte
    {
        None,
        Select,
        Paint,
    }

    private readonly Player _player = player;

    private static Asset<Texture2D> Texture(string name, bool immediate = false) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/UI/Tool/" + name,
        immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
    private static LocalizedText Text(string name) => Language.GetText("Mods.Parterraria.ToolUI." + name);

    public override void OnInitialize()
    {
        UIPanel mainPanel = new()
        {
            Width = StyleDimension.FromPixels(140),
            Height = StyleDimension.FromPixels(56),
            HAlign = 0.5f,
            VAlign = 0.4f
        };

        Append(mainPanel);

        UIText toolSelect = new(GetToolModeName(), 1)
        {
            Top = StyleDimension.FromPixels(-20)
        };
        toolSelect.OnUpdate += (self) => (self as UIText).SetText(GetToolModeName());
        mainPanel.Append(toolSelect);

        UIText boardSelect = new(Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().SelectedMinigame.DisplayName.Value, 1)
        {
            Top = StyleDimension.FromPixels(20),
            HAlign = 1f,
            VAlign = 1f
        };
        boardSelect.OnUpdate += (self) => (self as UIText).SetText(Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().SelectedMinigame.DisplayName.Value);
        mainPanel.Append(boardSelect);

        int number = 0;
        AppendToolButton("Place", SwitchMode, null, mainPanel, ref number);
        AppendToolButton("Close", ExitMenu, null, mainPanel, ref number);
    }

    public static string GetToolModeName() => Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().toolMode switch
    {
        MinigameToolPlayer.ToolMode.None => "None",
        MinigameToolPlayer.ToolMode.Place => "Place",
        MinigameToolPlayer.ToolMode.Edit => "Edit",
        _ => "Erase"
    };

    private void SwitchMode(UIMouseEvent evt, UIElement listeningElement)
    {
        _player.mouseInterface = true;
        _player.GetModPlayer<MinigameToolPlayer>().ClearTool();
        _player.GetModPlayer<MinigameToolPlayer>().toolMode++;

        if (_player.GetModPlayer<MinigameToolPlayer>().toolMode > MinigameToolPlayer.ToolMode.Erase)
            _player.GetModPlayer<MinigameToolPlayer>().toolMode = MinigameToolPlayer.ToolMode.Place;

        Main.isMouseLeftConsumedByUI = true;

        (listeningElement as UIImageButton).SetImage(Texture(GetToolModeName()));
        listeningElement.Width = StyleDimension.FromPixels(32);
        listeningElement.Height = StyleDimension.FromPixels(32);
        listeningElement.Recalculate();
    }

    private void ExitMenu(UIMouseEvent evt, UIElement listeningElement)
    {
        BoardUISystem.CloseToolUI();
        BoardUISystem.CloseMiscUI();
        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;
    }

    private static void AppendToolButton(string name, MouseEvent onClick, MouseEvent rightClick, UIPanel panel, ref int number)
    {
        var tex = Texture(name);

        UIImageButton button = new(tex)
        {
            Width = StyleDimension.FromPercent(32),
            Height = StyleDimension.FromPercent(32),
            Left = StyleDimension.FromPixels(number * 40)
        };

        button.OnLeftClick += onClick;
        button.OnUpdate += StopUseOnHover;

        if (rightClick is not null)
            button.OnRightClick += rightClick;

        panel.Append(button);

        number++;
    }

    private static void StopUseOnHover(UIElement affectedElement)
    {
        if (affectedElement.GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
            Main.LocalPlayer.mouseInterface = true;
    }
}
