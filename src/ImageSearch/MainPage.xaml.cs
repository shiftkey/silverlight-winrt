using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


#if SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif

namespace ImageSearch
{
  public partial class MainPage : UserControl
  {
    IList<object>       Images { get; set; }

    public MainPage() : this ("Seattle") { }

    public MainPage(string query)
    {
      InitializeComponent();

      // Setup Binding
      InitializeDatabinding();

      // Load images (default to Seattle)
      QueryImages(query);
    }

    public void QueryImages(string query)
    {
      // Get Images
      ImageResult.GetImages(query);
    }

    #region MainPage Implementation
    private void InitializeDatabinding()
    {
      if (null == FindName("lb"))
      {
        Images = ImageResult.GetEmptyList();
        cvs.Source = Images;
      }
 
      // Completed handler
      ImageResult.DownloadCompleted += (o, args) =>
      {
        if (null != args.Results)
        {
          // Bind results
          headerText.Text = args.Query;

          if (null == Images)
          {
            cvs.Source = args.Results;
          }
          else
          {
            int idx = 0;
            int count = Images.Count;

            // Add new images
            foreach (ImageResult ir in args.Results)
            {
              if (idx < count)
              {
                Images.RemoveAt(idx);
              }
              Images.Insert(idx, ir);
              idx++;
            }

            // Remove extras
            while (count > idx)
            {
              --count;
              Images.RemoveAt(idx);
            }
          }
        }
      };

    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
      string query = headerText.Text.Trim();

      if (query.Length > 0)
      {
        QueryImages(query);
      }
    }

    #endregion
  }
}