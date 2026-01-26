namespace Parterraria.Common;

internal static class PlayerExtensions
{
    public static void QuickDismount(this Player player) => player.mount.Dismount(player);
}
