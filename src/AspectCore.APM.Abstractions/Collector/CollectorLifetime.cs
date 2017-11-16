﻿using System;
using System.Threading;
using AspectCore.APM.Common;

namespace AspectCore.APM.Collector
{
    internal class CollectorLifetime : ICollector, ICollectorLifetime
    {
        private readonly IInternalLogger _logger;
        private readonly IPayloadDispatcher _payloadDispatcher;
        private int _status = 1; // 0. started 1. stopped

        public CollectorLifetime(IPayloadDispatcher payloadDispatcher, IInternalLogger logger = null)
        {
            _payloadDispatcher = payloadDispatcher ?? throw new ArgumentNullException(nameof(payloadDispatcher));
            _logger = logger;
        }

        public bool Push(IPayload payload)
        {
            if (!_payloadDispatcher.Dispatch(payload))
            {
                _logger?.LogWarning("Couldn't dispatch payload, actor may be blocked by another operation.");
                return false;
            }
            return true;
        }

        public bool Start()
        {
            if (Interlocked.CompareExchange(ref _status, 0, 1) == 1)
            {
                _payloadDispatcher.Start();
                _logger?.LogInformation($"AspectCore APM collector started.Use {_payloadDispatcher.Name}.");
                return true;
            }

            return false;
        }

        public void Stop()
        {
            if (Interlocked.CompareExchange(ref _status, 1, 0) == 0)
            {
                _payloadDispatcher.Stop();
                _logger?.LogInformation("AspectCore APM collector stopped.");
            }
        }
    }
}