using Microsoft.Xna.Framework.Graphics;
using NPCUtils;
using Parterraria.Core.MinigameSystem;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.NPCs;

[AutoloadCritter]
internal class GoldFaerie : ModNPC
{
    private Vector2 Position
    {
        get => new(NPC.ai[0], NPC.ai[1]);
        set => (NPC.ai[0], NPC.ai[1]) = (value.X, value.Y);
    }

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;

        NPCID.Sets.TrailCacheLength[Type] = 5;
        NPCID.Sets.TrailingMode[Type] = 3;
    }

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.BlackDragonfly);
        NPC.aiStyle = -1;
        NPC.dontTakeDamage = true;
        NPC.Size = new Vector2(24, 18);
    }

    public override void AI()
    {
        if (!WorldMinigameSystem.InMinigame)
        {
            NPC.active = false;
            return;
        }

        if (Position == Vector2.Zero || Position.DistanceSQ(NPC.Center) < 60 * 60)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                var area = WorldMinigameSystem.Self.playingMinigame.area;
                Position = new Vector2(Main.rand.Next(area.Left, area.Right), Main.rand.Next(area.Top, area.Bottom));
                NPC.netUpdate = true;
            }
        }
        else
        {
            NPC.velocity += NPC.DirectionTo(Position) * 1.2f;

            if (NPC.velocity.LengthSquared() > 18 * 18)
                NPC.velocity = Vector2.Normalize(NPC.velocity) * 18;

            NPC.velocity *= 0.99f;
        }
    }

    public override void FindFrame(int frameHeight) => NPC.frame.Y = (int)((NPC.frameCounter += 0.4f) % 2) * frameHeight;
    public override bool? CanFallThroughPlatforms() => true;

    public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
    {
        Texture2D tex = TextureAssets.Npc[Type].Value;

        for (int k = 0; k < NPC.oldPos.Length; k++)
        {
            Vector2 drawPos = NPC.oldPos[k] - Main.screenPosition + new Vector2(0f, NPC.gfxOffY) + NPC.Size / 2f;
            Color color = NPC.GetAlpha(drawColor) * ((NPC.oldPos.Length - k) / (float)NPC.oldPos.Length);
            Main.EntitySpriteDraw(tex, drawPos, NPC.frame, color, NPC.oldRot[k], NPC.Size / 2f, NPC.scale, SpriteEffects.None, 0);
        }

        return true;
    }
}
