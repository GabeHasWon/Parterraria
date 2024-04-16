namespace Parterraria.Content.Items.Board.PartyItems;

internal class HighDice : DiceItem
{
    protected override int DiceType => ModContent.ProjectileType<HighDice_Dice>();

    public class HighDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [4, 5, 6];
        protected override int PipCountToFrame(int pipCount) => pipCount - 4;
    }
}
