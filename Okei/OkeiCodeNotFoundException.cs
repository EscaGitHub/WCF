using System;

namespace Swr.Capital1C.Okei
{
    public class OkeiCodeNotFoundException : Exception
    {
        public OkeiCodeNotFoundException(string unitOfMeasure) : base($"Не найден код по классификатору ОКЕИ для обозначения '{unitOfMeasure}'")
        {
        }
    }
}