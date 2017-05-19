using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TdkDataService.Model
{
    public class ProverbsDictionarySearchResult
    {
        public ProverbsDictionarySearchResult(List<Proverb> proverb)
        {
            this.Proverbs = proverb;
        }

        public List<Proverb> Proverbs { get; private set; }
    }
}
