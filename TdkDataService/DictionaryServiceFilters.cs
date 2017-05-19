using System;

namespace TdkDataService
{
    public abstract class BaseFilter
    {
        public String SearchString { get; set; }
        public Nullable<int> SearchId { get; set; }
    }

    public class BigTurkishDictionaryFilter : BaseFilter
    {
        public enum MatchTypeFilter { FULL_MATCH, PARTIAL_MATCH };

        public MatchTypeFilter MatchType { get; set; }
    }

    public class ProverbsDictionaryFilter : BaseFilter
    {
        public enum MatchTypeFilter { IN_PROVERB, IN_MEANING };

        public MatchTypeFilter MatchType { get; set; }
    }
}
