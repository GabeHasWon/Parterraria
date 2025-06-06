using Parterraria.Content.Items.Board.PartyItems;
using Terraria.ID;
using Terraria.Localization;

namespace Parterraria.Content.NPCs;

/// <summary>
/// Dummy NPC for registering a shop.
/// </summary>
public class ShopNPC : ModNPC
{
    //public override string Texture => "Terraria/Images/NPC_" + NPCID.Guide;

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