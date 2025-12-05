using System;
using System.Collections.Generic;
using System.Linq;

public enum GemType {
    blue,
    green,
    red,
    yellow,
    purple,
    bomb,
    none
}

public static class GemTypeExtensions {
    private static readonly List<GemType> _allGemTypes;

    public static IReadOnlyList<GemType> AllGemTypes => _allGemTypes;
    public static int GemTypeCount { get; }

    static GemTypeExtensions() {
        _allGemTypes = Enum.GetValues(typeof(GemType)).Cast<GemType>().ToList();
        _allGemTypes.Remove(GemType.none);
        GemTypeCount = _allGemTypes.Count;
    }
}