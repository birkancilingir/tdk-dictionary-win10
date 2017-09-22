using System;

namespace TdkDataService.Filters
{
    public abstract class BaseFilter
    {
        public String SearchString { get; set; }
        public Nullable<int> SearchId { get; set; }
    }
}
