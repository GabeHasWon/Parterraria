using System.Collections.Generic;

namespace Parterraria.Content.Items.Board.PartyItems;

internal class NormalDice : DiceItem
{
    protected override int DiceType => ModContent.ProjectileType<NormalDice_Dice>();

    public class NormalDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [1, 2, 3, 4, 5, 6];
        protected override Dictionary<int, int> PipCountToFrame => new() { { 1, 0 }, { 2, 1 }, { 3, 2 }, { 4, 3 }, { 5, 4 }, { 6, 5 } };
    }
}
