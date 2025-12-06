using Pools;

public class GemView : SpawnableMonoBehaviour<GemView> {
    public Gem Gem { get; private set; }

    public void Bind(Gem gem) {
        Gem = gem;
    }
}