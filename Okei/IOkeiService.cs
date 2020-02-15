using System.Threading.Tasks;

namespace Swr.Capital1C.Okei
{
    public interface IOkeiService
    {
        Task<string> GetOkeiCodeAsync(string unitOfMeasure);
    }
}