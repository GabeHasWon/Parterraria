using Parterraria.Core.BoardSystem;
using ReLogic.Content;
using System.Linq;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.ModLoader.UI;
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

    private UIPanel listPanel = null;

    private static Asset<Texture2D> Texture(string name, bool immediate = false) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/UI/Tool/" + name,
        immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);

    public override void OnInitialize()
    {
        UIPanel mainPanel = new()
        {
            Width = StyleDimension.FromPixels(140),
            Height = StyleDimension.FromPixels(56),
            HAlign = 0.5f,
            VAlign = 0.4f
        };

        mainPanel.OnUpdate += self =>
        {
            if (self.ContainsPoint(Main.MouseScreen))
            {
                Main.LocalPlayer.mouseInterface = true;
            }
        };

        Append(mainPanel);

        UIText toolSelect = new(GetToolModeName(Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().toolMode), 1)
        {
            Top = StyleDimension.FromPixels(-20)
        };

        toolSelect.OnUpdate += (self) =>
        {
            MinigameToolPlayer.ToolMode toolMode = Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().toolMode;
            toolMode++;

            if (toolMode > MinigameToolPlayer.ToolMode.Erase)
            {
                toolMode = MinigameToolPlayer.ToolMode.Place;
            }

            (self as UIText).SetText(GetToolModeName(Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().toolMode) + "[c/AAAAAA: -> " + GetToolModeName(toolMode) + "]");
        };
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
        AppendToolButton("Place", SetMode, SwitchMode, mainPanel, ref number);
        AppendToolButton("Close", ExitMenu, null, mainPanel, ref number);
        AppendToolButton("List", ToggleList, null, mainPanel, ref number);
    }

    private void SetMode(UIMouseEvent evt, UIElement listeningElement)
    {
        _player.GetModPlayer<MinigameToolPlayer>().ClearTool();
        //_player.GetModPlayer<MinigameToolPlayer>().toolMode++;

        //if (_player.GetModPlayer<MinigameToolPlayer>().toolMode > MinigameToolPlayer.ToolMode.Erase)
        //    _player.GetModPlayer<MinigameToolPlayer>().toolMode = MinigameToolPlayer.ToolMode.Place;
    }

    private void ToggleList(UIMouseEvent evt, UIElement listeningElement)
    {
        if (listPanel is not null)
        {
            listPanel.Remove();
            listPanel = null;
            return;
        }

        listPanel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(400),
            Height = StyleDimension.FromPixels(200),
            HAlign = 0.5f,
            VAlign = 0.4f,
            Left = StyleDimension.FromPixels(280)
        };

        Append(listPanel);

        UIList list = new UIList()
        {
            Width = StyleDimension.FromPixelsAndPercent(-24, 1),
            Height = StyleDimension.FromPercent(1)
        };

        listPanel.Append(list);

        UIScrollbar bar = new()
        {
            Width = StyleDimension.FromPixels(20),
            Height = StyleDimension.FromPercent(1),
            HAlign = 1f
        };

        list.SetScrollbar(bar);
        listPanel.Append(bar);

        list.Add(new UIElement() { Height = StyleDimension.FromPixels(4) });

        foreach ((string name, Minigame game) in Minigame.MinigamesByModAndName)
        {
            string modName = name.Split('/')[0];
            Mod mod = ModLoader.GetMod(modName);
            int offset = 0;

            UIElement element = new() 
            { 
                Height = StyleDimension.FromPixels(40), 
                Width = StyleDimension.FromPercent(1)
            };

            if (ModContent.RequestIfExists(mod.Name + "/icon_small", out Asset<Texture2D> tex))
            {
                UIImage image = new(tex);
                element.Append(image);
                offset = 40;
            }

            var minigameText = new UIButton<string>(game.DisplayName.Value + $" [c/AAAAAA:({game.Name})]")
            {
                Left = StyleDimension.FromPixels(offset),
                Width = StyleDimension.FromPixelsAndPercent(-offset - 10, 1),
                Height = StyleDimension.FromPixels(34),
                VAlign = 0.1f
            };

            minigameText.OnLeftClick += (_, _) =>
            {
                Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>()._selectedMinigameId = Minigame.MinigamesById.First(x => x.Value == game).Key;
                Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolUI.MinigameSelected", game.DisplayName.Value));
            };

            element.Append(minigameText);
            list.Add(element);
        }

        list.Add(new UIElement() { Height = StyleDimension.FromPixels(4) });
    }

    public static string GetToolModeName(MinigameToolPlayer.ToolMode mode) => mode switch
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

        (listeningElement as UIImageButton).SetImage(Texture(GetToolModeName(Main.LocalPlayer.GetModPlayer<MinigameToolPlayer>().toolMode)));
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

        if (onClick is not null)
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
