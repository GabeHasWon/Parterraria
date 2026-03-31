using Parterraria.Core.MinigameSystem;
using Terraria.GameContent;
using Terraria.ID;

namespace Parterraria.Content.NPCs;

internal class SlimeOfTerraria : ModNPC
{
    private ref float Timer => ref NPC.ai[0];

    public override void SetStaticDefaults()
    {
        Main.npcFrameCount[Type] = 2;

        NPCID.Sets.TrailCacheLength[Type] = 5;
        NPCID.Sets.TrailingMode[Type] = 3;
    }

    public override void SetDefaults()
    {
        NPC.aiStyle = -1;
        NPC.dontTakeDamage = true;
        NPC.Size = new Vector2(40, 30);
        NPC.Opacity = 0.9f;
        NPC.lifeMax = 50000;
        NPC.damage = 0;
        NPC.friendly = true;
    }

    public override void AI()
    {
        if (!WorldMinigameSystem.InMinigame)
        {
            NPC.active = false;
            return;
        }

        Timer++;

        if (NPC.velocity.Y == 0)
        {
            NPC.velocity.X *= 0.8f;

            if (Timer >= 80)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.velocity.Y = -8f;
                    NPC.velocity.X = Main.rand.NextFloat(-4, 4);
                    NPC.netUpdate = true;
                }

                Timer = 0;
            }
        }
    }

    public override void FindFrame(int frameHeight) => NPC.frame.Y = (int)((NPC.frameCounter += 0.1f) % 2) * frameHeight;

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
