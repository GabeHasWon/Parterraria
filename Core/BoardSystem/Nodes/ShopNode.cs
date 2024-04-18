using Parterraria.Content.Items.Board.PartyItems;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Core.BoardSystem.Nodes;

public class ShopNode() : EmptyNode
{
    public override void PassBy(Board board, Player player)
    {
        NPC npc;

        if (!NPC.AnyNPCs(ModContent.NPCType<ShopNPC>()))
        {
            int x = NPC.NewNPC(null, (int)player.Center.X, (int)player.Center.Y, ModContent.NPCType<ShopNPC>());
            player.SetTalkNPC(x);
            npc = Main.npc[x];
        }
        else
        {
            npc = Main.npc[NPC.FindFirstNPC(ModContent.NPCType<ShopNPC>())];
            player.SetTalkNPC(npc.whoAmI);
            npc.Center = player.Center;
        }

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

/// <summary>
/// Dummy NPC for registering a shop.
/// </summary>
public class ShopNPC : ModNPC
{
    public override string Texture => "Terraria/Images/NPC_" + NPCID.Guide;

    public override void SetStaticDefaults() => Main.npcFrameCount[Type] = Main.npcFrameCount[NPCID.Guide];

    public override void SetDefaults()
    {
        NPC.CloneDefaults(NPCID.Guide);
        NPC.Size = new Vector2(60);
        NPC.townNPC = true;
    }

    public override bool PreAI() => false;

    public override void SetChatButtons(ref string button, ref string button2) => button = Language.GetTextValue("LegacyInterface.28");

    public override void OnChatButtonClicked(bool firstButton, ref string shop)
    {
        if (firstButton)
            shop = "Shop";
    }

    public override void AddShops()
    {
        NPCShop shop = new NPCShop(Type)
            .Add(new NPCShop.Entry(new Item(ModContent.ItemType<DoubleDice>()) { shopSpecialCurrency = Parterraria.AmethystCurrencyID, shopCustomPrice = 4 }))
            .Add(new NPCShop.Entry(new Item(ModContent.ItemType<TripleDice>()) { shopSpecialCurrency = Parterraria.AmethystCurrencyID, shopCustomPrice = 9 }))
            .Add(new NPCShop.Entry(new Item(ModContent.ItemType<HighDice>()) { shopSpecialCurrency = Parterraria.AmethystCurrencyID, shopCustomPrice = 3 }))
            .Add(new NPCShop.Entry(new Item(ModContent.ItemType<LowDice>()) { shopSpecialCurrency = Parterraria.AmethystCurrencyID, shopCustomPrice = 1 }))
            .Add(new NPCShop.Entry(new Item(ModContent.ItemType<PartyMirror>()) { shopSpecialCurrency = Parterraria.AmethystCurrencyID, shopCustomPrice = 5 }));
        shop.Register();
    }
}