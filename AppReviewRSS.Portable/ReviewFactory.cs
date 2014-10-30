using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AppReviewRSS.Portable
{
    public class ReviewFactory
    {
        #region public methods
        public static async Task<List<Review>> GetReviews(
            string applicationId, 
            int minimumRatingToIncludeInFeed,
            int maximumRatingToIncludeInFeed)
        {
            List<Review> reviews = new List<Review>();
            string appStoreUrlBase = "http://windowsphone.com/s?appId=";
            
            string appStoreUrl = string.Format(
                "{0}{1}",
                appStoreUrlBase,
                applicationId);

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("user-agent", "Mozilla/5.0 (compatible; MSIE 10.0; Windows NT 6.2; WOW64; Trident/6.0)");

            HttpResponseMessage response = await httpClient.GetAsync(
                appStoreUrl,
                HttpCompletionOption.ResponseContentRead);

            string pageContent = await response.Content.ReadAsStringAsync();
            
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.OptionOutputAsXml = true;
            htmlDocument.LoadHtml(pageContent);
            
            HtmlNode document = htmlDocument.DocumentNode;
            HtmlNode htmlNode = document.ChildNodes["html"];

            HtmlNode reviewNode = htmlNode
                .Descendants("div")
                .Where(n => n.HasAttributes && n.Attributes["id"] != null && n.Attributes["id"].Value.Equals("reviews", StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            
            foreach(HtmlNode review in reviewNode.Descendants("li").Where(n => n.HasAttributes && n.Attributes["itemprop"] != null))
            {
                Review newReview = new Review();

                // 2 div tags in each review: reviewDetails and reviewText
                foreach(HtmlNode reviewSection in review.Descendants("div"))
                {
                    if (reviewSection.HasAttributes && 
                        reviewSection.Attributes["class"] != null && 
                        reviewSection.Attributes["class"].Value.Equals("reviewDetails", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // review details
                        foreach(HtmlNode metaNode in reviewSection.Descendants("meta"))
                        {
                            switch(metaNode.Attributes["itemprop"].Value.ToLower())
                            {
                                case "author":
                                    newReview.Author = metaNode.Attributes["content"].Value;
                                    break;
                                case "datepublished":
                                    newReview.ReviewDate = Convert.ToDateTime(metaNode.Attributes["content"].Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                                case "reviewrating":
                                    newReview.ReviewRating = Convert.ToInt16(metaNode.Attributes["content"].Value, System.Globalization.CultureInfo.InvariantCulture);
                                    break;
                            }
                        }
                    }

                    if (reviewSection.HasAttributes && 
                        reviewSection.Attributes["class"] != null &&
                        reviewSection.Attributes["class"].Value.Equals("reviewText", StringComparison.CurrentCultureIgnoreCase))
                    {
                        // review text
                        newReview.ReviewText = reviewSection.InnerText;
                    }
                }

                if (newReview.ReviewRating >= minimumRatingToIncludeInFeed &&
                    newReview.ReviewRating <= maximumRatingToIncludeInFeed)
                {
                    newReview.StoreUrl = appStoreUrl;
                    newReview.AppId = applicationId;
                    reviews.Add(newReview);
                }
            }

            return reviews
                .OrderByDescending(r => r.ReviewDate)
                .ToList();
        }
        #endregion
    }
}
