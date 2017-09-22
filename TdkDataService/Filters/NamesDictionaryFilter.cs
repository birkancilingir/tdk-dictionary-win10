using static TdkDataService.DictionaryServiceEnumerations;

namespace TdkDataService.Filters
{
    public class NamesDictionaryFilter : BaseFilter
    {
        public MatchType Match { get; set; }

        public GenderType Gender { get; set; }

        public SearchType Search { get; set; }

        public int PageNumber { get; set; }

    }
}
