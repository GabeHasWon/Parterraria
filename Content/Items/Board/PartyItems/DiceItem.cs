using System;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.Items.Board.PartyItems;

internal abstract class DiceItem : ModItem
{
    protected abstract int DiceType { get; }

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
    }

    public override void ModifyShootStats(Player player, ref Vector2 p, ref Vector2 velocity, ref int t, ref int d, ref float k) => velocity.Y = -16;

    public abstract class DiceProjectile : ModProjectile
    {
        protected abstract int[] PipChoices { get; }
        protected abstract Dictionary<int, int> PipCountToFrame { get; }

        private bool Spinning
        {
            get => Projectile.ai[0] == 0;
            set => Projectile.ai[0] = !value ? 1 : 0;
        }

        private ref float PipCount => ref Projectile.ai[1];
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

            if (Spinning && plr.active && !plr.dead && plr.velocity.Y < -0.5f && plr.Hitbox.Intersects(Projectile.Hitbox))
            {
                PipCount = Main.rand.Next(PipChoices);
                HitY = Projectile.Center.Y;
                Projectile.velocity.Y = plr.velocity.Y * 4f;
                Spinning = false;
            }

            Projectile.velocity.Y *= 0.8f;

            if (Spinning)
                Projectile.timeLeft++;
            else
            {
                if (Math.Abs(Projectile.Center.Y - HitY) < 10f)
                    Projectile.velocity.Y *= 0.8f;
                else
                    Projectile.velocity.Y += 0.2f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;

            if (Spinning)
            {
                int frame = (int)(Main.GameUpdateCount / 2 % 4);
                Rectangle source = new(26 * (frame + 1), 0, 24, 24);

                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, Color.White, 0, source.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
            else
            {
                Rectangle source = new(22 * PipCountToFrame[(int)PipCount], 26, 20, 20);

                Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, source, Color.White, 0, source.Size() / 2f, 1f, SpriteEffects.None, 0);
            }
            return false;
        }
    }
}
