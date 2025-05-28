using System.ServiceModel.Syndication;
using System.Web.Mvc;
using System.Xml;

namespace SwiftKampus.Services
{
    public class FeedResult : ActionResult
    {
        private readonly Rss20FeedFormatter _formattedFeed;

        public FeedResult(Rss20FeedFormatter formattedFeed)
        {
            _formattedFeed = formattedFeed;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ContentType = "application/rss+xml";
            using (XmlWriter writer = XmlWriter.Create(context.HttpContext.Response.Output))
            {
                _formattedFeed.WriteTo(writer);
            }
        }
    }
}