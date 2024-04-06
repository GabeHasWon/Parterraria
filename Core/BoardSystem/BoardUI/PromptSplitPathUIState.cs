using System.Collections.Generic;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;

namespace Parterraria.Core.BoardSystem.BoardUI;

internal class PromptSplitPathUIState(List<NodeLinks.Link> linksToCheck) : UIState
{
    private readonly List<NodeLinks.Link> links = linksToCheck;

    public override void OnInitialize()
    {
        Width = StyleDimension.Fill;
        Height = StyleDimension.Fill;

        UIPanel panel = new()
        {
            Width = StyleDimension.FromPixels((links.Count + 1) * 36),
            Height = StyleDimension.FromPixels(60),
            Top = StyleDimension.FromPercent(0.2f),
            HAlign = 0.5f
        };

        Append(panel);

        int fromLeft = 0;
        
        foreach (var item in links)
        {
            UIImageButton button = new(BoardNode.Tex(item.ToNode, true))
            {
                Width = StyleDimension.FromPixels(30),
                Height = StyleDimension.FromPixels(30),
                Left = StyleDimension.FromPixels(fromLeft++ * 36),
            };

            NodeLinks.Link link = item;
            button.OnLeftClick += (_, _) => ConfirmChoice(link);
            button.OnUpdate += (ui) => HoverOverButton(ui, link.ToNode);
            panel.Append(button);
        }
    }

    private static void HoverOverButton(UIElement ui, BoardNode node)
    {
        if (ui.IsMouseHovering)
            WorldBoardSystem.Self.hoverNode = node;
    }

    private static void ConfirmChoice(NodeLinks.Link link)
    {
        var boardPlayer = Main.LocalPlayer.GetModPlayer<PlayingBoardPlayer>();
        boardPlayer.prompingSplitPath = false;
        boardPlayer.nextNode = link.ToNode;

        WorldBoardSystem.Self.hoverNode = null;
        WorldBoardSystem.CloseMiscUI();
    }
}
