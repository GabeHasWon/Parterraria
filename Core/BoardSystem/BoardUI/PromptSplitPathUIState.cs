using Parterraria.Core.Synchronization;
using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class PromptSplitPathUIState(List<NodeLinks.Link> linksToCheck, bool goingBack) : UIState
{
    private readonly List<NodeLinks.Link> links = linksToCheck;
    private readonly bool goingBack = goingBack;

    public override void OnInitialize()
    {
        Width = StyleDimension.Fill;
        Height = StyleDimension.Fill;

        UIPanel panel = new()
        {
            Width = StyleDimension.FromPixels(links.Count * 72 + 14),
            Height = StyleDimension.FromPixels(86),
            Top = StyleDimension.FromPercent(0.2f),
            HAlign = 0.5f,
        };

        Append(panel);

        int fromLeft = 0;
        
        foreach (var item in links)
        {
            UIImageButton button = new(BoardNode.Tex(goingBack ? item.Parent : item.ToNode, true))
            {
                Width = StyleDimension.FromPixels(60),
                Height = StyleDimension.FromPixels(60),
                Left = StyleDimension.FromPixels(fromLeft++ * 72),
            };

            NodeLinks.Link link = item;
            button.OnLeftClick += (_, _) => ConfirmChoice(link, goingBack);
            button.OnUpdate += (ui) => HoverOverButton(ui, goingBack ? link.Parent : link.ToNode);
            panel.Append(button);
        }

        panel.Append(new UIText(Language.GetTextValue("Mods.Parterraria.MiscUI.PromptSplitPathUIState.Choose")) { HAlign = 0.5f, VAlign = -1 });
    }

    private static void HoverOverButton(UIElement ui, BoardNode node)
    {
        if (ui.IsMouseHovering)
            WorldBoardSystem.Self.hoverNode = node;
    }

    private static void ConfirmChoice(NodeLinks.Link link, bool backwards)
    {
        var boardPlayer = Main.LocalPlayer.GetModPlayer<PlayingBoardPlayer>();
        boardPlayer.promptingSplit = false;
        boardPlayer.nextNode = backwards ? link.Parent : link.ToNode;

        if (Main.netMode == NetmodeID.MultiplayerClient)
            new SyncConfirmSplitPathModule(Main.myPlayer, boardPlayer.nextNode.nodeId).Send();

        WorldBoardSystem.Self.hoverNode = null;
        BoardUISystem.CloseMiscUI();
    }
}
