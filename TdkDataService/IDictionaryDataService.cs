using System;
using System.Threading.Tasks;
using TdkDataService.Model;

namespace TdkDataService
{
    public interface IDictionaryDataService
    {
        Task<BigTurkishDictionarySearchResult> SearchBigTurkishDictionary(BigTurkishDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds);

        Task<ProverbsDictionarySearchResult> SearchProverbsDictionary(ProverbsDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds);
    }
}
