using Parterraria.Core.BoardSystem.Nodes;
using System;
using System.IO;
using System.Linq;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

#nullable enable

[Autoload(false)]
internal class MoveEvent : Microevent
{
    public const string Key = "Mods.Parterraria.Microevents.";

    public override bool UseDefaultPopup => false;
    public override LocalizedText Text => Language.GetOrRegister(Key + "MoveEvent.Text", () => "");

    protected override void InternalInvoke(Player player, BinaryReader? reader)
    {
        for (int i = 0; i < 12; ++i)
            Dust.NewDust(player.position, player.width, player.head, DustID.Confetti, 0, -4);

        PlayingBoardPlayer boardPlr = player.GetModPlayer<PlayingBoardPlayer>();
        ref BoardNode node = ref boardPlr.connectedNode;

        int movement = reader is null ? MoveRange() : reader.ReadSByte();
        int[] options = new int[Math.Abs(movement)];

        for (int i = 0; i < movement; ++i)
            options[i] = reader is null ? (byte)Main.rand.Next(byte.MaxValue + 1) : reader.ReadByte();

        if (movement > 0)
        {
            for (int i = 0; i < movement; ++i)
                node = node.links.links[options[i] % node.links.LinkCount].ToNode;
        }
        else
        {
            for (int i = 0; i < -movement; ++i)
            {
                var nodes = WorldBoardSystem.Self.playingBoard.nodes.Where(x => x.links.HasLinkTo(boardPlr.connectedNode) && x is not StartNode);
                node = nodes.ElementAt(options[i] % nodes.Count());
            }
        }

        if (!Main.dedServ)
        {
            for (int i = 0; i < 12; ++i)
                Dust.NewDust(node.position, player.width, player.head, DustID.Confetti, 0, -4);

            SpawnPopupText(node.position, Text.Format(Math.Abs(movement), Language.GetTextValue(Key + "MoveEvent." + (movement >= 0 ? "Front" : "Back"))), Color.WhiteSmoke);
        }
    }

    public override void NetSend(BinaryWriter writer)
    {
        sbyte move = MoveRange();
        writer.Write(move);

        Main.NewText(move);

        for (int i = 0; i < move; ++i)
            writer.Write((byte)Main.rand.Next(byte.MaxValue + 1));
    }

    private static sbyte MoveRange()
    {
        int baseValue = Main.rand.Next(1, 4) * (Main.rand.NextBool() ? -1 : 1);
        return (sbyte)baseValue;
    }
}
