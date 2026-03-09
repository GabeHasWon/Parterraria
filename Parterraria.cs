global using Terraria.ModLoader;
global using Terraria;
global using ReLogic.Graphics;
global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;

using Terraria.GameContent.UI;
using Terraria.Localization;
using Parterraria.Content.Items.Board;
using System.IO;

namespace Parterraria;

public class Parterraria : Mod
{
    public static int AmethystCurrencyID { get; private set; }

    public override void Load()
    {
        AmethystCurrencyID = CustomCurrencyManager.RegisterCurrency(new AmethystCurrency(ModContent.ItemType<AmethystCoin>(), 999L));
        NPCUtils.NPCUtils.AutoloadModBannersAndCritters(this);
    }

    public override void Unload() => NPCUtils.NPCUtils.UnloadMod(this);

    public override void PostSetupContent() => NetEasy.NetEasy.Register(this);
    public override void HandlePacket(BinaryReader reader, int whoAmI) => NetEasy.NetEasy.HandleModule(reader, whoAmI);

    public class AmethystCurrency(int coinItemID, long currencyCap) : CustomCurrencySingleCoin(coinItemID, currencyCap)
    {
        public override void GetPriceText(string[] lines, ref int currentLine, long price)
        {
            Color color = Color.Purple * (Main.mouseTextColor / 255f);
            lines[currentLine++] = string.Format("[c/{0:X2}{1:X2}{2:X2}:{3} {4} {5}]",
                [
                    color.R,
                    color.G,
                    color.B,
                    Language.GetTextValue("LegacyTooltip.50"),
                    price,
                    Language.GetTextValue("Mods.Parterraria.Amethyst")
                ]);
        }
    }
}