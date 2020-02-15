using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using Swr.Capital1C.Service.Domain.Services.Boms.Models;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models;

namespace Swr.Capital1C.Service
{
    [ServiceContract]
    public interface IService
    {
        /// <summary>
        /// Проверка доступности сервера
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        Task<bool> IsAvailableAsync();

        [OperationContract]
        Task<ITEMS_RES> GetNomenclaturesAsync(ITEMS_REQ request);

        [OperationContract]
        Task<ITEM_RESULTS_RES> SetNomenclaturesResponseAsync(ITEM_RESULTS_REQ request);

        [OperationContract]
        Task<BOMS_RES> GetBomsAsync(BOMS_REQ request);

        [OperationContract]
        Task<BOM_RESULTS_RES> SetBomsResponseAsync(BOM_RESULTS_REQ request);
    }
}
