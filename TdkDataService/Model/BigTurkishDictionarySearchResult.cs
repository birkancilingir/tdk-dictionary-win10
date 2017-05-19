using System;
using System.Collections.Generic;

namespace TdkDataService.Model
{
    public class BigTurkishDictionarySearchResult
    {
        public BigTurkishDictionarySearchResult(List<Word> words, Boolean isSuggestion)
        {
            this.Words = words;
            this.IsSuggestion = isSuggestion;
        }

        public List<Word> Words { get; private set; }
        public Boolean IsSuggestion { get; private set; }
    }
}
