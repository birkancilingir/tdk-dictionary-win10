using System;
using System.Threading.Tasks;
using TdkDataService.Filters;
using TdkDataService.Model;

namespace TdkDataService
{
    public interface IDictionaryDataService
    {
        Task<BigTurkishDictionarySearchResult> SearchBigTurkishDictionary(BigTurkishDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds);
        Task<NamesDictionarySearchResult> SearchNamesDictionary(NamesDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds);
    }
}
