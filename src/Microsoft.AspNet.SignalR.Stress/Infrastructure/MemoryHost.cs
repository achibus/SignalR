﻿using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Transports;
using Microsoft.Owin.Testing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Stress.Infrastructure
{
    public  class MemoryHost : ITestHost
    {
        private readonly TestServer _testServer;
        private readonly IHttpClient _client;
        private TransportType _transportType;
        private bool _disposed;

        public MemoryHost(string transport)
        {
            _testServer = TestServer.Create<Startup>();
            _client = new MemoryClient(_testServer.Handler);

            if (!Enum.TryParse<TransportType>(transport, true, out _transportType))
            {
                // default it to Long Polling for transport
                _transportType = TransportType.LongPolling;
            }

            _disposed = false;
        }

        void ITestHost.Initialize(int? keepAlive,
            int? connectionTimeout,
            int? disconnectTimeout,
            int? transportConnectTimeout,
            int? maxIncomingWebSocketMessageSize,
            bool enableAutoRejoiningGroups)
        {
            _client.Initialize(null);

            (this as ITestHost).TransportFactory = () =>
            {
                switch (_transportType)
                {
                    case TransportType.Websockets:
                        return new WebSocketTransport(_client);
                    case TransportType.ServerSentEvents:
                        return new ServerSentEventsTransport(_client);
                    case TransportType.ForeverFrame:
                        break;
                    case TransportType.LongPolling:
                        return new LongPollingTransport(_client);
                    default:
                        return new AutoTransport(_client);
                }

                throw new NotSupportedException("Transport not supported");
            };
        }

        IDependencyResolver ITestHost.Resolver { get; set; }

        void IDisposable.Dispose()
        {
            if (!_disposed)
            {
                _testServer.Dispose();
                _disposed = true;
            }
        }

        Func<IClientTransport> ITestHost.TransportFactory { get; set; }
    }
}
