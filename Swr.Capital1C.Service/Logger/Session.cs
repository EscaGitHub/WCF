using System;
using System.Data.SqlClient;
using System.Globalization;

namespace Swr.Capital1C.Service.Logger
{
    public class Session : IDisposable
    {
        public Session(string number, string machineName, string userName)
        {
            Number = number;
            MachineName = machineName;
            UserName = userName;

            NLog.MappedDiagnosticsContext.Set("SessionNumber", number);
            NLog.MappedDiagnosticsContext.Set("MachineName", string.IsNullOrEmpty(machineName) ? string.Empty : machineName);
            NLog.MappedDiagnosticsContext.Set("UserName", string.IsNullOrEmpty(userName) ? string.Empty : userName);
        }

        public string Number { get; private set; }

        public string MachineName { get; private set; }

        public string UserName { get; private set; }

        public void Dispose()
        {
            NLog.MappedDiagnosticsContext.Remove("SessionNumber");
            NLog.MappedDiagnosticsContext.Remove("MachineName");
            NLog.MappedDiagnosticsContext.Remove("UserName");
        }
    }
}
