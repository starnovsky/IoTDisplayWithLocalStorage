using SQLite.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http;

namespace IoTDisplayWithLocalStorage
{
    public sealed class ImageLoader
    {
        private int? _currentImageIdx = null;
        private bool _isWritingImages = false;

        private SQLiteConnection GetConnection()
        {
            var dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");

            return new SQLiteConnection(new SQLite.Net.Platform.WinRT.SQLitePlatformWinRT(), dbPath);
        }

        public byte[] GetNextImage()
        {
            try
            {
                // get SQLite connection
                using (var conn = GetConnection())
                {
                    // get table reference
                    var table = conn.Table<StoredImage>();

                    // select image ids
                    var ids = table.Select(i => i.ImageID).ToList();
                    int? imageID = null;

                    if (!ids.Any())
                    {
                        return null;
                    }

                    // get next image id
                    if (!_currentImageIdx.HasValue || _currentImageIdx >= ids.Count)
                    {
                        imageID = ids.First();
                        _currentImageIdx = 0;
                    }
                    else
                    {
                        imageID = ids[_currentImageIdx.Value];
                        _currentImageIdx++;
                    }

                    // select image bytes from database
                    return table
                        .Where(i => i.ImageID == imageID)
                        .Select(i => i.Image)
                        .FirstOrDefault();
                }
            }
            catch(Exception)
            {
                // we may be geting database deadlocks from time to time
                // because we reading and writing to the same table at the same time
                return null;
            }
        }

        public async Task StoreImages(IEnumerable<string> images)
        {
            if(_isWritingImages)
            {
                return;
            }

            _isWritingImages = true;

            // get SQLite connection
            using (var conn = GetConnection())
            {
                // ensure the table is created
                conn.CreateTable<StoredImage>();

                // get table reference
                var table = conn.Table<StoredImage>();

                // clear the table
                conn.DeleteAll<StoredImage>();

                foreach (string imageUrl in images)
                {
                    using (var client = new HttpClient())
                    {
                        // load image from Flickr into byte array
                        var response = await client.GetAsync(new Uri(imageUrl));
                        var buffer = await response.Content.ReadAsBufferAsync();
                        byte[] rawBytes = new byte[buffer.Length];
                        using (var reader = DataReader.FromBuffer(buffer))
                        {
                            reader.ReadBytes(rawBytes);
                        }

                        // create new database record
                        var image = new StoredImage()
                        {
                            Image = rawBytes
                        };

                        try
                        {
                            // try to insert it to database
                            conn.Insert(image);
                        }
                        catch(Exception)
                        {
                            // we may be geting database deadlocks from time to time
                            // because we reading and writing to the same table at the same time
                        }
                    }

                }
            }

            _isWritingImages = false;
        }        
    }
}
