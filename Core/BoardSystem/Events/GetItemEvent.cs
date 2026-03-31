using Parterraria.Common;
using Parterraria.Content.Items.Board.PartyItems;
using System.IO;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

#nullable enable

internal class GetItemEvent : Microevent
{
    public override Quality EventQuality => Quality.Great;
    public override LocalizedText Text => Language.GetOrRegister("Mods.Parterraria.Microevents.GetItemEvent", () => "");
    public override bool UseDefaultPopup => false;

    private static int[] Options => [ModContent.ItemType<HighDice>(), ModContent.ItemType<DoubleDice>(), ModContent.ItemType<PartyMirror>(), ModContent.ItemType<BrokenDice>(),
        ModContent.ItemType<NegativeDice>()];

    protected override void InternalInvoke(Player player, BinaryReader? reader)
    {
        int item;

        if (reader is null)
            item = Main.rand.Next(Options);
        else
            item = Options[reader.ReadByte()];

        CommonUtils.SafelyAddItemToInv(player, item, 1);

        if (!Main.dedServ)
            SpawnPopupText(player.Center, Text.Format(Lang.GetItemNameValue(item)), PopupColor);
    }

    public override void NetSend(BinaryWriter writer) => writer.Write((byte)Main.rand.Next(Options));
}
