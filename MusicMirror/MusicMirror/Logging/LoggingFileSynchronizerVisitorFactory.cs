using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace MusicMirror.Logging
{
	public sealed class LoggingFileSynchronizerVisitorFactory : IFileSynchronizerVisitorFactory
	{
		private readonly IFileSynchronizerVisitorFactory _innerFactory;
		private readonly ILogger _log;

		public LoggingFileSynchronizerVisitorFactory(IFileSynchronizerVisitorFactory innerFactory, ILogger log)
		{
			_innerFactory = Guard.ForNull(innerFactory, nameof(innerFactory));
			_log = Guard.ForNull(log, nameof(log));
		}

		public IFileSynchronizerVisitor CreateVisitor(MusicMirrorConfiguration configuration)
		{
			return new LoggingFileSynchronizerVisitor(_innerFactory.CreateVisitor(configuration), _log);
		}
	}

	public sealed class LoggingFileSynchronizerVisitor : IFileSynchronizerVisitor
	{
		private readonly IFileSynchronizerVisitor _innerVisitor;
		private readonly ILogger _log;

		public LoggingFileSynchronizerVisitor(IFileSynchronizerVisitor innerVisitor, ILogger log)
		{
			_innerVisitor = Guard.ForNull(innerVisitor, nameof(innerVisitor));
			_log = Guard.ForNull(log, nameof(log));
		}

		public async Task Visit(CancellationToken ct, FileModifiedNotification notification)
		{
			await Log(
				_innerVisitor.Visit(ct, notification),
				"Visiting FileModifiedNotification. File is " + notification.FileInfo.FullName,
				notification);
		}

		public async Task Visit(CancellationToken ct, FileInitNotification notification)
		{
			await Log(
				_innerVisitor.Visit(ct, notification),
				"Visiting FileInitNotification. File is " + notification.FileInfo.FullName,
				notification);
		}

		public async Task Visit(CancellationToken ct, FileRenamedNotification notification)
		{
			await Log(
				_innerVisitor.Visit(ct, notification),
				"Visiting FileRenamedNotification. File is : " + notification.FileInfo.FullName + ", old path is " + notification.OldFullPath,
				notification);
		}

		public async Task Visit(CancellationToken ct, FileDeletedNotification notification)
		{
			await Log(
				_innerVisitor.Visit(ct, notification),
				"Visiting FileDeletedNotification. File is : " + notification.FileInfo.FullName,
				notification);
		}

		public async Task Visit(CancellationToken ct, FileAddedNotification notification)
		{
			await Log(
				_innerVisitor.Visit(ct, notification),
				"Visiting FileAddedNotification. File is : " + notification.FileInfo.FullName,
				notification);
		}

		private async Task Log(Task task, string message, IFileNotification notification)
		{
			_log.Info(message);
			try
			{
				await task;
			}
			catch (Exception ex)
			{
				_log.Error(ex, "Error while visiting " + notification.Kind);
				throw;
			}
		}
	}
}
