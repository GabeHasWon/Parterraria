using Parterraria.Core.BoardSystem;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.PartyItems;

internal abstract class DiceItem : ModItem
{
    protected abstract int DiceType { get; }
    protected virtual bool IsConsumable => true;

    public override void SetDefaults()
    {
        Item.Size = new(30);
        Item.noMelee = false;
        Item.useTurn = true;
        Item.useTime = 8;
        Item.useAnimation = 8;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.noUseGraphic = true;
        Item.shoot = DiceType;
        Item.consumable = IsConsumable;
    }

    public override bool CanUseItem(Player player)
    {
        var boardPlayer = player.GetModPlayer<PlayingBoardPlayer>();
        return !boardPlayer.isMoving && boardPlayer.diceCount == 0;
    }

    public override void ModifyShootStats(Player player, ref Vector2 p, ref Vector2 velocity, ref int t, ref int d, ref float k) => velocity.Y = -16;

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        var boardPlayer = player.GetModPlayer<PlayingBoardPlayer>();
        boardPlayer.diceCount = 1;
        return true;
    }

    public abstract class DiceProjectile : ModProjectile
    {
        protected abstract int[] PipChoices { get; }

        private bool Spinning
        {
            get => Projectile.ai[0] == 0;
            set => Projectile.ai[0] = !value ? 1 : 0;
        }

        private ref float Roll => ref Projectile.ai[1];
        private ref float HitY => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.Size = new(12);
            Projectile.timeLeft = 600;
            Projectile.penetrate = 6;
            Projectile.aiStyle = -1;
            Projectile.hostile = true;
            Projectile.friendly = false;
        }

        public override void AI()
        {
            Player plr = Main.player[Projectile.owner];

            if (plr.GetModPlayer<PlayingBoardPlayer>().isMoving && Spinning)
            {
                Projectile.timeLeft = 30;
                Spinning = false;
            }

            if (Spinning && plr.active && !plr.dead && plr.velocity.Y < -0.5f && plr.Hitbox.Intersects(Projectile.Hitbox))
            {
                Roll = Main.rand.Next(PipChoices);
                HitY = Projectile.Center.Y;
                Projectile.velocity.Y = plr.velocity.Y * 6f;
                Spinning = false;

                AdvancedPopupRequest popup = default;
                popup.Color = Color.White;
                popup.Text = Roll.ToString();
                popup.Velocity = Projectile.velocity * 0.75f;
                popup.DurationInFrames = 240;
                PopupText.NewText(popup, Projectile.Center);

                plr.GetModPlayer<PlayingBoardPlayer>().RolledDice((int)Roll);
            }

            Projectile.velocity.Y *= 0.8f;
            Projectile.velocity.X *= 0.9f;

            if (Spinning)
            {
                Projectile.timeLeft++;
                var node = plr.GetModPlayer<PlayingBoardPlayer>().connectedNode;

                if (node is null)
                    return;

                if (node != null && Projectile.Top.Y < node.Bounds.Y)
                    Projectile.velocity.Y += 0.2f;
            }
            else
            {
                if (Math.Abs(Projectile.Center.Y - HitY) < 10f)
                    Projectile.velocity.Y *= 0.8f;
                else
                    Projectile.velocity.Y += 0.2f;

                if (Projectile.timeLeft < 30)
                    Projectile.Opacity = Projectile.timeLeft / 30f;
            }
        }

        protected abstract int PipCountToFrame(int pipCount);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Color col = lightColor * Projectile.Opacity;

            if (Spinning)
            {
                int frame = (int)(Main.GameUpdateCount / 2 % 4);
                Rectangle source = new(26 * (frame + 1), 0, 24, 24);

                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, col, 0, source.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
            else
            {
                Rectangle source = new(22 * PipCountToFrame((int)Roll), 26, 20, 20);

                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, col, 0, source.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
