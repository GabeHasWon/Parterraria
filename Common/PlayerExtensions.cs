namespace Parterraria.Common;

internal static class PlayerExtensions
{
    public static void QuickDismount(this Player player) => player.mount.Dismount(player);

    public static void SafeTeleport(this Player player, Vector2 teleport)
    {
        player.Center = teleport;
        player.fallStart = player.fallStart2 = (int)(player.Center.Y / 16f);
        player.RemoveAllGrapplingHooks();
        player.QuickDismount();
    }
}
