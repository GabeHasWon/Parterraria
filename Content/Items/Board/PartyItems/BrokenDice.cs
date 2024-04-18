namespace Parterraria.Content.Items.Board.PartyItems;

internal class BrokenDice : DiceItem
{
    protected override int DiceType => ModContent.ProjectileType<BrokenDice_Dice>();

    public class BrokenDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [-2, 0, 2, 4, 6, 8];
        protected override int PipCountToFrame(int pipCount) => (pipCount + 2) / 2;
    }
}
