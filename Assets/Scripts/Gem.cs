public class Gem {
    public GemColor Color { get; }
    public GemType Type { get; }
    public int ScoreValue { get; }

    public Gem(GemColor color, GemType type, int scoreValue) {
        Color = color;
        Type = type;
        ScoreValue = scoreValue;
    }
}