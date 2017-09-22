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

    public class NamesDictionaryViewModel : ViewModelBase
    {
        private IDictionaryDataService _dataService = new DictionaryDataService();

        public class DropDownItem
        {
            public String Key { get; set; } // TODO: Key olarak enumaration geçirilmeli
            public String Value { get; set; }
        }

        public NamesDictionaryViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }

            //ResourceLoader resourceLoader = new ResourceLoader();

            MatchTypes = new ObservableCollection<DropDownItem>
            {
                //TODO: new MatchTypeItem {Key = "FullMatch", Value = resourceLoader.GetString("ComboBoxItemFullMatch") },
                //TODO: new MatchTypeItem {Key = "PartialMatch", Value = resourceLoader.GetString("ComboBoxItemPartialMatch") }
                new DropDownItem {Key = "PARTIAL_MATCH", Value = "Benzer" },
                new DropDownItem {Key = "FULL_MATCH", Value = "Aynı" }
            };
            MatchType = MatchTypes[0];

            GenderTypes = new ObservableCollection<DropDownItem>
            {
                new DropDownItem {Key = "ALL", Value = "Tüm adlar arasında"},
                new DropDownItem {Key = "MAN", Value = "Erkek adları arasında"},
                new DropDownItem {Key = "WOMAN", Value = "Kız adları arasında"},
                new DropDownItem {Key = "BOTH", Value = "Erkek veya kız adı olanlar arasında"}
            };
            GenderType = GenderTypes[0];

            SearchTypes = new ObservableCollection<DropDownItem>
            {
                new DropDownItem { Key = "BY_NAME", Value = "Ada göre"},
                new DropDownItem { Key = "BY_MEANING", Value = "Anlama göre"}
            };
            SearchType = SearchTypes[0];

            People = new ObservableCollection<Person>();
        }

        #region Data Members

        public ObservableCollection<DropDownItem> MatchTypes { get; private set; }

        private DropDownItem _matchType;
        public DropDownItem MatchType
        {
            get { return this._matchType; }
            set { Set(ref this._matchType, value); }
        }

        public ObservableCollection<DropDownItem>GenderTypes { get; private set; }

        private DropDownItem _genderType;
        public DropDownItem GenderType
        {
            get { return this._genderType; }
            set { Set(ref this._genderType, value); }
        }

        public ObservableCollection<DropDownItem> SearchTypes { get; private set; }

        private DropDownItem _searchType;
        public DropDownItem SearchType
        {
            get { return this._searchType; }
            set { Set(ref this._searchType, value); }
        }

        private String _Value = String.Empty;
        public String Value
        {
            get { return _Value; }
            set { Set(ref _Value, value); }
        }

        private ObservableCollection<Person> _people;
        public ObservableCollection<Person> People
        {
            get { return this._people; }
            set { Set(ref this._people, value); }
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

            if (People != null)
                People.Clear();

            NamesDictionaryFilter filter = new NamesDictionaryFilter();
            filter.SearchString = name;
            filter.SearchId = id;

            if (MatchType.Key.Equals("FULL_MATCH"))
                filter.Match = DictionaryServiceEnumerations.MatchType.FULL_MATCH;
            else
                filter.Match = DictionaryServiceEnumerations.MatchType.PARTIAL_MATCH;

            if (GenderType.Key.Equals("ALL"))
                filter.Gender = DictionaryServiceEnumerations.GenderType.ALL;
            else if (GenderType.Key.Equals("MAN"))
                filter.Gender = DictionaryServiceEnumerations.GenderType.MAN;
            else if (GenderType.Key.Equals("WOMAN"))
                filter.Gender = DictionaryServiceEnumerations.GenderType.WOMAN;
            else
                filter.Gender = DictionaryServiceEnumerations.GenderType.BOTH;
            
            if (MatchType.Key.Equals("BY_NAME"))
                filter.Search = DictionaryServiceEnumerations.SearchType.BY_NAME;
            else
                filter.Search = DictionaryServiceEnumerations.SearchType.BY_MEANING;
            
            try
            {
                NamesDictionarySearchResult result = await _dataService.SearchNamesDictionary(filter,
                    () => { Views.Shell.SetBusy(true, "Lütfen Bekleyiniz"); },
                    () => { Views.Shell.SetBusy(false); }
                );

                People = new ObservableCollection<Person>(result.People);
                if (People.Count == 0)
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
