using Parterraria.Content.NPCs;
using Terraria.Audio;
using Terraria.ID;

namespace Parterraria.Core.BoardSystem.Nodes;

public class ShopNode() : EmptyNode
{
    public override void PassBy(Board board, Player player)
    {
        NPC npc;

        //if (!NPC.AnyNPCs(ModContent.NPCType<ShopNPC>()))
        //{
        int x = NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<ShopNPC>(), 0, player.whoAmI);
        player.SetTalkNPC(x);
        npc = Main.npc[x];
        //}
        //else
        //{
        //    npc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<ShopNPC>())];
        //    player.SetTalkNPC(npc.whoAmI);
        //    npc.Center = player.Center;
        //    npc.ai[0] = player.whoAmI;
        //}

        for (int i = 0; i < 15; ++i)
            Dust.NewDust(npc.position, npc.width, npc.height, DustID.Confetti, Main.rand.NextFloat(-2, 2f), Main.rand.NextFloat(-8, -6));

        Main.playerInventory = true;
        Main.stackSplit = 9999;
        Main.npcChatText = "";
        Main.SetNPCShopIndex(1);
        Main.instance.shop[Main.npcShop].SetupShop(NPCShopDatabase.GetShopName(npc.type, "Shop"), npc);
        SoundEngine.PlaySound(SoundID.MenuOpen);
    }
}