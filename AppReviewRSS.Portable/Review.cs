using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppReviewRSS.Portable
{
    public class Review
    {
        #region properties
        public string Author { get; set; }
        public DateTime ReviewDate { get; set; }
        public int ReviewRating { get; set; }
        public string ReviewText { get; set; }
        public string StoreUrl { get; set; }

        public string ReviewTitle
        {
            get
            {
                if (string.IsNullOrEmpty(this.ReviewText))
                {
                    return null;
                }
                else if (this.ReviewText.Length <= 60)
                {
                    return this.ReviewText;
                }
                else
                {
                    return string.Format(
                        "{0}...",
                        this.ReviewText.Substring(0, 57));
                }
            }
            set { }
        }
        #endregion
    }
}
