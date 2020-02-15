using System.Diagnostics;

namespace Swr.Capital1C.WindowsService
{
	internal class ServiceLogger
	{
		private static ServiceLogger _instance;

		public static ServiceLogger Instance
		{
			get { return _instance ?? (_instance = new ServiceLogger()); }
		}

		private string _serviceName;
		public string ServiceName
		{
			get { return _serviceName; }
			set
			{
				if (_serviceName == value)
					return;

				_serviceName = value;

				var exist = EventLog.SourceExists(ServiceName);

				if (!exist)
					EventLog.CreateEventSource(ServiceName, ServiceName);
			}
		}

		public void LogInformation(string log)
		{
			AddLog(log, EventLogEntryType.Information);
		}

		public void LogError(string log)
		{
			AddLog(log, EventLogEntryType.Error);
		}

		private void AddLog(string log, EventLogEntryType eventType)
		{
			try
			{
				if (string.IsNullOrWhiteSpace(ServiceName))
					return;

				var eventLog = new EventLog { Source = ServiceName };
				eventLog.WriteEntry(log, eventType);
			}
			catch
			{
			}
		}
	}
}
