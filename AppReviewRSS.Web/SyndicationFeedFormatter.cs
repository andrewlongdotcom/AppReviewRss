using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Security.Policy;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AppReviewRSS.Web
{
    public class SyndicationFeedFormatter : MediaTypeFormatter
    {
        private readonly string atom = "application/atom+xml";
        private readonly string rss = "application/rss+xml";
        private readonly string text = "text/html";

        public SyndicationFeedFormatter()
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(atom));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(rss));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue(text));
        }

        public SyndicationFeedFormatter(string format)
        {
            this.AddUriPathExtensionMapping("rss", new MediaTypeHeaderValue(format));
            this.AddQueryStringMapping("formatter", "rss", new MediaTypeHeaderValue(format));
        }

        Func<Type, bool> SupportedType = (type) =>
        {
            if (type == typeof(Portable.Review) || type == typeof(IEnumerable<Portable.Review>))
                return true;
            else
                return false;
        };

        public override bool CanReadType(Type type)
        {
            return SupportedType(type);
        }

        public override bool CanWriteType(Type type)
        {
            return SupportedType(type);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, System.Net.Http.HttpContent content, System.Net.TransportContext transportContext)
        {
            return Task.Factory.StartNew(() =>
            {
                if (type == typeof(Portable.Review) || type == typeof(IEnumerable<Portable.Review>))
                    BuildSyndicationFeed((List<Portable.Review>)value, writeStream, content.Headers.ContentType.MediaType);
            });
        }

        public static string GetSyndicationFeedAsString(List<Portable.Review> reviews)
        {
            if (reviews == null || reviews.Count == 0)
            {
                return null;
            }

            SyndicationFeed feed = GetSyndicationFeed(reviews);

            using (TextWriter textWriter = new UTF8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(textWriter))
                {
                    Rss20FeedFormatter rssformatter = new Rss20FeedFormatter(feed)
                    {
                        SerializeExtensionsAsAtom = false,
                        PreserveAttributeExtensions = true,
                        PreserveElementExtensions = true
                    };

                    rssformatter.WriteTo(xmlWriter);
                }
                return textWriter.ToString();
            }
        }

        private void BuildSyndicationFeed(List<Portable.Review> reviews, Stream stream, string contenttype)
        {
            SyndicationFeed feed = GetSyndicationFeed(reviews);

            using (XmlWriter writer = XmlWriter.Create(stream))
            {
                if (string.Equals(contenttype, atom))
                {
                    Atom10FeedFormatter atomformatter = new Atom10FeedFormatter(feed);
                    atomformatter.WriteTo(writer);
                }
                else
                {
                    Rss20FeedFormatter rssformatter = new Rss20FeedFormatter(feed)
                        {
                            SerializeExtensionsAsAtom = false,
                            PreserveAttributeExtensions = true,
                            PreserveElementExtensions = true
                        };
                    rssformatter.WriteTo(writer);
                }
            }
        }

        private static SyndicationFeed GetSyndicationFeed(List<Portable.Review> reviews)
        {
            if (reviews == null ||
                reviews.Count == 0)
            {
                return null;
            }

            List<SyndicationItem> items = new List<SyndicationItem>();
            var feed = new SyndicationFeed("WP8 App Reviews", "", new Uri(reviews[0].StoreUrl))
            {
                Id = reviews[0].StoreUrl
            };

            foreach (Portable.Review review in reviews)
            {
                items.Add(BuildSyndicationItem(review));
            }

            feed.Items = items;
            return feed;
        }

        private static SyndicationItem BuildSyndicationItem(Portable.Review review)
        {
            var item = new SyndicationItem(
                string.Format("Another {0}-star review!", review.ReviewRating),
                review.ReviewText,
                new Uri(review.StoreUrl))
            {
                Id = string.Format(
                    "{0}_{1}",
                    review.AppId,
                    review.Author.Replace(" ", string.Empty)),
                LastUpdatedTime = review.ReviewDate.ToLocalTime(),
                PublishDate = review.ReviewDate.ToLocalTime()
            };
            item.Authors.Add(new SyndicationPerson() { Name = review.Author });

            return item;
        }

    }
}
