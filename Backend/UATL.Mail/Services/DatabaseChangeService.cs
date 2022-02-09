using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Threading;
using System.Reactive.Disposables;

namespace UATL.Mail.Services
{
    public class DatabaseChangeService : IHostedService
    {
        private ILogger<DatabaseChangeService> _logger;
        public DatabaseChangeService(ILogger<DatabaseChangeService> logger)
        {
            _logger = logger;
        }

        private IDisposable? _accountSubscription;
        private IDisposable? _orderSubscription;
        private IDisposable? _customerSubscription;
        private IDisposable? _productSubscription;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
