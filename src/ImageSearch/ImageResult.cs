using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Collections;
using System.Collections.ObjectModel;

#if SILVERLIGHT
using System.Windows.Input;
#else
using Windows.UI.Xaml.Input;
#endif

namespace ImageSearch
{
    public class ImageResult
    {
        private static string searchUrl = "http://api.bing.net/xml.aspx?AppId=B24C5222A056E9B833120A7E85994F42D33F9DE6&Version=2.0&Market=en-us&Image.Count=50&Sources=Image&Query=";
        public static event EventHandler<ImageResultEventArgs> DownloadCompleted;

        public string Title { get; set; }
        public string MediaUrl { get; set; }
        public string RefUrl { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }
        public string ThumbnailUrl { get; set; }
        public int ThumbnailWidth { get; set; }
        public int ThumbnailHeight { get; set; }
        public int ThumbnailSize { get; set; }
        public string WidthText { get { return "Width: " + Width.ToString(); } }
        public string HeightText { get { return "Height: " + Height.ToString(); } }
        public string DomainText { get { return new Uri(MediaUrl).Host; } }
        public ICommand NavigatePageCommand { get; set; }

        public ImageResult()
        {
            // Setup command (MVVM Support)
            NavigatePageCommand = new DelegateCommand(NavigatePage, AlwaysTrue);
        }

#if SILVERLIGHT
        public static void GetImages(string query)
#elif WINRT
        public static async void GetImages(string query)
#endif
        {
            // Default query

#if WINRT
            var wc = new System.Net.Http.HttpClient();

            var response = await wc.GetAsync(new Uri(searchUrl + query));
            ParseImages(query, XDocument.Parse(response.Content.ReadAsString()));
            
#elif SILVERLIGHT

            WebClient wc = new WebClient();
            wc.DownloadStringCompleted += new DownloadStringCompletedEventHandler(WebClient_DownloadStringCompleted);
            wc.DownloadStringAsync(new Uri(searchUrl + query), query);

#endif
        }

#if SILVERLIGHT
        static void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                ParseImages((e.UserState as String), XDocument.Parse(e.Result));
            }
        }
#endif

        public static void ParseImages(string query, XDocument imgs)
        {
            XNamespace xmlns = "http://schemas.microsoft.com/LiveSearch/2008/04/XML/multimedia";
            List<ImageResult> imageList = null;

            // Linq Magic
            var images = from img in imgs.Descendants(xmlns + "ImageResult")
                         where !img.Element(xmlns + "MediaUrl").Value.EndsWith(".bmp")
                         select new ImageResult
                         {
                             Title = img.Element(xmlns + "Title").Value,
                             MediaUrl = img.Element(xmlns + "MediaUrl").Value,
                             RefUrl = img.Element(xmlns + "Url").Value,
                             Width = (int)img.Element(xmlns + "Width"),
                             Height = (int)img.Element(xmlns + "Height"),
                             Size = (int)img.Element(xmlns + "FileSize"),
                             ThumbnailUrl = img.Element(xmlns + "Thumbnail").Element(xmlns + "Url").Value,
                             ThumbnailWidth = (int)img.Element(xmlns + "Thumbnail").Element(xmlns + "Width"),
                             ThumbnailHeight = (int)img.Element(xmlns + "Thumbnail").Element(xmlns + "Height"),
                         };

            // Convert to list of ImageResults
            imageList = images.ToList<ImageResult>();

            if (null != DownloadCompleted)
            {
                DownloadCompleted(null, new ImageResultEventArgs(query, imageList));
            }
        }

        public static IList<object> GetEmptyList()
        {
#if SILVERLIGHT
            return new ObservableVector<object>();
#elif WINRT
            return new ObservableCollection<object>();
#endif
        }

        private void NavigatePage(object param)
        {
            string page = param as string;

            if (!string.IsNullOrEmpty(page))
            {
#if WINDOWS_PHONE
        Microsoft.Phone.Tasks.WebBrowserTask task = new Microsoft.Phone.Tasks.WebBrowserTask();
        task.Uri = new Uri(page);
        task.Show();
#elif SILVERLIGHT
        System.Windows.Browser.HtmlPage.Window.Navigate(new Uri(page));
#elif WINRT

#endif
            }
        }

        private bool AlwaysTrue(object param)
        {
            return true;
        }
    }

    #region ImageResultEventArgs
    public class ImageResultEventArgs : EventArgs
    {
        public List<ImageResult> Results { get; set; }
        public string Query { get; set; }
        public ImageResultEventArgs(string query, List<ImageResult> results)
        {
            this.Query = query;
            this.Results = results;
        }
    }
    #endregion

    #region ViewModel Support
    // ToDo: See below for the change
    public class DelegateCommand : ICommand
    {
        Func<object, bool> canExecute;
        Action<object> executeAction;

        public DelegateCommand(Action<object> executeAction, Func<object, bool> canExecute)
        {
            this.executeAction = executeAction;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }
# if SILVERLIGHT
        public event EventHandler CanExecuteChanged { add { } remove { } }
#elif WINRT
        public event Windows.UI.Xaml.EventHandler CanExecuteChanged;
#endif
    }
    #endregion
}


