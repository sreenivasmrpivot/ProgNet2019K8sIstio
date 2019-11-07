using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace GiftShopUI.Middleware
{
    public class IstioTracingMiddleware : IMiddleware
    {
        public static readonly List<string> RequestHeaders = new List<string>();
        public static readonly List<string> ResponseHeaders = new List<string>();

        public IstioTracingMiddleware()
        {
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            AssignIstioHeaders(context);
            await next(context);
        }

        private void AssignIstioHeaders(HttpContext context)
        {
            List<string> istioHeaders = new List<string>() {
                "x-request-id",
                "x-b3-traceid",
                "x-b3-spanid",
                "x-b3-parentspanid",
                "x-b3-sampled",
                "x-b3-flags",
                "x-ot-span-context"};

            foreach(var hdr in istioHeaders)
            {
                context.Response.Headers[hdr] = context.Request.Headers[hdr];
            }
        }
    }
}