using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace Parterraria;

internal class ParterrariaServerConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ServerSide;

    [DefaultValue(false)]
    public bool AlwaysAdventureOnServer;
}
