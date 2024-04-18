using Parterraria.Core.BoardSystem;
using Parterraria.Core.MinigameSystem;
using Terraria.DataStructures;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.PartyItems;

internal class PartyMirror : ModItem
{
    public override void SetDefaults()
    {
        Item.Size = new(44, 52);
        Item.noMelee = true;
        Item.useTurn = true;
        Item.useTime = 20;
        Item.useAnimation = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.noUseGraphic = true;
        Item.shoot = ProjectileID.Waffle;
        Item.consumable = true;
        Item.rare = ItemRarityID.LightPurple;
    }

    public override bool CanUseItem(Player player)
    {
        var boardPlayer = player.GetModPlayer<PlayingBoardPlayer>();
        return !boardPlayer.isMoving && boardPlayer.diceCount == 0 && !WorldMinigameSystem.InMinigame && !WorldMinigameSystem.selectingMinigame;
    }

    public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int d, ref float k)
    {
        player.GetModPlayer<PlayingBoardPlayer>().connectedNode = Main.rand.Next(WorldBoardSystem.Self.playingBoard.nodes);
        player.Center = player.GetModPlayer<PlayingBoardPlayer>().connectedNode.position;
        position = player.Center;

        velocity.Y = -16;
        type = ModContent.ProjectileType<NormalDice.NormalDice_Dice>();
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        player.GetModPlayer<PlayingBoardPlayer>().SetDiceCount(1);

        for (int i = 0; i < 12; ++i)
            Dust.NewDust(position, 1, 1, DustID.Confetti, 0, -4);
        return true;
    }
}
