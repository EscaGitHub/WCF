using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using NLog.Fluent;
using Swr.Capital1C.Okei;
using Swr.Capital1C.Service.Logger;
using Swr.Capital1C.Service.Settings.Model;
using Swr.Infrastructure.Mailing;

namespace Swr.Capital1C.Service.Settings
{
    public static class CommonSettingsController
    {
        public static CommonSettings Settings => _settings ?? LoadCommonSettings();

        private static readonly NLog.Logger Logger = LogHelper.Instance.GetLogger("Менеджер настроек");
        private static CommonSettings _settings;

        private const string RelativePath = @"\SWR\SWR-Capital1C\";
        private const string SettingsFileName = "ServiceSettings.xml";

        public static CommonSettings GetDefault()
        {
            var settings = new CommonSettings
            {
                PdmDbConnection = {Server = "Server", DataBase = "PDM", UserName = "login", Password = "password"},
                ExportedDocumentsStatusDbConnection = { Server = "(local)", DataBase = "Capital", UserName = "login", Password = "password" },
                IsServiceVariableName = "спр_Служебная_запись",
                ArticleVariableName = "_Артикул_для_1С",
                OkeiServiceConnection = new OkeiServiceConnection { Address = @"http://localhost:5002/api/v2/okei/national-legends/" },
                EmailServiceSettings = new EmailServiceSettings
                {
                    Sender = "email@email.ru",
                    UserName = "email@email.ru",
                    Password = "password",
                    ServerAddress = "smtp.email.ru",
                    Port = 465,
                    EnableSsl = true,
                    EnableAuthentication = true,
                    To = new List<string> { "emailTo@email.ru"}
                },
                NomenclatureDefinition =
                {
                    StatusVariableName = "_Статус_передачи_номенклатуры",
                    StatusVariableValue = "Готова к передаче в 1С:УПП",
                    FolderDefinitions = new List<FolderDefinition>
                    {
                        new FolderDefinition {Extensions = new List<string> {".sldprt", ".sldasm", ".swrmat.cvd"}, FolderPaths = new List<string> {@"\Проекты\"}},
                        new FolderDefinition {Extensions = new List<string> {".sldprt", ".sldasm", ".swrmat.cvd"}, FolderPaths = new List<string> {@"\Справочники\"}},
                    },
                    VariableMaps = new List<VariableMap>
                    {
                        new VariableMap {AttributeName = "Артикул", MessageAttributeName = "ID"},
                        new VariableMap {AttributeName = "Наименование номенклатуры", MessageAttributeName = "NAME"},
                        new VariableMap {AttributeName = "Единица измерения", MessageAttributeName = "UOM"},
                        new VariableMap {AttributeName = "Раздел спецификации", MessageAttributeName = "BOM_PART"},
                        new VariableMap {AttributeName = "Вид номенклатуры", MessageAttributeName = "TYPE"},
                        new VariableMap {AttributeName = "Признак готовой продукции", MessageAttributeName = "IS_PRODUCT"},
                        new VariableMap {AttributeName = "Вид воспроизводства", MessageAttributeName = "PURCHASED"},
                        new VariableMap {AttributeName = "Материал", MessageAttributeName = "MATERIAL"},
                        new VariableMap {AttributeName = "Плотность", MessageAttributeName = "DENSITY"},
                        new VariableMap {AttributeName = "Масса", MessageAttributeName = "WEIGHT"},
                        new VariableMap {AttributeName = "Площадь поверхности", MessageAttributeName = "AREA"},
                        new VariableMap {AttributeName = "Количество гибов", MessageAttributeName = "BEND"},
                        new VariableMap {AttributeName = "Общая длина", MessageAttributeName = "LENGTH"},
                        new VariableMap {AttributeName = "Общая ширина", MessageAttributeName = "WIDTH"},
                        new VariableMap {AttributeName = "Общая толщина", MessageAttributeName = "THICK"},
                        new VariableMap {AttributeName = "Длина реза", MessageAttributeName = "LENGTH_CUT"},
                    }
                },
                BomDefinition =
                {
                    StatusVariableName = "_Статус_передачи_спецификации",
                    StatusVariableValue = "Готова к передаче в 1С:УПП",
                    FolderDefinitions = new List<FolderDefinition>
                    {
                        new FolderDefinition {Extensions = new List<string> {".sldprt", ".sldasm", ".swrmat.cvd"}, FolderPaths = new List<string> {@"\Проекты\"}},
                        new FolderDefinition {Extensions = new List<string> {".sldprt", ".sldasm", ".swrmat.cvd"}, FolderPaths = new List<string> {@"\Справочники\"}},
                    },
                    VariableMaps = new List<VariableMap>
                    {
                        new VariableMap {AttributeName = "Артикул родителя", MessageAttributeName = "PARENT_ID"},
                        new VariableMap {AttributeName = "Версия", MessageAttributeName = "VERSION"},
                        new VariableMap {AttributeName = "Наименование", MessageAttributeName = "BOM_NAME"},
                        new VariableMap {AttributeName = "Кол-во выходного изделия", MessageAttributeName = "QTY_BOM"},
                        new VariableMap {AttributeName = "Артикул", MessageAttributeName = "CHILD_ID"},
                        new VariableMap {AttributeName = "Количество", MessageAttributeName = "QTY"},
                        new VariableMap {AttributeName = "Единица измерения количества", MessageAttributeName = "UOM"},
                    }
                },
                NomenclatureCatalogServiceConnection = new CatalogServiceConnection
                {
                    Address = "http://localhost:5004/api/v2/nomenclatures/",
                    IdentityAddress = "http://localhost:5004/api/account",
                    Login = "clientuser",
                    Password = "12345"
                },
                BomCatalogServiceConnection = new CatalogServiceConnection
                {
                    Address = "http://localhost:5000/api/v2/engineering-boms/",
                    IdentityAddress = "http://localhost:5000/api/account",
                    Login = "clientuser",
                    Password = "12345"
                }
            };

            return settings;
        }

        private static CommonSettings LoadCommonSettings()
        {
            Logger.Debug("Загрузка настроек...");

            var serviceSettings = LoadServiceSettings();

            if (serviceSettings == null)
            {
                Logger.Warn("Не удалось прочитать настройки службы. Будут загружены настройки по умолчанию.");

                serviceSettings = GetDefault();

                SaveServiceSettings(serviceSettings);

                Logger.Debug("Настройки по умолчанию загружены и сохранены в файл.");
            }

            Logger.Debug("Настройки загружены.");

            _settings = serviceSettings;

            return _settings;
        }

        private static CommonSettings LoadServiceSettings()
        {
            var folderPath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + RelativePath;
            var filePath = Path.Combine(folderPath, SettingsFileName);

            if (!File.Exists(filePath))
            {
                Logger.Debug($"Файл настроек не найден: {filePath}");

                return null;
            }

            var settings = filePath.DeserializeFromFile<CommonSettings>();

            return settings;
        }

        private static void SaveServiceSettings(CommonSettings settings, string path = null)
        {
            if (settings == null) throw new NullReferenceException("Настройки null.");

            if (path == null) path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + RelativePath;

            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            settings.SerializeToFile(Path.Combine(path, SettingsFileName));
        }

        private static T DeserializeFromFile<T>(this string file) where T : new()
        {
            if (!File.Exists(file)) throw new InvalidOperationException($"Файл '{file}' не найден.");

            using (var reader = new StreamReader(file))
            {
                var serializer = new XmlSerializer(typeof(T));
                try
                {
                    return (T)serializer.Deserialize(reader);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(String.Format("Невозможно считать данные. Файл \"{0}\". Ошибка '{1}'", file, ex.Message));
                }
            }
        }

        private static void SerializeToFile<T>(this T settings, string file)
        {
            if (file == null) throw new NullReferenceException("Имя файла null.");
            if (settings == null) throw new NullReferenceException("Объект null.");

            string folderPath = Path.GetDirectoryName(file);

            if (!String.IsNullOrWhiteSpace(folderPath) && !Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            using (var writer = new StreamWriter(file))
            {
                var serializer = new XmlSerializer(settings.GetType());

                serializer.Serialize(writer, settings);
            }
        }

        public static void Dispose()
        {
            _settings = null;
        }
    }
}
