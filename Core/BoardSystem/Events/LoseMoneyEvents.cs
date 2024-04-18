using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

internal class LoseMoneyEvent : Microevent
{
    public override Quality EventQuality => Quality.Good;
    protected virtual int Loss => WorldBoardSystem.Self.playingBoard.config.CoinDeltaFromNodes;
    public override LocalizedText Text => Language.GetOrRegister("Mods.Parterraria.Microevents.LoseMoneyEvent", () => "").WithFormatArgs(Loss);

    protected override void InternalInvoke(Player player) => CommonUtils.ConsumeItemFromInventory<AmethystCoin>(player, Loss, true);
}

internal class DoubleLoseMoneyEvent : LoseMoneyEvent
{
    public override Quality EventQuality => Quality.Excellent;
    protected override int Loss => base.Loss * 2;
}

internal class ThreeHalvesLoseMoneyEvent : LoseMoneyEvent
{
    public override Quality EventQuality => Quality.Good;
    protected override int Loss => (int)(base.Loss * 1.5f);
}