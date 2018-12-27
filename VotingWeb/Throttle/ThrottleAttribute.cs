namespace VotingWeb.Model
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    using System;
    using System.Web;
    using System.Web.Caching;

    /// <summary>
    /// Enum for parameter to throttle on.
    /// </summary>
    public enum ThrottleOn
    {
        IpAddress = 1,
        Path = 2
    }

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
        public int TimeDurationInSeconds { get; set; }

        /// <summary>
        /// Allowed request count.
        /// </summary>
        public int AllowedRequestCount { get; set; }

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

                var currentRequestCount = HttpRuntime.Cache[key] == null ? 1 : (int)HttpRuntime.Cache[key] + 1;
                HttpRuntime.Cache.Insert(key, currentRequestCount, null, DateTime.UtcNow.AddSeconds(TimeDurationInSeconds), Cache.NoSlidingExpiration, CacheItemPriority.Low, null);

                if (currentRequestCount > AllowedRequestCount)
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