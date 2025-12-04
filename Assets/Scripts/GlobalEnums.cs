using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GlobalEnums : MonoBehaviour {
    public enum GemType {
        blue,
        green,
        red,
        yellow,
        purple,
        bomb,
        none
    };

    public enum GameState {
        wait,
        move
    }
}

public static class GemTypeExtensions {
    private static readonly List<GlobalEnums.GemType> _allGemTypes;

    public static IReadOnlyList<GlobalEnums.GemType> AllGemTypes => _allGemTypes;
    public static int GemTypeCount { get; }

    static GemTypeExtensions() {
        _allGemTypes = Enum.GetValues(typeof(GlobalEnums.GemType)).Cast<GlobalEnums.GemType>().ToList();
        _allGemTypes.Remove(GlobalEnums.GemType.none);
        GemTypeCount = _allGemTypes.Count;
    }
}