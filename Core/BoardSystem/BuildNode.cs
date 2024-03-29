namespace Parterraria.Core.BoardSystem;

internal class BuildNode(Vector2 position, float baseWidth, string nodeType)
{
    public readonly string nodeType = nodeType;

    public Vector2 position = position;
    public float width = baseWidth;
    public bool settingWidth = false;
}
