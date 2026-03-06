namespace Parterraria.Common;

internal static class ItemHelper
{
    /// <summary>
    /// Creates an air item.
    /// </summary>
    public static Item Air()
    {
        Item item = new(0);
        item.TurnToAir();
        return item;
    }
}
