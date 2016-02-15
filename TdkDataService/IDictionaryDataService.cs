using System;
using System.Threading.Tasks;
using TdkDataService.Model;

namespace TdkDataService
{
    public interface IDictionaryDataService
    {
        Task<SearchResult> SearchBigTurkishDictionary(BigTurkishDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds);
    }
}
