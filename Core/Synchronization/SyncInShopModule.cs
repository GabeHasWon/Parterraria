using NetEasy;
using System;
using Terraria.ID;

namespace Parterraria.Core.Synchronization;

[Serializable]
public class SyncInShopModule(byte who, bool inShop) : Module
{
    public class SyncInShopPlayer : ModPlayer 
    {
        bool wasNpcShop = false;

        public override void PostUpdate()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                return;

            bool inShop = Main.npcShop > 0;

            if (wasNpcShop != inShop)
                new SyncInShopModule((byte)Main.myPlayer, inShop).Send();

            wasNpcShop = inShop;
        }
    }

    internal static bool[] PlayerInShop = new bool[Main.maxPlayers];

    private readonly int _who = who;
    private readonly bool _inShop = inShop;

    public static bool InShop(int who) => PlayerInShop[who];

    protected override void Receive() => PlayerInShop[_who] = _inShop;
}

