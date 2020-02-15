using System;
using System.Collections.Generic;
using System.Linq;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Exceptions;
using Swr.Capital1C.Service.Domain.Services.Nomenclatures.Models.In;
using Swr.Capital1C.Service.Infrastructure.Nomenclatures;
using Swr.Capital1C.Service.Properties;
using Swr.Infrastructure.Mailing;

namespace Swr.Capital1C.Service.Infrastructure.Email
{
    public static class EmailMessageFactory
    {
        private const string NomenclatureExportError = "Ошибки экспорта номенклатуры";
        private const string MessageHeader = "Ошибки в процессе экспорта номенклатуры из SWE-PDM в 1C:УПП";

        //public static EmailMessage SetNomenclatureNotFound(string id,
        //    NomenclatureNotFoundException exception)
        //{
        //    var builder = new HtmlEmailMessageBuilder();

        //    builder.SetSubject(NomenclatureExportError);

        //    builder.SetEventLevel(EventLevel.Error);
        //    builder.SetHeader(MessageHeader);
        //    builder.SetDescription($"Номенклатура с идентификатором '{id}' не найдена.");
        //    builder.SetDetails("Подробности", exception.Message);

        //    return builder.GetMessage();
        //}

        //public static EmailMessage SetNomenclatureIsInvalid(string id,
        //    NomenclatureIsInvalidException exception)
        //{
        //    var builder = new HtmlEmailMessageBuilder();

        //    builder.SetSubject(NomenclatureExportError);

        //    builder.SetEventLevel(EventLevel.Error);
        //    builder.SetHeader(MessageHeader);
        //    builder.SetDescription($"Номенклатура с идентификатором '{id}' содержит ошибки.");
        //    builder.SetDetails("Подробности", exception.Message);

        //    return builder.GetMessage();
        //}

        //public static EmailMessage SetNomenclatureIsOutdated(Nomenclature nomenclature,
        //    NomenclatureIsOutdatedException exception, string articleAttributeName)
        //{
        //    var builder = new HtmlEmailMessageBuilder();

        //    builder.SetSubject(NomenclatureExportError);

        //    builder.SetEventLevel(EventLevel.Error);
        //    builder.SetHeader(MessageHeader);

        //    var code = nomenclature.GetValueOrDefault(articleAttributeName);
        //    builder.SetDescription($"Ранее была передана новая версия номенклатуры с кодом '{code}' (идентификатор '{nomenclature.Id}').");
        //    builder.SetDetails("Подробности", exception.Message);

        //    return builder.GetMessage();
        //}

        //public static EmailMessage SetOkeiCodeNotFound(Nomenclature nomenclature, OkeiCodeNotFoundException exception, string articleAttributeName)
        //{
        //    var builder = new HtmlEmailMessageBuilder();

        //    builder.SetSubject(NomenclatureExportError);

        //    builder.SetEventLevel(EventLevel.Error);
        //    builder.SetHeader(MessageHeader);

        //    var code = nomenclature.GetValueOrDefault(articleAttributeName);
        //    builder.SetDescription($"Код ОКЕИ не найден для номенклатуры с кодом '{code}' (идентификатор '{nomenclature.Id}').");
        //    builder.SetDetails("Подробности", exception.Message);

        //    return builder.GetMessage();
        //}

        public static EmailMessage SetUnexpectedError(Exception exception)
        {
            var builder = new HtmlEmailMessageBuilder();

            builder.SetSubject(NomenclatureExportError);

            builder.SetEventLevel(EventLevel.Error);
            builder.SetHeader(MessageHeader);
            builder.SetDescription("Возникла непредвиденная ошибка.");
            builder.SetDetails("Подробности", exception.ToString());

            return builder.GetMessage();
        }

        public static EmailMessage SetErrorMessages(IEnumerable<string> errors, string subject, string header)
        {
            var builder = new HtmlEmailMessageBuilder();

            builder.SetSubject(NomenclatureExportError);

            builder.SetEventLevel(EventLevel.Error);
            builder.SetHeader(MessageHeader);
            builder.SetDescription("Возникли ошибки передачи.");
            builder.SetDetails("Подробности", string.Join(Environment.NewLine, errors));

            return builder.GetMessage();
        }
    }
}