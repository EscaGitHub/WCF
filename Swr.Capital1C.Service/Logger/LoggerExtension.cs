using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NLog;
using NLog.Targets;

namespace Swr.Capital1C.Service.Logger
{
	public static class LoggerExtension
	{
		public static void ErrorWithContext(this NLog.Logger logger, Exception exception, object data, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Error, logger.Name, CultureInfo.CurrentCulture, message, args, exception);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			if (data != null)
				SetEventContext(SerializeToXml(data), eventInfo);

			logger.Log(eventInfo);
		}

		public static void ErrorWithContext(this NLog.Logger logger, object data, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Error, logger.Name, CultureInfo.CurrentCulture, message, args);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			if (data != null)
				SetEventContext(SerializeToXml(data), eventInfo);

			logger.Log(eventInfo);
		}

        public static void TraceWithContext(this NLog.Logger logger, object data, string message, params object[] args)
        {
            var eventInfo = new LogEventInfo(LogLevel.Trace, logger.Name, CultureInfo.CurrentCulture, message, args);

            if (data != null)
                SetEventContext(SerializeToXml(data), eventInfo);

            logger.Log(eventInfo);
        }

        public static void ErrorWithContext(this NLog.Logger logger, Exception exception, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Error, logger.Name, CultureInfo.CurrentCulture, message, args, exception);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			logger.Log(eventInfo);   
		}

		public static void ErrorWithContext(this NLog.Logger logger, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Error, logger.Name, CultureInfo.CurrentCulture, message, args);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			logger.Log(eventInfo);
		}

		public static void InfoWithContext(this NLog.Logger logger, object data, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Info, logger.Name, CultureInfo.CurrentCulture, message, args);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			if (data != null)
				SetEventContext(SerializeToXml(data), eventInfo);

			logger.Log(eventInfo);
		}

		public static void InfoWithContext(this NLog.Logger logger, IDocumentContext documentContext, string message, params object[] args)
		{
			var eventInfo = new LogEventInfo(LogLevel.Info, logger.Name, CultureInfo.CurrentCulture, message, args);

			if (documentContext != null)
				SetEventContext(documentContext, eventInfo);

			logger.Log(eventInfo);
		}

		private static void SetEventContext(IDocumentContext documentContext, LogEventInfo eventInfo)
		{
			eventInfo.Properties["FileId"] = documentContext.FileId;
			eventInfo.Properties["FilePath"] = documentContext.FolderPath + documentContext.Name;
			eventInfo.Properties["FolderId"] = documentContext.FolderId;
			eventInfo.Properties["RevisionNumber"] = documentContext.Redaction;
			eventInfo.Properties["Configuration"] = documentContext.Configuration;
		}

		private static void SetEventContext(string xmlData, LogEventInfo eventInfo)
		{
			eventInfo.Properties["XmlData"] = xmlData;
		}

		private static string SerializeToXml(object obj)
		{
			if (obj == null)
			{
				return null;
			}

			var serializer = new XmlSerializer(obj.GetType());

			var settings = new XmlWriterSettings { Encoding = new UnicodeEncoding(false, false), Indent = true, OmitXmlDeclaration = true };

			using (var textWriter = new StringWriter())
			{
				using (var xmlWriter = XmlWriter.Create(textWriter, settings))
				{
					serializer.Serialize(xmlWriter, obj);
				}

				return textWriter.ToString();
			}
		}
	}
}
