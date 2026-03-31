using Parterraria.Common;
using Parterraria.Content.Items.Board;
using System.IO;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

#nullable enable

internal class LoseMoneyEvent : Microevent
{
    public override Quality EventQuality => Quality.Bad;

    /// <summary>
    /// Loses equivalent to the board's config Coin from Nodes.
    /// </summary>
    protected virtual int Loss => WorldBoardSystem.Self.playingBoard.config.CoinDeltaFromNodes;

    public override LocalizedText Text => Language.GetOrRegister("Mods.Parterraria.Microevents.LoseMoneyEvent", () => "").WithFormatArgs(Loss);

    protected override void InternalInvoke(Player player, BinaryReader? reader) => CommonUtils.ConsumeItemFromInventory<AmethystCoin>(player, Loss, true);
}

internal class DoubleLoseMoneyEvent : LoseMoneyEvent
{
    public override Quality EventQuality => Quality.Terrible;
    protected override int Loss => base.Loss * 2;
}

internal class ThreeHalvesLoseMoneyEvent : LoseMoneyEvent
{
    public override Quality EventQuality => Quality.Bad;
    protected override int Loss => (int)(base.Loss * 1.5f);
}