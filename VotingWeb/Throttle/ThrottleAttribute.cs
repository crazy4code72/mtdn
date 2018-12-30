namespace VotingWeb.Model
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Collections.Concurrent;

    /// <summary>
    /// Throttle attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ThrottleAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Parameter on which throttling will be applied.
        /// </summary>
        public ThrottleOn ThrottleOn { get; set; }

        /// <summary>
        /// Time duration in seconds.
        /// User (ip address) will be able to make only 'AllowedRequestCount' number of requests in this time period.
        /// </summary>
        public int ExpiryTimeInSeconds { get; set; }

        /// <summary>
        /// Allowed request count.
        /// </summary>
        public int MaximumRequestCount { get; set; }

        /// <summary>
        /// Cache.
        /// </summary>
        private ConcurrentDictionary<string, ThrottleInfo> cache = new ConcurrentDictionary<string, ThrottleInfo>();

        /// <summary>
        /// On action execution.
        /// </summary>
        /// <param name="filterContext">Filter context</param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            try
            {
                string key = string.Empty;
                switch (ThrottleOn)
                {
                    case ThrottleOn.IpAddress: key = filterContext.HttpContext.Connection.RemoteIpAddress.ToString(); break;
                    case ThrottleOn.Path: key = filterContext.HttpContext.Request.Path.ToString(); break;
                    default: key = filterContext.HttpContext.Connection.RemoteIpAddress.ToString(); break;
                }

                ThrottleInfo throttleInfo = cache.ContainsKey(key) ? cache[key] : null;
                if (throttleInfo == null || throttleInfo.ExpiresAt <= DateTime.UtcNow)
                {
                    throttleInfo = new ThrottleInfo
                    {
                        ExpiresAt = DateTime.UtcNow.AddSeconds(ExpiryTimeInSeconds),
                        RequestCount = 0
                    };
                };

                throttleInfo.RequestCount++;
                cache[key] = throttleInfo;

                if (throttleInfo.RequestCount > MaximumRequestCount)
                {
                    filterContext.Result = new ContentResult
                    {
                        StatusCode = (int)Enums.ResponseMessageCode.Success,
                        Content = Enums.ResponseMessageCode.TooManyTries.ToString()
                    };
                }
            }
            catch (Exception) { }
        }
    }
}