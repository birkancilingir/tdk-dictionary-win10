using System.Collections.Generic;
using TdkDataService.Model.Entity;

namespace TdkDataService.Model
{

    public class NamesDictionarySearchResult
    {
        public NamesDictionarySearchResult(List<Person> people, int pageCount)
        {
            this.People = people;
            this.PageCount = pageCount;
        }

        public List<Person> People { get; private set; }
        public int PageCount { get; private set; }
    }
}
