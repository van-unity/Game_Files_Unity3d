using System.Collections.Generic;

public class ResolveResult {
    public IEnumerable<CollectedGemInfo> CollectedGems { get; }
    public IEnumerable<ChangeInfo> Changes { get; }

    public ResolveResult(IEnumerable<CollectedGemInfo> collectedGems, IEnumerable<ChangeInfo> changes) {
        CollectedGems = collectedGems;
        Changes = changes;
    }
}