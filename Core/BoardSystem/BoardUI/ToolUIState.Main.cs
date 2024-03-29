using ReLogic.Content;
using Terraria.GameContent.UI.Elements;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal partial class ToolUIState(Player player) : UIState
{
    private enum OpenPanelState : byte
    {
        None,
        Select,
        Paint,
    }

    private readonly Player _player = player;

    // UI stuff
    private UIPanel _openPanel = null;
    private OpenPanelState _state = OpenPanelState.None;

    // Info / tracking
    private string _boardKey = string.Empty;

    private static Asset<Texture2D> Texture(string name) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/UI/Tool/" + name);
    private static LocalizedText Text(string name) => Language.GetText("Mods.Parterraria.ToolUI." + name);

    public override void OnInitialize()
    {
        UIPanel mainPanel = new()
        {
            Width = StyleDimension.FromPixels(300),
            Height = StyleDimension.FromPixels(56),
            HAlign = 0.5f,
            VAlign = 0.4f
        };

        Append(mainPanel);

        int number = 0;
        AppendToolButton("Select", OpenBoardList, mainPanel, ref number);
        AppendToolButton("Paint", SetPaintMode, mainPanel, ref number);
        AppendToolButton("Link", SetLinkMode, mainPanel, ref number);
    }

    private void SetPaintMode(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText("Select a board first!");
            return;
        }

        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;

        if (_player.GetModPlayer<BoardToolPlayer>().Mode != BoardToolPlayer.ToolMode.Paint)
            _player.GetModPlayer<BoardToolPlayer>().Mode = BoardToolPlayer.ToolMode.Paint;
        else
            _player.GetModPlayer<BoardToolPlayer>().Mode = BoardToolPlayer.ToolMode.None;
    }

    private void SetLinkMode(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText("Select a board first!");
            return;
        }

        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;

        if (_player.GetModPlayer<BoardToolPlayer>().Mode != BoardToolPlayer.ToolMode.Link)
            _player.GetModPlayer<BoardToolPlayer>().Mode = BoardToolPlayer.ToolMode.Link;
        else
            _player.GetModPlayer<BoardToolPlayer>().Mode = BoardToolPlayer.ToolMode.None;
    }

    private static void AppendToolButton(string name, MouseEvent openBoardList, UIPanel panel, ref int number)
    {
        var tex = Texture(name);

        UIImageButton button = new(tex)
        {
            Width = StyleDimension.FromPercent(32),
            Height = StyleDimension.FromPercent(32),
            Left = StyleDimension.FromPixels(number * 40)
        };

        button.OnLeftClick += openBoardList;
        panel.Append(button);

        number++;
    }
}
