namespace Apps.ServiceNow.Utils;

public static class TagHelper
{
    public static bool MatchesTagFilter(
        HashSet<string> tags,
        IReadOnlyCollection<string>? all,
        IReadOnlyCollection<string>? any,
        IReadOnlyCollection<string>? exclude)
    {
        if (exclude is { Count: > 0 } && tags.Overlaps(exclude)) 
            return false;
        
        if (all is { Count: > 0 } && !tags.IsSupersetOf(all)) 
            return false;
        
        if (any is { Count: > 0 } && !tags.Overlaps(any)) 
            return false;
        
        return true;
    }
}