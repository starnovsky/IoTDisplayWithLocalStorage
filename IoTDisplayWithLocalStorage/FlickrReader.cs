using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Xml.Dom;

namespace IoTDisplayWithLocalStorage
{
    public sealed class FlickrReader
    {
        private const string FeedBaseUrl = "https://api.flickr.com/services/feeds/photos_public.gne";

        private List<string> _images = null;
        private int _currentIndex = 0;
        private bool _loading = false;
        private readonly string _tags;

        public FlickrReader(string tags)
        {
            _tags = tags;
        }

        public async Task<IEnumerable<string>> GetImages()
        {
            // build request to flickr image feed
            var url = FeedBaseUrl + "?tags=" + _tags;

            // load image feed
            var feed = await XmlDocument.LoadFromUriAsync(new Uri(url));

            // parse out images from ATOM 1.0 feed
            _images = feed
                .DocumentElement
                .ChildNodes
                .Where(n => n.NodeName == "entry")
                .SelectMany(n => n.ChildNodes)
                .Where(n => n.NodeName == "link")
                .Where(l => l.Attributes.Any(a => a.NodeName == "rel" && (string)a.NodeValue == "enclosure"))
                .SelectMany(e => e.Attributes.Where(a => a.NodeName == "href")
                .Select(a => (string)a.NodeValue))
                .ToList();

            return _images;
        }

        public async Task<string> GetImage()
        {
            if(_loading)
            {
                return null;
            }

            if(_images == null)
            {
                // prevent from loading images multiple times
                _loading = true;

                // build request to flickr image feed
                var url = FeedBaseUrl + "?tags=" + _tags;

                // load image feed
                var feed = await XmlDocument.LoadFromUriAsync(new Uri(url));

                // parse out images from ATOM 1.0 feed
                _images = feed
                    .DocumentElement
                    .ChildNodes
                    .Where(n => n.NodeName == "entry")
                    .SelectMany(n => n.ChildNodes)
                    .Where(n => n.NodeName == "link")
                    .Where(l => l.Attributes.Any(a => a.NodeName == "rel" && (string)a.NodeValue == "enclosure"))
                    .SelectMany(e => e.Attributes.Where(a => a.NodeName == "href")
                    .Select(a => (string)a.NodeValue))
                    .ToList();

                _loading = false;
            }

            if(_currentIndex >= _images.Count)
            {
                _currentIndex = 0;                
            }


            return _currentIndex < _images.Count ? _images[_currentIndex++] : null;
        }
    }
}
