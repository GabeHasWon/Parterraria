using Parterraria.Common;
using Parterraria.Content.Items.Board;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Events;

internal class GainMoneyEvent : Microevent
{
    public override Quality EventQuality => Quality.Good;
    protected virtual int Gain => WorldBoardSystem.Self.playingBoard.config.CoinDeltaFromNodes;
    public override LocalizedText Text => Language.GetOrRegister("Mods.Parterraria.Microevents.GainMoneyEvent", () => "").WithFormatArgs(Gain);

    protected override void InternalInvoke(Player player) => CommonUtils.SafelyAddItemToInv<AmethystCoin>(player, Gain);
}

internal class DoubleGainMoneyEvent : GainMoneyEvent
{
    public override Quality EventQuality => Quality.Excellent;
    protected override int Gain => base.Gain * 2;
}

internal class ThreeHalvesGainMoneyEvent : GainMoneyEvent
{
    public override Quality EventQuality => Quality.Good;
    protected override int Gain => (int)(base.Gain * 1.5f);
}