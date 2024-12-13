using Parterraria.Common;
using Parterraria.Content.Items.Board.PartyItems;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

internal class GetItemEvent : Microevent
{
    public override Quality EventQuality => Quality.Great;
    public override LocalizedText Text => Language.GetOrRegister("Mods.Parterraria.Microevents.GetItemEvent", () => "");

    protected override void InternalInvoke(Player player)
    {
        int[] items = [ModContent.ItemType<HighDice>(), ModContent.ItemType<DoubleDice>(), ModContent.ItemType<PartyMirror>(), ModContent.ItemType<BrokenDice>()];
        CommonUtils.SafelyAddItemToInv(player, Main.rand.Next(items), 1);
    }
}
