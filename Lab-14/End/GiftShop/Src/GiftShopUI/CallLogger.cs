 
using System;
using System.IO;
using System.Diagnostics.Tracing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Headers;
using System.Linq;

using Castle.DynamicProxy;

namespace GiftShopUI
{
    public class CallLogger : IInterceptor
    {
        TextWriter _output;
        // Microsoft.AspNetCore.Http header;
        // private ITracing _tracing;
        // public CallLogger(RequestHeader requestHeader,ITracing tracer)
        public CallLogger()
        {
            _output = Console.Out;
            // header = requestHeader;
            // this._tracing = tracer;
        }

        public void Intercept(IInvocation invocation)
        {              
            // _output.Write("Calling method {0} with parameters {1}...{2}..{3}..{4}",
            // invocation.Method.Name,
            // string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()), header.SpanId, header.ParentSpanId, header.TraceId);

            _output.WriteLine("Calling method {0} with parameters {1}",
            invocation.Method.Name,
            string.Join(", ", invocation.Arguments.Select(a => (a ?? "").ToString()).ToArray()));

            invocation.Proceed();

            _output.WriteLine("Done: result was {0}.", invocation.ReturnValue);

        }
    }

}