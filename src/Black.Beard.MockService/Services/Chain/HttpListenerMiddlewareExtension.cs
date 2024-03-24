namespace Bb.Services.Chain
{
    public static class HttpListenerMiddlewareExtension
    {

        /// <summary>
        /// Uses the HTTP information logger.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static IApplicationBuilder UseListener(this IApplicationBuilder builder) 
            => builder.UseMiddleware<HttpListenerMiddleware>();

    }


}
