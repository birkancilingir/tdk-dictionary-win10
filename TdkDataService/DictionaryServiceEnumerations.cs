using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdkDataService
{
    public class DictionaryServiceEnumerations
    {
        public enum MatchType { FULL_MATCH, PARTIAL_MATCH };

        public enum GenderType { ALL, MAN, WOMAN, BOTH, NONE };

        public enum SearchType { BY_MEANING, BY_NAME };
    }
}
