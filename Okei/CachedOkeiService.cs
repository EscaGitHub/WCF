using System.Collections.Generic;
using System.Threading.Tasks;

namespace Swr.Capital1C.Okei
{
    public class CachedOkeiService : IOkeiService
    {
        private readonly IOkeiService _okeiService;
        private readonly Dictionary<string, string> _cachedCodes;

        public CachedOkeiService(IOkeiService okeiService)
        {
            _okeiService = okeiService;
            _cachedCodes = new Dictionary<string, string>();
        }

        public async Task<string> GetOkeiCodeAsync(string unitOfMeasure)
        {
            if (_cachedCodes.ContainsKey(unitOfMeasure))
                return _cachedCodes[unitOfMeasure];

            var code = await _okeiService.GetOkeiCodeAsync(unitOfMeasure);

            _cachedCodes.Add(unitOfMeasure, code);

            return code;
        }
    }
}
