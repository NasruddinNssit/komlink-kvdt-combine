namespace kvdt_kiosk.Database
{
    public class ConnectionString
    {
        public static string GetConnectionString()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string dbPath = System.IO.Path.Combine(path, "db-kvdt-kiosk.db");
            return $"Data Source={dbPath};";
        }
    }
}
