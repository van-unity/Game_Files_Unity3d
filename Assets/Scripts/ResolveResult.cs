using System;
using System.Collections.Generic;

public class ResolveResult {
    public IEnumerable<CollectedGemInfo> CollectedGems { get; }
    public IEnumerable<CollectedGemInfo> CollectedGemsSpecial { get; }
    public IEnumerable<ChangeInfo> Changes { get; }

    public ResolveResult(
        IEnumerable<CollectedGemInfo> collectedGems,
        IEnumerable<ChangeInfo> changes,
        IEnumerable<CollectedGemInfo> collectedGemsSpecial = null
    ) {
        CollectedGems = collectedGems;
        Changes = changes;
        CollectedGemsSpecial = collectedGemsSpecial ?? ArraySegment<CollectedGemInfo>.Empty;
    }
}