using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TdkDataService;
using TdkDataService.Model;
using Template10.Mvvm;
using Template10.Services.NavigationService;
using Windows.ApplicationModel.Resources;
using Windows.Networking.Connectivity;
using Windows.UI.Xaml.Navigation;

namespace TdkDictionaryWin10.ViewModels
{
    public class ProverbsDictionaryViewModel : ViewModelBase
    {
        private IDictionaryDataService _dataService = new DictionaryDataService();

        public class MatchTypeItem
        {
            public String Key { get; set; }
            public String Value { get; set; }
        }

        public ProverbsDictionaryViewModel()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                Value = "Designtime value";
            }

            //ResourceLoader resourceLoader = new ResourceLoader();

            MatchTypes = new ObservableCollection<MatchTypeItem>
            {
                //TODO: new MatchTypeItem {Key = "InProverb", Value = resourceLoader.GetString("ComboBoxItemInProverb") },
                //TODO: new MatchTypeItem {Key = "InMeaning", Value = resourceLoader.GetString("ComboBoxItemInMeaning") }
                new MatchTypeItem {Key = "InProverb", Value = "Atasözü / Deyimde" },
                new MatchTypeItem {Key = "InMeaning", Value = "Anlamda" }
            };

            MatchType = MatchTypes[0];

            Proverbs = new ObservableCollection<Proverb>();
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

        private ObservableCollection<Proverb> _proverb;
        public ObservableCollection<Proverb> Proverbs
        {
            get { return this._proverb; }
            set { Set(ref this._proverb, value); }
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


        DelegateCommand<string> _SearchProverbs;
        public DelegateCommand<string> SearchProverbs =>
            _SearchProverbs
                ?? (_SearchProverbs = new DelegateCommand<string>((word) =>
                {
                    Debug.WriteLine("SearchProverbs");
                    ListProverbs(null, word);
                }, (word) => !String.IsNullOrEmpty(word))
                   );


        private async void ListProverbs(Nullable<int> id, String name)
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

            if (Proverbs != null)
                Proverbs.Clear();
            IsNoResultFound = false;

            ProverbsDictionaryFilter filter = new ProverbsDictionaryFilter();
            filter.SearchString = name;
            filter.SearchId = id;

            if (MatchType.Key.Equals("InProverb"))
            {
                filter.MatchType = ProverbsDictionaryFilter.MatchTypeFilter.IN_PROVERB;
            }
            else
            {
                filter.MatchType = ProverbsDictionaryFilter.MatchTypeFilter.IN_MEANING;
            }

            try
            {
                ProverbsDictionarySearchResult result = await _dataService.SearchProverbsDictionary(filter,
                    () => { Views.Shell.SetBusy(true, "Lütfen Bekleyiniz"); },
                    () => { Views.Shell.SetBusy(false); }
                );

                Proverbs = new ObservableCollection<Proverb>(result.Proverbs);
                if (Proverbs.Count == 0)
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
