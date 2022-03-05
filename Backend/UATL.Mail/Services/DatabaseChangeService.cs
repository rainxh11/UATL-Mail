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
        private IDisposable? _statsSubscriptions;
        private IDisposable? _mailSubscriptions;

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
            _statsSubscriptions?.Dispose();
            _mailSubscriptions?.Dispose();

            return Task.CompletedTask;
        }

        public void StartObservables(CancellationToken ct = default)
        {
            var mailChanges = Observable
                .FromEvent<IEnumerable<ChangeStreamDocument<MailModel>>>(x => _mailWatcher.OnChangesCSD += x,
                    x => _mailWatcher.OnChangesCSD -= x)
                .SelectMany(x => x);

            var draftChanges = Observable
                .FromEvent<IEnumerable<ChangeStreamDocument<Draft>>>(x => _draftWatcher.OnChangesCSD += x,
                    x => _draftWatcher.OnChangesCSD -= x)
                .SelectMany(x => x);

            var accountChanges = Observable
                .FromEvent<IEnumerable<ChangeStreamDocument<Account>>>(x => _accountWatcher.OnChangesCSD += x,
                    x => _accountWatcher.OnChangesCSD -= x)
                .SelectMany(x => x);

            _sentMailSubscription = mailChanges
                .Where(x => x.OperationType == ChangeStreamOperationType.Insert)
                .Where(x => x.FullDocument.From != null)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.From.ID, "sent_mail")))
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.From.ID, "refresh_stats")))
                .GroupBy(x => x.FullDocument.From.ID)
                .Subscribe(x => _logger.LogInformation("User: {0}, Sent {1} Mail.", x.Key, x.Count()));

            _receivedMailSubscription = mailChanges
                .Where(x => x.OperationType == ChangeStreamOperationType.Insert)
                .Where(x => x.FullDocument.To != null)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.To.ID, "received_mail")))
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.To.ID, "refresh_stats")))
                .GroupBy(x => x.FullDocument.To.ID)
                .Subscribe(x => _logger.LogInformation("User: {0}, Received {1} Mail.", x.Key, x.Count()));

            _draftSubscription = draftChanges
                .Where(x => x.FullDocument != null)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.From.ID, "refresh_stats")))
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.From.ID, "refresh_draft")))
                .Subscribe();

            _accountSubscription = accountChanges
                .Where(x => x.FullDocument != null)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.Send(x.FullDocument.ID, "refresh_account")))
                .Subscribe();


            _statsSubscriptions = mailChanges
                .CombineLatest(draftChanges, accountChanges)
                .Where(x => x.First.OperationType == ChangeStreamOperationType.Delete ||
                            x.First.OperationType == ChangeStreamOperationType.Drop)
                .Where(x => x.Second.OperationType == ChangeStreamOperationType.Delete ||
                            x.Second.OperationType == ChangeStreamOperationType.Drop)
                .Where(x => x.Third.OperationType == ChangeStreamOperationType.Delete ||
                            x.Third.OperationType == ChangeStreamOperationType.Drop)
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.SendAll("refresh_stats")))
                .Subscribe();

            /*_mailSubscriptions = mailChanges
                .Do(x => _backgroundJobs.Enqueue(() => _notificationSerivce.SendAll("refresh_mail", ct)))
                .Subscribe(x => _logger.LogInformation("Mail Collection Changed: {0}", x.OperationType));*/

        }
    }
}
