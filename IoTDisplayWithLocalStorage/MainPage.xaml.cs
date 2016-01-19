using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace IoTDisplayWithLocalStorage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            var imageLoader = new ImageLoader();

            // start image loading task, don't wait
            var task = ThreadPool.RunAsync(async (source) =>
            {
                await LoadImages(imageLoader);
            });

            // schedule image databaase refresh every 20 seconds
            TimeSpan imageLoadPeriod = TimeSpan.FromSeconds(20);
            ThreadPoolTimer imageLoadTimes = ThreadPoolTimer.CreatePeriodicTimer(
                async (source) =>
                {
                    await LoadImages(imageLoader);
                }, imageLoadPeriod);
        

            TimeSpan displayImagesPeriod = TimeSpan.FromSeconds(5);
            // display new images every five seconds
            ThreadPoolTimer imageDisplayTimer = ThreadPoolTimer.CreatePeriodicTimer(
                async (source) =>
                {
                    // get next image (byte aray) from database
                    var imageBytes = imageLoader.GetNextImage();

                    if (imageBytes != null)
                    {
                        // we have to update UI in UI thread only
                        await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                            async () =>
                            {
                                // create bitmap from byte array
                                BitmapImage bitmap = new BitmapImage();
                                MemoryStream ms = new MemoryStream(imageBytes);
                                await bitmap.SetSourceAsync(ms.AsRandomAccessStream());

                                // display image
                                splashImage.Source = bitmap;
                            }
                        );
                    }
                }, displayImagesPeriod);
                
        }

        private async Task LoadImages(ImageLoader imageLoader)
        {
            // only load images when we are connected
            if (InternetConnectivity.IsConnected())
            {
                // create Flickr reader
                var reader = new FlickrReader("unicorn");

                //load Flickr images
                var images = await reader.GetImages();

                // store images to database
                await imageLoader.StoreImages(images);
            }
        }

    }
}

