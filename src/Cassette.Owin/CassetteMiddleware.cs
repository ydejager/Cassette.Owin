﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Owin;

using Owin;

using Trace = Cassette.Diagnostics.Trace;

namespace Cassette.Owin
{
    public class CassetteMiddleware : OwinMiddleware
    {
        internal static string StartUpTrace;

        private static readonly object Lock = new object();

        private static WebHost _host;

        private readonly ThreadLocal<IOwinContext> _contextLocal = new ThreadLocal<IOwinContext>(() => null);
        private readonly CassetteOptions _options;

        public CassetteMiddleware(OwinMiddleware next, IAppBuilder builder, CassetteOptions options)
            : base(next)
        {
            _options = options;

            var context = new OwinContext(builder.Properties);
            var token = context.Get<CancellationToken>(Constants.HostappDisposingKey);
            if (token != CancellationToken.None)
            {
                token.Register(() =>
                {
                    lock (Lock)
                    {
                        if (_host != null)
                        {
                            try
                            {
                                _host.Dispose();
                            }
                            catch (Exception ex)
                            {
                                // nothing to do here...
                            }
                        }
                    }
                });
            }
        }

        public override Task Invoke(IOwinContext context)
        {
            context.Environment.Add(Constants.TimestampKey, DateTime.UtcNow);

            _contextLocal.Value = context;
            lock (Lock)
            {
                if (_host == null)
                {
                    var startupTimer = Stopwatch.StartNew();
                    using (var recorder = new StartUpTraceRecorder())
                    {
                        _host = new WebHost(_options, () => _contextLocal.Value);
                        _host.Initialize();

                        Trace.Source.TraceInformation("Total time elapsed: {0}ms", startupTimer.ElapsedMilliseconds);
                        StartUpTrace = recorder.TraceOutput;
                    }
                }
            }

            _host.StoreRequestContainerInOwinContext();

            var path = context.Request.Path.ToString();

            if (path.StartsWith(_options.RouteRoot) && (path.Length == _options.RouteRoot.Length || path[_options.RouteRoot.Length] == '/'))
            {
                return _host.ProcessRequest(context, Next);
            }

            //return Next.Invoke(context);
            return _host.ProcessRewriteRequest(context, Next);
        }
    }
}