using Parterraria.Common;
using Parterraria.Core.BoardSystem.BoardUI.EditUI;
using Parterraria.Core.Synchronization;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.UI;
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

    public static bool HoveringList = true;

    // UI stuff
    private UIPanel _openPanel = null;
    private OpenPanelState _state = OpenPanelState.None;
    private UIPanel listPanel = null;

    // Info / tracking
    private string _boardKey = string.Empty;
    private readonly Dictionary<ToolMode, bool> _toggledAlt = new() { { ToolMode.Paint, false }, { ToolMode.Link, false } };

    private static Asset<Texture2D> Texture(string name, bool immediate = false) => ModContent.Request<Texture2D>("Parterraria/Assets/Textures/UI/Tool/" + name,
        immediate ? AssetRequestMode.ImmediateLoad : AssetRequestMode.AsyncLoad);
    private static LocalizedText Text(string name) => Language.GetText("Mods.Parterraria.ToolUI." + name);

    public override void OnInitialize()
    {
        if (WorldBoardSystem.PlayingParty)
            _boardKey = WorldBoardSystem.Self.playingBoardKey;

        UIPanel mainPanel = new()
        {
            Width = StyleDimension.FromPixels(300),
            Height = StyleDimension.FromPixels(56),
            HAlign = 0.5f,
            VAlign = 0.4f
        };

        Append(mainPanel);

        UIText toolSelect = new(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().Mode.ToString(), 1) 
        { 
            Top = StyleDimension.FromPixels(-20)
        };
        toolSelect.OnUpdate += (self) => (self as UIText).SetText(Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().Mode.ToString());
        mainPanel.Append(toolSelect);

        UIText boardSelect = new(GetPlayerEditingBoardName(), 1)
        {
            Top = StyleDimension.FromPixels(-20),
            HAlign = 1f
        };
        boardSelect.OnUpdate += (self) => (self as UIText).SetText(GetPlayerEditingBoardName());
        mainPanel.Append(boardSelect);

        int number = 0;
        AppendToolButton("Select", OpenBoardList, null, mainPanel, ref number);
        AppendToolButton("Paint", SetPaintMode, (_, ui) => SwitchTool(ToolMode.Paint, ui as UIImageButton, "Paint", "Erase"), mainPanel, ref number);
        AppendToolButton("Link", SetLinkMode, (_, ui) => SwitchTool(ToolMode.Link, ui as UIImageButton, "Link", "Unlink"), mainPanel, ref number);
        AppendToolButton("Play", StartParty, (_, ui) => EndParty(), mainPanel, ref number);
        AppendToolButton("Edit", EditBoard, (_, ui) => BoardUISystem.CloseMiscUI(), mainPanel, ref number);
        
        AppendToolButton("Close", ExitMenu, null, mainPanel, ref number);
        AppendToolButton("Validate", ValidateBoard, null, mainPanel, ref number);
    }

    private void ValidateBoard(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.NoBoard"), CommonColors.Error);
            return;
        }

        Board board = WorldBoardSystem.Self.worldBoards[_boardKey];
        List<int> invalidNodes = [];

        foreach (BoardNode node in board.nodes)
            if (node.links.LinkCount == 0)
                invalidNodes.Add(node.nodeId);

        if (invalidNodes.Count == 0)
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.ValidBoard"));
        else
        {
            string nodes = "";

            foreach (int nodeId in invalidNodes)
            {
                nodes += $"ID: {nodeId} (a {board.nodesById[nodeId].DisplayName}), ";
            }

            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.HangingNodes", nodes[..^2]), Color.Lerp(Color.Red, Color.White, 0.5f));
        }
    }

    internal void ToggleMinigameNodeList()
    {
        if (listPanel is not null)
        {
            listPanel.Remove();
            listPanel = null;
            return;
        }

        if (_openPanel is not null)
        {
            _openPanel.Remove();
            _openPanel = null;
        }

        listPanel = new UIPanel()
        {
            Width = StyleDimension.FromPixels(400),
            Height = StyleDimension.FromPixels(200),
            HAlign = 0.5f,
            VAlign = 0.4f,
            Top = StyleDimension.FromPixels(-160)
        };

        listPanel.OnUpdate += self =>
        {
            if (self.ContainsPoint(Main.MouseScreen))
            {
                HoveringList = true;
            }
        };

        Append(listPanel);

        UIText buildingText = new UIText("[c/CCCCCC:Now building:] " + ToolUsage.buildingNode?.DisplayName.Value ?? "None")
        {
            Width = StyleDimension.Fill,
            Top = StyleDimension.FromPixels(-20)
        };

        buildingText.OnUpdate += self => (self as UIText).SetText("[c/CCCCCC:Now building:] " + NodeLoader.Get(ToolUsage.buildNodeIndex)?.DisplayName.Value ?? "None");
        listPanel.Append(buildingText);

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

        for (int i = 0; i < NodeLoader.NodeCount; ++i)
        {
            BoardNode node = NodeLoader.Get(i);

            UIElement element = new()
            {
                Height = StyleDimension.FromPixels(40),
                Width = StyleDimension.FromPercent(1)
            };

            UIImage image = new(node.Icon) { ImageScale = 0.6f, Left = StyleDimension.FromPixels(-12), Top = StyleDimension.FromPixels(-12) };
            element.Append(image);

            var nodeButton = new UIButton<string>(node.DisplayName.Value + $" [c/AAAAAA:({node.Name})]")
            {
                Left = StyleDimension.FromPixels(40),
                Width = StyleDimension.FromPixelsAndPercent(-40 - 10, 1),
                Height = StyleDimension.FromPixels(34),
                VAlign = 0.1f
            };

            int index = i;

            nodeButton.OnLeftClick += (_, _) =>
            {
                ToolUsage.buildNodeIndex = index;

                ref var buildingNode = ref ToolUsage.buildingNode;
                Vector2 position = buildingNode.position;
                float width = buildingNode.halfWidth;
                buildingNode = Activator.CreateInstance(Type.GetType(ToolUsage.BuildNodeType)) as BoardNode;
                buildingNode.position = position;
                buildingNode.halfWidth = width;
            };

            element.Append(nodeButton);
            list.Add(element);
        }

        list.Add(new UIElement() { Height = StyleDimension.FromPixels(4) });
    }

    private void EditBoard(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.NoBoard"), CommonColors.Error);
            return;
        }

        BoardUISystem.SetMiscUI(new EditObjectUIState(WorldBoardSystem.Self.worldBoards[_boardKey].config, (obj) => WorldBoardSystem.Self.worldBoards[_boardKey].config = (BoardConfig)obj));
    }

    private static void EndParty()
    {
        if (Main.netMode == NetmodeID.SinglePlayer)
            WorldBoardSystem.StopParty();
        else
            new SyncEndPartyModule().Send();
    }

    private void StartParty(UIMouseEvent evt, UIElement listeningElement)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.NoBoard"), CommonColors.Error);
            return;
        }

        if (WorldBoardSystem.CanPlayParty(_boardKey, out string denialKey))
        {
            _player.mouseInterface = true;
            Main.isMouseLeftConsumedByUI = true;

            if (Main.netMode == NetmodeID.MultiplayerClient)
                new SyncStartPartyModule(Main.myPlayer, _boardKey).Send(-1, -1, false);
            else
            {
                WorldBoardSystem.Self.boardHost = Main.myPlayer;
                WorldBoardSystem.PlayParty(_boardKey);
                BoardUISystem.CloseToolUI();
                BoardUISystem.CheckCloseMiscUI<EditObjectUIState>();
            }
        }
        else
            Main.NewText(Language.GetTextValue(denialKey), CommonColors.Error);
    }

    private void ExitMenu(UIMouseEvent evt, UIElement listeningElement)
    {
        BoardUISystem.CloseToolUI();
        ToolUsage.ResetTool();
        Main.isMouseLeftConsumedByUI = true;

        _player.GetModPlayer<BoardToolPlayer>().editingBoard = string.Empty;
        _player.GetModPlayer<BoardToolPlayer>().Mode = ToolMode.None;
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

        if (rightClick is not null)
            button.OnRightClick += rightClick;

        panel.Append(button);

        number++;
    }

    private static string GetPlayerEditingBoardName()
    {
        string board = Main.LocalPlayer.GetModPlayer<BoardToolPlayer>().editingBoard;
        return board is null || board == string.Empty ? "[None]" : board;
    }

    private void SetPaintMode(UIMouseEvent evt, UIElement listeningElement) => ChangeTool(ToolMode.Paint, ToolMode.Erase);
    private void SetLinkMode(UIMouseEvent evt, UIElement listeningElement) => ChangeTool(ToolMode.Link, ToolMode.Unlink);

    private void ChangeTool(ToolMode mode, ToolMode altMode)
    {
        if (_boardKey == string.Empty)
        {
            Main.NewText(Language.GetTextValue("Mods.Parterraria.ToolInfo.Board.NoBoard"), CommonColors.Error);
            return;
        }

        Main.isMouseLeftConsumedByUI = true;
        _player.mouseInterface = true;

        ToolMode useMode = _toggledAlt[mode] ? altMode : mode;

        if (_player.GetModPlayer<BoardToolPlayer>().Mode != useMode)
            _player.GetModPlayer<BoardToolPlayer>().Mode = useMode;
        else
            _player.GetModPlayer<BoardToolPlayer>().Mode = ToolMode.None;

        ToolUsage.ResetTool();
    }

    private void SwitchTool(ToolMode mode, UIImageButton button, string defaultTexture, string altTexture)
    {
        _toggledAlt[mode] = !_toggledAlt[mode];

        if (!_toggledAlt[mode])
            button.SetImage(Texture(defaultTexture, true));
        else
            button.SetImage(Texture(altTexture, true));

        button.Recalculate();
    }
}
