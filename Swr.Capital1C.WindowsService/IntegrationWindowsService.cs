using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using NLog;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings;

namespace Swr.Capital1C.WindowsService
{
    public partial class IntegrationWindowsService : ServiceBase
    {
        private Logger _logger;

        private ServiceHost _integrationService;

        public IntegrationWindowsService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            ServiceLogger.Instance.ServiceName = ServiceName;

            try
            {
                _logger = LogHelper.Instance.GetLogger("Служба Windows. Интеграция PDM с 1С:УППП");
            }
            catch (Exception e)
            {
                ServiceLogger.Instance.LogInformation(e.ToString());
                throw;
            }

            try
            {
                var loadSettingsOnStartToFile = CommonSettingsController.Settings;

                StartService();

                _logger.Info("Служба запущена");
                ServiceLogger.Instance.LogInformation("Служба запущена");
            }
            catch (Exception e)
            {
                _logger.Error(e);
                ServiceLogger.Instance.LogError(e.ToString());
                throw;
            }
        }

        private void StartService()
        {
            _logger.Debug("Запуск сервиса...");

            _integrationService?.Close();

            _integrationService = new ServiceHost(typeof(Service.Service));
            _integrationService.Open();
        }

        protected override void OnStop()
        {
            if (_integrationService != null)
            {
                try
                {
                    _integrationService.Abort();
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    
                }

                _integrationService = null;

            }
        }
    }
}
