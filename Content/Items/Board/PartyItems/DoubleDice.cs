using Parterraria.Core.BoardSystem;
using Terraria.DataStructures;

namespace Parterraria.Content.Items.Board.PartyItems;

internal class DoubleDice : DiceItem, IBoardShopItem
{
    protected override int DiceType => ModContent.ProjectileType<NormalDice.NormalDice_Dice>();

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.GetModPlayer<PlayingBoardPlayer>().SetDiceCount(2);
        Projectile.NewProjectile(source, position, new Vector2(-3, -16), type, 0, 0, player.whoAmI);
        Projectile.NewProjectile(source, position, new Vector2(3, -16), type, 0, 0, player.whoAmI);
        return false;
    }
}
