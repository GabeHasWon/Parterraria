using Parterraria.Core.BoardSystem.Nodes;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

internal abstract class MoveEvent : Microevent
{
    public const string Key = "Mods.Parterraria.Microevents.";

    protected abstract int Movement { get; }

    public override LocalizedText Text => Language.GetOrRegister(Key + "MoveEvent.Text", () => "").WithFormatArgs(Movement, Key + "MoveEvent." +  (Movement >= 0 ? "Front" : "Back"));

    protected override void InternalInvoke(Player player)
    {
        for (int i = 0; i < 12; ++i)
            Dust.NewDust(player.position, player.width, player.head, DustID.Confetti, 0, -4);

        PlayingBoardPlayer boardPlr = player.GetModPlayer<PlayingBoardPlayer>();
        ref BoardNode node = ref boardPlr.connectedNode;
        
        if (Movement > 0)
        {
            for (int i = 0; i < Movement; ++i)
                node = node.links.links[0].ToNode;
        }
        else
        {
            for (int i = 0; i < -Movement; ++i)
            {
                List<BoardNode> nodes = [];
                node = WorldBoardSystem.Self.playingBoard.nodes.First(x => x.links.HasLinkTo(boardPlr.connectedNode) && x is not StartNode);
            }
        }

        for (int i = 0; i < 12; ++i)
            Dust.NewDust(player.position, player.width, player.head, DustID.Confetti, 0, -4);
    }
}

internal class MoveForwardEvent : MoveEvent
{
    public override Quality EventQuality => Quality.Neutral;
    protected override int Movement => 2;
}

internal class MoveBackwardEvent : MoveEvent
{
    public override Quality EventQuality => Quality.Neutral;
    protected override int Movement => -2;
}