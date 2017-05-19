using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TdkDataService.Model;
using Windows.Web.Http;

namespace TdkDataService
{
    public class DictionaryDataService : IDictionaryDataService
    {
        private static async Task<string> PostRequest(String uri, Dictionary<String, String> parameters)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    IHttpContent content = new HttpFormUrlEncodedContent(parameters);
                    HttpResponseMessage response = await client.PostAsync(new Uri(uri), content);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            throw new Exception(HttpStatusCode.InternalServerError.ToString());
                        }
                        else
                        {
                            // Throw default exception for other errors
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }

                throw;
            }
        }

        private static async Task<string> GetRequest(string uri)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(new Uri(uri));
                    if (!response.IsSuccessStatusCode)
                    {
                        if (response.StatusCode == HttpStatusCode.InternalServerError)
                        {
                            throw new Exception(HttpStatusCode.InternalServerError.ToString());
                        }
                        else
                        {
                            // Throw default exception for other errors
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    return await response.Content.ReadAsStringAsync();
                }
            }
            catch (Exception)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    // An unhandled exception has occurred; break into the debugger
                    System.Diagnostics.Debugger.Break();
                }

                throw;
            }
        }

        public async Task<BigTurkishDictionarySearchResult> SearchBigTurkishDictionary(BigTurkishDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds)
        {
            onLoadingStarts();

            string responseBody = null;

            string uri = DictionaryServiceConstants.SITE_ENDPOINT;
            if (filter.SearchId == null)
            {
                uri = uri + "?option=com_bts&arama=kelime";
                Dictionary<String, String> parameters = new Dictionary<String, String>
                {
                    {"gonder", "ARA"},
                    {"kategori", "verilst"},
                    {"kelime", System.Net.WebUtility.UrlEncode(filter.SearchString)}
                };

                switch (filter.MatchType)
                {
                    case BigTurkishDictionaryFilter.MatchTypeFilter.FULL_MATCH:
                        parameters.Add("ayn", "tam");
                        break;
                    case BigTurkishDictionaryFilter.MatchTypeFilter.PARTIAL_MATCH:
                        parameters.Add("ayn", "dzn");
                        break;
                    default:
                        parameters.Add("ayn", "tam");
                        break;
                }

                responseBody = await PostRequest(uri, parameters);
            }
            else {
                uri = uri + "?option=com_bts&view=bts&kategori1=veritbn&kelimesec=" + filter.SearchId;

                responseBody = await GetRequest(uri);
            }

            List<Word> words = new List<Word>();
            Boolean isSuggestion = false;

            using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(responseBody))))
            {
                String responseHtml = reader.ReadToEnd();

                // Clear illegal tags as they cause errors while parsing
                responseHtml = responseHtml.Replace("< ar", "ar");
                responseHtml = responseHtml.Replace("<ar", "ar");
                responseHtml = responseHtml.Replace("< Ar", "Ar");
                responseHtml = responseHtml.Replace("<Ar", "Ar");

                HtmlDocument doc = new HtmlDocument();
                doc.OptionFixNestedTags = true;
                doc.LoadHtml(responseHtml);

                HtmlNode rootNode = doc.DocumentNode
                    .Descendants("div")
                    .Where(div => div.GetAttributeValue("class", "") == "main_body")
                    .First();

                if (rootNode != null)
                {
                    if (filter.MatchType == BigTurkishDictionaryFilter.MatchTypeFilter.PARTIAL_MATCH)
                    {
                        List<HtmlNode> nodes = doc.DocumentNode
                                .Descendants()
                                .Where(node => (node.Name == "a"
                                                && node.GetAttributeValue("href", "").StartsWith("/index.php?option=com_bts")
                                                && (node.ParentNode.Name != "span" || node.ParentNode.GetAttributeValue("class", "") != "comicm")
                                                && node.ParentNode.Name != "li")).ToList();
                        if (nodes.Count() > 0)
                        {
                            // Check for suggestions
                            foreach (HtmlNode node in nodes)
                            {
                                Word word = new Word();

                                int id;
                                if (Int32.TryParse(node.GetAttributeValue("href", "").Substring(node.GetAttributeValue("href", "").LastIndexOf("kelimesec=") + 10), out id))
                                    word.Id = id;
                                else
                                    word.Id = null;
                                word.Name = node.InnerText.Replace("&nbsp;", "").Trim();
                                word.Origin = String.Empty;
                                word.Description = String.Empty;
                                word.DictionaryName = String.Empty;
                                word.Year = null;

                                words.Add(word);
                            }

                            List<String> pages = doc.DocumentNode
                                .Descendants()
                                .Where(node => (node.Name == "option" && node.ParentNode.GetAttributeValue("name", "") != "ayn"))
                                .Select(node => (node.GetAttributeValue("value", "")))
                                .ToList();

                            foreach (String page in pages)
                            {
                                String nextUri = DictionaryServiceConstants.SITE_ENDPOINT
                                    + "?option=com_bts&view=bts&kategori1=verilst&ayn1=dzn&konts=" + page + "&kelime1=" + System.Net.WebUtility.UrlEncode(filter.SearchString);

                                responseBody = await GetRequest(nextUri);

                                using (StreamReader nextReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(responseBody))))
                                {
                                    HtmlDocument nextdoc = new HtmlDocument();
                                    nextdoc.OptionFixNestedTags = true;
                                    nextdoc.LoadHtml(nextReader.ReadToEnd());

                                    rootNode = nextdoc.DocumentNode
                                        .Descendants("div")
                                        .Where(div => div.GetAttributeValue("class", "") == "main_body")
                                        .First();

                                    if (rootNode != null)
                                    {
                                        nodes = nextdoc.DocumentNode
                                            .Descendants()
                                            .Where(node => (node.Name == "a"
                                                            && node.GetAttributeValue("href", "").StartsWith("/index.php?option=com_bts")
                                                            && (node.ParentNode.Name != "span" || node.ParentNode.GetAttributeValue("class", "") != "comicm")
                                                            && node.ParentNode.Name != "li")).ToList();

                                        foreach (HtmlNode node in nodes)
                                        {
                                            Word word = new Word();

                                            int id;
                                            if (Int32.TryParse(node.GetAttributeValue("href", "").Substring(node.GetAttributeValue("href", "").LastIndexOf("kelimesec=") + 10), out id))
                                                word.Id = id;
                                            else
                                                word.Id = null;
                                            word.Name = node.InnerText.Replace("&nbsp;", "").Trim();
                                            word.Origin = String.Empty;
                                            word.Description = String.Empty;
                                            word.DictionaryName = String.Empty;
                                            word.Year = null;

                                            words.Add(word);
                                        }
                                    }
                                }
                            }

                            if (words.Count > 0)
                                isSuggestion = true;
                        }
                    }
                    else
                    {
                        if (doc.DocumentNode
                                .Descendants()
                                .Where(node => (node.Name == "span"
                                                && (node.GetAttributeValue("class", "") == "comick"
                                                    || node.GetAttributeValue("class", "") == "comicb"
                                                    || node.GetAttributeValue("class", "") == "comics"))).Count() > 0)
                        {
                            // Words Found
                            isSuggestion = false;

                            List<HtmlNode> nodes = doc.DocumentNode
                                    .Descendants()
                                    .Where(node => (node.Name == "span"
                                                    && (node.GetAttributeValue("class", "") == "comick"
                                                        || node.GetAttributeValue("class", "") == "comicb"
                                                        || node.GetAttributeValue("class", "") == "comics"))
                                                    || (node.Name == "p"
                                                        && node.GetAttributeValue("class", "") == "thomicb")).ToList();

                            for (int i = 0; i < nodes.Count; i = i + 5)
                            {
                                Word word = new Word();
                                word.Id = null;
                                word.Name = nodes[i + 0].InnerText;
                                word.Origin = nodes[i + 1].InnerText;
                                word.Description = nodes[i + 2].InnerHtml;
                                word.DictionaryName = nodes[i + 3].InnerText;

                                int year;
                                if (Int32.TryParse(nodes[i + 4].InnerText, out year))
                                    word.Year = year;
                                else
                                    word.Year = null;

                                words.Add(word);
                            }
                        }
                        else
                        {
                            // Check for suggestions
                            List<HtmlNode> nodes = doc.DocumentNode
                                    .Descendants()
                                    .Where(node => (node.Name == "a"
                                                    && node.ParentNode.Name == "table"
                                                    && node.GetAttributeValue("href", "").StartsWith("/index.php?option=com_bts"))).ToList();

                            foreach (HtmlNode node in nodes)
                            {
                                Word word = new Word();
                                word.Name = node.InnerText;
                                word.Origin = String.Empty;
                                word.Description = String.Empty;
                                word.DictionaryName = String.Empty;
                                word.Year = null;

                                words.Add(word);
                            }

                            if (words.Count > 0)
                                isSuggestion = true;
                        }
                    }
                }
            }

            onLoadingEnds();

            return new BigTurkishDictionarySearchResult(words, isSuggestion);
        }

        public async Task<ProverbsDictionarySearchResult> SearchProverbsDictionary(ProverbsDictionaryFilter filter, Action onLoadingStarts, Action onLoadingEnds)
        {



            onLoadingStarts();

            string responseBody = null;

            string uri = DictionaryServiceConstants.SITE_ENDPOINT + "?option=com_atasozleri&arama=kelime";
            Dictionary<String, String> parameters = new Dictionary<String, String>
            {
                {"gonder", "ARA"},
                {"kategori", "atalst"},
                {"kelime", System.Net.WebUtility.UrlEncode(filter.SearchString)}
            };

            switch (filter.MatchType)
            {
                case ProverbsDictionaryFilter.MatchTypeFilter.IN_PROVERB:
                    parameters.Add("hng", "tam");
                    break;
                case ProverbsDictionaryFilter.MatchTypeFilter.IN_MEANING:
                    parameters.Add("hng", "dzn");
                    break;
                default:
                    parameters.Add("hng", "tam");
                    break;
            }

            responseBody = await PostRequest(uri, parameters);

            List<Word> words = new List<Word>();

            using (StreamReader reader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(responseBody))))
            {
                String responseHtml = reader.ReadToEnd();

                // Clear illegal tags as they cause errors while parsing
                //responseHtml = responseHtml.Replace("< ar", "ar");
                //responseHtml = responseHtml.Replace("<ar", "ar");
                //responseHtml = responseHtml.Replace("< Ar", "Ar");
                //responseHtml = responseHtml.Replace("<Ar", "Ar");

                HtmlDocument doc = new HtmlDocument();
                doc.OptionFixNestedTags = true;
                doc.LoadHtml(responseHtml);

                HtmlNode rootNode = doc.DocumentNode
                    .Descendants("div")
                    .Where(div => div.GetAttributeValue("class", "") == "main_body")
                    .First();

                if (rootNode != null)
                {
                    List<HtmlNode> nodes = doc.DocumentNode
                            .Descendants()
                            .Where(node => (node.Name == "a"
                                            && node.GetAttributeValue("href", "").StartsWith("/index.php?option=com_bts")
                                            && (node.ParentNode.Name != "span" || node.ParentNode.GetAttributeValue("class", "") != "comicm")
                                            && node.ParentNode.Name != "li")).ToList();
                    if (nodes.Count() > 0)
                    {
                        // Check for suggestions
                        foreach (HtmlNode node in nodes)
                        {
                            Word word = new Word();

                            int id;
                            if (Int32.TryParse(node.GetAttributeValue("href", "").Substring(node.GetAttributeValue("href", "").LastIndexOf("kelimesec=") + 10), out id))
                                word.Id = id;
                            else
                                word.Id = null;
                            word.Name = node.InnerText.Replace("&nbsp;", "").Trim();
                            word.Origin = String.Empty;
                            word.Description = String.Empty;
                            word.DictionaryName = String.Empty;
                            word.Year = null;

                            words.Add(word);
                        }

                        List<String> pages = doc.DocumentNode
                            .Descendants()
                            .Where(node => (node.Name == "option" && node.ParentNode.GetAttributeValue("name", "") != "ayn"))
                            .Select(node => (node.GetAttributeValue("value", "")))
                            .ToList();

                        foreach (String page in pages)
                        {
                            String nextUri = DictionaryServiceConstants.SITE_ENDPOINT
                                + "?option=com_bts&view=bts&kategori1=verilst&ayn1=dzn&konts=" + page + "&kelime1=" + System.Net.WebUtility.UrlEncode(filter.SearchString);

                            responseBody = await GetRequest(nextUri);

                            using (StreamReader nextReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(responseBody))))
                            {
                                HtmlDocument nextdoc = new HtmlDocument();
                                nextdoc.OptionFixNestedTags = true;
                                nextdoc.LoadHtml(nextReader.ReadToEnd());

                                rootNode = nextdoc.DocumentNode
                                    .Descendants("div")
                                    .Where(div => div.GetAttributeValue("class", "") == "main_body")
                                    .First();

                                if (rootNode != null)
                                {
                                    nodes = nextdoc.DocumentNode
                                        .Descendants()
                                        .Where(node => (node.Name == "a"
                                                        && node.GetAttributeValue("href", "").StartsWith("/index.php?option=com_bts")
                                                        && (node.ParentNode.Name != "span" || node.ParentNode.GetAttributeValue("class", "") != "comicm")
                                                        && node.ParentNode.Name != "li")).ToList();

                                    foreach (HtmlNode node in nodes)
                                    {
                                        Word word = new Word();

                                        int id;
                                        if (Int32.TryParse(node.GetAttributeValue("href", "").Substring(node.GetAttributeValue("href", "").LastIndexOf("kelimesec=") + 10), out id))
                                            word.Id = id;
                                        else
                                            word.Id = null;
                                        word.Name = node.InnerText.Replace("&nbsp;", "").Trim();
                                        word.Origin = String.Empty;
                                        word.Description = String.Empty;
                                        word.DictionaryName = String.Empty;
                                        word.Year = null;

                                        words.Add(word);
                                    }
                                }
                            }
                        }

                        if (words.Count > 0)
                            isSuggestion = true;
                    }
                }
            }

            onLoadingEnds();

            //return new ProverbsDictionarySearchResult(words);

            return new ProverbsDictionarySearchResult(null);









        }
    }
}
