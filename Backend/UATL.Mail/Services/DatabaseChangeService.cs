using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Threading;
using System.Reactive.Disposables;
using MongoDB.Entities;
using UATL.MailSystem.Models;
using UATL.MailSystem.Models.Models;
using Microsoft.AspNetCore.SignalR;
using UATL.Mail.Hubs;
using MongoDB.Driver;
using UATL.Mail.Services;
using Hangfire;

namespace UATL.MailSystem.Services
{
    public class DatabaseChangeService : IHostedService
    {
        private Watcher<Account> _accountWatcher;
        private Watcher<Draft> _draftWatcher;
        private Watcher<MailModel> _mailWatcher;

        private readonly ILogger<DatabaseChangeService> _logger;
        private NotificationService _notificationSerivce;
        private IBackgroundJobClient _backgroundJobs;

        public DatabaseChangeService(
            IBackgroundJobClient bgJobs,
            NotificationService nservice,
            ILogger<DatabaseChangeService> logger)
        {
            _mailWatcher = DB.Watcher<MailModel>("mail");
            _draftWatcher = DB.Watcher<Draft>("draft");
            _accountWatcher = DB.Watcher<Account>("account");
            _logger = logger;
            _notificationSerivce = nservice;
            _backgroundJobs = bgJobs;
        }

        private IDisposable? _accountSubscription;
        private IDisposable? _draftSubscription;
        private IDisposable? _sentMailSubscription;
        private IDisposable? _receivedMailSubscription;
        private IDisposable? _changeMailSubscription;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Yield();
            var eventType = EventType.Created | EventType.Deleted | EventType.Updated;

            _accountWatcher.Start(eventType, cancellation: cancellationToken);
            _draftWatcher.Start(eventType, cancellation: cancellationToken);
            _mailWatcher.Start(eventType, cancellation: cancellationToken);

            StartObservables(cancellationToken);
        }


        public Task StopAsync(CancellationToken cancellationToken)
        {
            _draftSubscription?.Dispose();
            _accountSubscription?.Dispose();
            _sentMailSubscription?.Dispose();
            _receivedMailSubscription?.Dispose();
            _changeMailSubscription?.Dispose();

            return Task.CompletedTask;
        }

        public void StartObservables(CancellationToken ct = default)
        {
            var mailChanges = Observable
               .FromEvent<IEnumerable<ChangeStreamDocument<MailModel>>>(x => _mailWatcher.OnChangesCSD += x, x => _mailWatcher.OnChangesCSD -= x)
               .SelectMany(x => x);

            var draftChanges = Observable
                .FromEvent<IEnumerable<ChangeStreamDocument<Draft>>>(x => _draftWatcher.OnChangesCSD += x, x => _draftWatcher.OnChangesCSD -= x)
                .SelectMany(x => x);

            _sentMailSubscription = mailChanges
                .Where(x => x.OperationType == ChangeStreamOperationType.Insert)
                .Where(x => x.FullDocument.From != null)
                .GroupBy(x => x.FullDocument.From.ID)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.Key, "sent_mail", ct)))
                .Subscribe(x => _logger.LogInformation("User: {0}, Sent {1} Mail.", x.Key, x.Count()));

            _receivedMailSubscription = mailChanges
                .Where(x => x.OperationType == ChangeStreamOperationType.Insert)
                .Where(x => x.FullDocument.To != null)
                .GroupBy(x => x.FullDocument.To.ID)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.Key, "received_mail", ct)))
                .Subscribe(x => _logger.LogInformation("User: {0}, Received {1} Mail.", x.Key, x.Count()));

            _draftSubscription = draftChanges
                .Where(x => x.FullDocument != null)
                .GroupBy(x => x.FullDocument.From.ID)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.Key, "refresh_draft", ct)))
                .Subscribe();
        }
    }
}
