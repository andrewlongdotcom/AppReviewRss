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
    public class SyndicationFeedFormatter
    {
        public static string GetSyndicationFeedAsString(List<Models.ReviewModel> reviews)
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

        private void BuildSyndicationFeed(List<Models.ReviewModel> reviews, Stream stream, string contenttype)
        {
            SyndicationFeed feed = GetSyndicationFeed(reviews);

            using (XmlWriter writer = XmlWriter.Create(stream))
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

        private static SyndicationFeed GetSyndicationFeed(List<Models.ReviewModel> reviews)
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

            foreach (Models.ReviewModel review in reviews)
            {
                items.Add(BuildSyndicationItem(review));
            }

            feed.Items = items;
            return feed;
        }

        private static SyndicationItem BuildSyndicationItem(Models.ReviewModel review)
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
