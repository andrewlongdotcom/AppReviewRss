﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace AppReviewRSS.Web.Controllers
{
    public class ReviewsController : ApiController
    {
        /// <summary>
        /// Get application reviews
        /// </summary>
        /// <param name="id">The Windows Phone store App ID</param>
        /// <returns></returns>
        public async Task<IEnumerable<Portable.Review>> Get(string id)
        {
            return await Get(id, 5, 5);
        }

        /// <summary>
        /// Get application reviews
        /// </summary>
        /// <param name="id">The Windows Phone store App ID</param>
        /// <param name="minimumReviewValue">The minimum review (star value) to include in the feed</param>
        /// <param name="maximumReviewValue">The maximum review (star value) to include in the feed</param>
        /// <returns></returns>
        public async Task<IEnumerable<Portable.Review>> Get(
            string id,
            Nullable<int> minimumReviewValue,
            Nullable<int> maximumReviewValue)
        {
            if (!minimumReviewValue.HasValue)
            {
                minimumReviewValue = 0;
            }

            if (!maximumReviewValue.HasValue)
            {
                maximumReviewValue = 5;
            }

            if (ConfigurationManager.AppSettings["WhitelistAppIds"].Contains(id))
            {
                return await Portable.ReviewFactory.GetReviews(
                    id,
                    minimumReviewValue.Value,
                    maximumReviewValue.Value);
            }
            else
            {
                return null;
            }
        }
    }
}