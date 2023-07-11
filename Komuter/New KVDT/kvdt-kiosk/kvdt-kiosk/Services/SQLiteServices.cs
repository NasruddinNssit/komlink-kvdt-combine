using kvdt_kiosk.Interfaces;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows;

namespace kvdt_kiosk.Services
{
    public class SQLiteServices : ISQLiteServices
    {
        public ConnectionString ConnectionString { get; set; }

        public Task<int> Delete<T>(T model)
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM " + typeof(T).Name + " WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", model.GetType().GetProperty("Id").GetValue(model));

                return Task.Run(() => cmd.ExecuteNonQuery());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }

            return Task.Run(() => 0);
        }

        public Task<int> DeleteAll<T>() where T : new()
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("DELETE FROM " + typeof(T).Name, conn);

                return Task.Run(() => cmd.ExecuteNonQuery());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                conn.Close();
            }
            return Task.Run(() => 0);

        }

        public Task<T> Get<T>(int id) where T : new()
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM " + typeof(T).Name + " WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);

                var reader = cmd.ExecuteReader();

                return Task.Run(() =>
                {
                    var model = new T();
                    while (reader.Read())
                    {
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            prop.SetValue(model, reader[prop.Name]);
                        }
                    }
                    return model;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }

        public Task<T> Get<T>(string id) where T : new()
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM " + typeof(T).Name + " WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                var reader = cmd.ExecuteReader();

                return Task.Run(() =>
                {
                    var model = new T();
                    while (reader.Read())
                    {
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            prop.SetValue(model, reader[prop.Name]);
                        }
                    }
                    return model;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }

        public Task<List<T>> GetAll<T>() where T : new()
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());
            try
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM " + typeof(T).Name, conn);
                var reader = cmd.ExecuteReader();

                var list = new List<T>();

                return Task.Run(() =>
                {
                    while (reader.Read())
                    {
                        var model = new T();
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            prop.SetValue(model, reader[prop.Name]);
                        }
                        list.Add(model);
                    }
                    return list;
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }

        public Task<int> Insert<T>(T model)
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("INSERT INTO " + typeof(T).Name + " VALUES (", conn);
                foreach (var prop in typeof(T).GetProperties())
                {
                    cmd.CommandText += "'" + prop.GetValue(model) + "',";
                }

                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 1);
                cmd.CommandText += ")";
                return Task.Run(() => cmd.ExecuteNonQuery());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }

        public Task<List<T>> Read<T>() where T : new()
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("SELECT * FROM " + typeof(T).Name, conn);
                var reader = cmd.ExecuteReader();

                var list = new List<T>();

                return Task.Run(() =>
                {
                    while (reader.Read())
                    {
                        var model = new T();
                        foreach (var prop in typeof(T).GetProperties())
                        {
                            prop.SetValue(model, reader[prop.Name]);
                        }
                        list.Add(model);
                    }
                    return list;
                });

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }

        public Task<int> Update<T>(T model)
        {
            var conn = new SqlConnection(ConnectionString.GetConnectionString());

            try
            {
                conn.Open();
                var cmd = new SqlCommand("UPDATE " + typeof(T).Name + " SET ", conn);
                foreach (var prop in typeof(T).GetProperties())
                {
                    cmd.CommandText += prop.Name + " = '" + prop.GetValue(model) + "',";
                }

                cmd.CommandText = cmd.CommandText.Remove(cmd.CommandText.Length - 1);

                cmd.CommandText += " WHERE Id = " + typeof(T).GetProperty("Id").GetValue(model);

                return Task.Run(() => cmd.ExecuteNonQuery());

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            conn.Close();
            return null;
        }
    }
}
