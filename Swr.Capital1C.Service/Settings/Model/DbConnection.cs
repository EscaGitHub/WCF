namespace Swr.Capital1C.Service.Settings.Model
{
    public class DbConnection
    {
        public string Server { get; set; }

        public string DataBase { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public int Timeout { get; set; } = 300;

        public string ConnectionString()
        {
            return $"Data Source={Server};Initial Catalog={DataBase};User Id={UserName};Password={Password};Connection Timeout={Timeout}";
        }
    }
}