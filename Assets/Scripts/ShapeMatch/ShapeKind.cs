namespace ShapeMatch
{
    /// <summary>The four shapes the matcher uses. Index order matches the sprite tables
    /// wired in the scene (white tile sprites and glossy target sprites).</summary>
    public enum ShapeKind
    {
        Circle = 0,
        Square = 1,
        Triangle = 2,
        Star = 3,
    }

    public static class ShapeKindExt
    {
        public const int Count = 4;
        public static string Label(this ShapeKind k) => k switch
        {
            ShapeKind.Circle => "circle",
            ShapeKind.Square => "square",
            ShapeKind.Triangle => "triangle",
            ShapeKind.Star => "star",
            _ => "shape",
        };
    }
}
