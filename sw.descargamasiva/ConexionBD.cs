using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sw.descargamasiva
{
    class ConexionBD
    {
        private string connectionString = "SERVER=192.168.1.200;Initial Catalog=LiftorProd;" +
            "Integrated Security=false;User=sa;Password=8nalgwpJ$";

        public void abrir()
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectionString);
                connection.Open();

            }
            catch (Exception ex)
            {
                var mensaje = "Error message: " + ex.Message;

            }
        }

        public void cerrar()
        {
            SqlConnection connection = new SqlConnection(connectionString);
            connection.Close();
        }

        //Ejecuta sp sin retorno de informacion
        public void ExecuteSP1(string procedimiento, params SqlParameter[] datos)
        {
            try
            {
                // Crea la conexión con SQL Server
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmnd = new SqlCommand(procedimiento, con))
                    {
                        // Especifica que será un procedimiento almacenado.
                        cmnd.CommandType = CommandType.StoredProcedure;
                        // Agrega los parámetros necesarios
                        foreach (SqlParameter p in datos)
                        {
                            cmnd.Parameters.Add(p);
                        }
                        con.Open();
                        // AQUÍ se ejecuta el procedimiento
                        using (SqlDataAdapter da = new SqlDataAdapter(cmnd))
                        {
                            DataSet ds = new DataSet();
                            con.Close();
                            da.Fill(ds);
                            //return ds;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine($"Security error.\n\nError message: {e.Message}\n\n" +
                   $"Details:\n\n{e.StackTrace}");
                throw;
            }
        }

        //Ejecucion de stores procedure con retorno de datos 
        public DataSet ExecuteSP(string procedimiento, params SqlParameter[] datos)
        {
            try
            {
                // Crea la conexión con SQL Server
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmnd = new SqlCommand(procedimiento, con))
                    {
                        // Especifica que será un procedimiento almacenado.
                        cmnd.CommandType = CommandType.StoredProcedure;
                        // Agrega los parámetros necesarios
                        foreach (SqlParameter p in datos)
                        {
                            cmnd.Parameters.Add(p);
                        }
                        con.Open();
                        // AQUÍ se ejecuta el procedimiento
                        using (SqlDataAdapter da = new SqlDataAdapter(cmnd))
                        {
                            DataSet ds = new DataSet();
                            con.Close();
                            da.Fill(ds);
                            return ds;
                        }
                    }
                }
            }
            catch (SqlException e)
            {
                Console.WriteLine($"Security error.\n\nError message: {e.Message}\n\n" +
                   $"Details:\n\n{e.StackTrace}");
                throw;
            }
        }

        public void InsertaTabla(string doc, string uuid)
        {
            string query = "Insert into XmlDoc (documento) values" +
                "(@doc)";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@doc", doc);//Se agregan los parametros con los valores del forms                
                //command.Parameters.AddWithValue("@uuId", uuid);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();//Cierre de la conexion en la bd

                }
                catch (Exception ex)
                {
                    throw new Exception("Hay un error en la bd " + ex.Message);
                }
            }
        }
    }
}
