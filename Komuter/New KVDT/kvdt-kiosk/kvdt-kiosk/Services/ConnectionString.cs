namespace kvdt_kiosk.Services
{
    public class ConnectionString
    {
        public static string GetConnectionString()
        {
            string path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace("file:\\", "");
            path = path.Replace("\\bin\\Debug", "");
            path = path.Replace("\\bin\\Release", "");
            path = path.Replace("\\kvdt-kiosk", "");
            path = path + "\\Database\\db_tvm_kiosk.db";

            return "Data Source=" + path;
        }
    }
}
