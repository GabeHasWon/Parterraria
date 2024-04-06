namespace Parterraria.Content.Items.Board.PartyItems;

internal class NormalDice : DiceItem
{
    protected override int DiceType => ModContent.ProjectileType<NormalDice_Dice>();
    protected override bool IsConsumable => false;

    public class NormalDice_Dice : DiceProjectile
    {
        protected override int[] PipChoices => [1, 2, 3, 4, 5, 6];
        protected override int PipCountToFrame(int pipCount) => pipCount - 1;
    }
}
