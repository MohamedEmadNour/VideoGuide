using Microsoft.AspNetCore.Http;
using System;

namespace VideoGuide.Services
{
    public class ImageUrlConverter
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageUrlConverter(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string ConvertToUrl(string localPath)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var domain = $"{request.Scheme}://{request.Host}";

            //// Assuming 'localPath' is an absolute path and you need to convert it into a relative path
            //var webRootPath = Directory.GetCurrentDirectory();
            //if (!localPath.StartsWith(webRootPath))
            //{
            //    throw new ArgumentException("The local path does not start within the web root path.");
            //}

            localPath = localPath.Replace('\\', '/');
            return $"{domain}/{localPath}";
        }
    }
}
