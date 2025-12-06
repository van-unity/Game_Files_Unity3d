using System;
using System.Collections.Generic;
using System.Linq;

public enum GemColor {
    Blue,
    Green,
    Red,
    Yellow,
    Purple
}

public enum GemType {
    Regular,
    Bomb
}

public static class GemTypeExtensions {
    private static readonly List<GemColor> _gemColors;
    private static readonly List<GemType> _gemTypes;

    public static IReadOnlyList<GemColor> GemColors => _gemColors;
    public static IReadOnlyList<GemType> GemTypes => _gemTypes;
    
    static GemTypeExtensions() {
        _gemColors = Enum.GetValues(typeof(GemColor)).Cast<GemColor>().ToList();
        _gemTypes = Enum.GetValues(typeof(GemType)).Cast<GemType>().ToList();
    }
}