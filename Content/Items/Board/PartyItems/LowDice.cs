namespace Parterraria.Content.Items.Board.PartyItems;

internal class LowDice : DiceItem, IBoardClearItem
{
    protected override int DiceType => ModContent.ProjectileType<LowDice_Dice>();

    public class LowDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [1, 2, 3];
        protected override int PipCountToFrame(int pipCount) => pipCount - 1;
    }
}
