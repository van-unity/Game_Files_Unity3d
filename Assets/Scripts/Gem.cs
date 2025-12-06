using System;

[Serializable]
public class Gem : IEquatable<Gem> {
    public GemColor Color { get; }
    public GemType Type { get; }
    public int ScoreValue { get; }

    public Gem(GemColor color, GemType type, int scoreValue) {
        Color = color;
        Type = type;
        ScoreValue = scoreValue;
    }

    public bool Equals(Gem other) {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Color == other.Color && Type == other.Type && ScoreValue == other.ScoreValue;
    }

    public override bool Equals(object obj) {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Gem)obj);
    }

    public override int GetHashCode() {
        return HashCode.Combine((int)Color, (int)Type, ScoreValue);
    }
}