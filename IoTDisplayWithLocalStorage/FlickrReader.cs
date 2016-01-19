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
            var images = feed
                .DocumentElement
                .ChildNodes
                .Where(n => n.NodeName == "entry")
                .SelectMany(n => n.ChildNodes)
                .Where(n => n.NodeName == "link")
                .Where(l => l.Attributes.Any(a => a.NodeName == "rel" && (string)a.NodeValue == "enclosure"))
                .SelectMany(e => e.Attributes.Where(a => a.NodeName == "href")
                .Select(a => (string)a.NodeValue))
                .ToList();

            return images;
        }
    }
}
