namespace Parterraria.Common;

internal class CommonColors
{
    public static readonly Color Error = new(255, 180, 180);
    public static readonly Color Success = new(180, 255, 180);
    public static readonly Color Info = new(228, 247, 246);

    public static readonly Color First = Color.Gold;
    public static readonly Color Second = Color.Silver;
    public static readonly Color Third = Color.SaddleBrown;

    public static Color GetPlacementColor(int placement) => placement switch
    {
        0 => First,
        1 => Second,
        2 => Third,
        _ => Color.White,
    };
}
