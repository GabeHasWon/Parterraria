using System;

namespace Parterraria.Content.Items.Board.PartyItems;

internal class NegativeDice : DiceItem
{
    protected override int DiceType => ModContent.ProjectileType<NegativeDice_Dice>();
    protected override bool IsConsumable => true;

    public class NegativeDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [-1, -2, -3, -4, -5, -6];
        protected override int PipCountToFrame(int pipCount) => Math.Abs(pipCount) - 1;
    }
}
