using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TdkDataService;
using TdkDataService.Filters;
using TdkDataService.Model;
using TdkDataService.Model.Entity;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Navigation;

namespace TdkDictionaryWin10.ViewModels
{
    public class BigTurkishDictionaryViewModel : ViewModelBase
    {
        private IDictionaryDataService _dataService = new DictionaryDataService();

        public class MatchTypeItem
        {
            public String Key { get; set; }
            public String Value { get; set; }
        }

        public BigTurkishDictionaryViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }

            //ResourceLoader resourceLoader = new ResourceLoader();

            MatchTypes = new ObservableCollection<MatchTypeItem>
            {
                //TODO: new MatchTypeItem {Key = "FullMatch", Value = resourceLoader.GetString("ComboBoxItemFullMatch") },
                //TODO: new MatchTypeItem {Key = "PartialMatch", Value = resourceLoader.GetString("ComboBoxItemPartialMatch") }
                new MatchTypeItem {Key = "FULL_MATCH", Value = "Tam sözcük" },
                new MatchTypeItem {Key = "PARTIAL_MATCH", Value = "1. ve/veya 2. kelimesi ... başlayan" }
            };

            MatchType = MatchTypes[0];

            Words = new ObservableCollection<Word>();
        }

        #region Data Members

        public ObservableCollection<MatchTypeItem> MatchTypes { get; private set; }

        private MatchTypeItem _matchType;
        public MatchTypeItem MatchType
        {
            get { return this._matchType; }
            set { Set(ref this._matchType, value); }
        }

        private String _Value = String.Empty;
        public String Value
        {
            get { return _Value; }
            set { Set(ref _Value, value); }
        }

        private ObservableCollection<Word> _words;
        public ObservableCollection<Word> Words
        {
            get { return this._words; }
            set { Set(ref this._words, value); }
        }

        private Boolean _isNoResultFound = false;

        public Boolean IsNoResultFound
        {
            get { return this._isNoResultFound; }
            set
            {
                Set(ref this._isNoResultFound, value);
            }
        }

        private Boolean _isSuggesstion = false;

        public Boolean IsSuggestion
        {
            get { return this._isSuggesstion; }
            set
            {
                Set(ref this._isSuggesstion, value);
            }
        }

        private Boolean _isPartialMatch = false;

        public Boolean IsPartialMatch
        {
            get { return this._isPartialMatch; }
            set
            {
                Set(ref this._isPartialMatch, value);
            }
        }

        #endregion

        public override Task OnNavigatedToAsync(object parameter, NavigationMode mode, IDictionary<string, object> state)
        {
            if (state.Any())
            {
                Value = state[nameof(Value)]?.ToString();
                state.Clear();
            }
            return Task.CompletedTask;
        }

        public override Task OnNavigatedFromAsync(IDictionary<string, object> state, bool suspending)
        {
            if (suspending)
            {
                state[nameof(Value)] = Value;
            }
            return Task.CompletedTask;
        }

        public override Task OnNavigatingFromAsync(NavigatingEventArgs args)
        {
            args.Cancel = false;
            return Task.CompletedTask;
        }

        public void GotoSettings() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 0);

        public void GotoPrivacy() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 1);

        public void GotoAbout() =>
            NavigationService.Navigate(typeof(Views.SettingsPage), 2);


        DelegateCommand<string> _SearchWords;
        public DelegateCommand<string> SearchWords =>
            _SearchWords
                ?? (_SearchWords = new DelegateCommand<string>((word) =>
                {
                    Debug.WriteLine("SearchWords");
                    ListWords(null, word);
                }, (word) => !String.IsNullOrEmpty(word))
                   );


        private async void ListWords(Nullable<int> id, String name)
        {
            if (String.IsNullOrWhiteSpace(name) || MatchType == null)
                return;

            ConnectionProfile internetConnectionProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetConnectionProfile == null || internetConnectionProfile.GetNetworkConnectivityLevel() != NetworkConnectivityLevel.InternetAccess)
            {
                ResourceLoader resourceLoader = new ResourceLoader();
                //await AlertService.ShowAlertAsync(resourceLoader.GetString("ErrorHeader"), resourceLoader.GetString("InternetNotAvailableErrorMessage"));
                return;
            }

            if (Words != null)
                Words.Clear();
            IsNoResultFound = false;
            IsSuggestion = false;

            BigTurkishDictionaryFilter filter = new BigTurkishDictionaryFilter();
            filter.SearchString = name;
            filter.SearchId = id;

            if (MatchType.Key.Equals("FULL_MATCH"))
            {
                filter.Match = DictionaryServiceEnumerations.MatchType.FULL_MATCH;
                IsPartialMatch = false;
            }
            else
            {
                filter.Match = DictionaryServiceEnumerations.MatchType.PARTIAL_MATCH;
                IsPartialMatch = true;
            }

            try
            {
                BigTurkishDictionarySearchResult result = await _dataService.SearchBigTurkishDictionary(filter,
                    () => { Views.Shell.SetBusy(true, "Lütfen Bekleyiniz"); },
                    () => { Views.Shell.SetBusy(false); }
                );

                IsSuggestion = result.IsSuggestion;

                Words = new ObservableCollection<Word>(result.Words);
                if (Words.Count == 0)
                    IsNoResultFound = true;
            }
            catch (Exception e)
            {
                if (e.Message == HttpStatusCode.InternalServerError.ToString())
                {
                    IsNoResultFound = false;

                    ResourceLoader resourceLoader = new ResourceLoader();
                    //AlertService.ShowAlertAsync(resourceLoader.GetString("ErrorHeader"), resourceLoader.GetString("ServerErrorMessage"));
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
