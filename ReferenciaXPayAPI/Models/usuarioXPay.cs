using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using static ReferenciaXPayAPI.Utilidades;
using Microsoft.Win32;

namespace ReferenciaXPayAPI.Models
{
    public class usuarioXPay
    {
        public int InsertUsuario(string userId, string nombre, string apellido, string email, string celular, string passwordHash, ref string respcode, ref string desccode)
        {
            SqlConnection conn = new SqlConnection();
            SqlCommand SQLComando = new SqlCommand();

            try
            {
                string APPNAME = "ReferenciaXPayAPI";
                string APPKEY = @"Software\ANTAD";
                string llave = APPKEY + "\\" + APPNAME;
                string stringConn;

                // Abrir la clave del registro en HKLM
                using (RegistryKey regVersion = Registry.LocalMachine.OpenSubKey(llave, false))
                {
                    if (regVersion != null)
                    {
                        stringConn = regVersion.GetValue("BD_Def", "").ToString();
                        GrabaLog("Valor ConnectionString: " + stringConn, "***");
                    }
                    else
                    {
                        stringConn = String.Empty;
                        GrabaLog("La clave no existe en el registro.", "***");
                    }
                }

                GrabaLog(stringConn, "ConnectionString: ");

                conn.ConnectionString = stringConn;
                SQLComando.Connection = conn;
                SQLComando.CommandType = CommandType.StoredProcedure;
                SQLComando.CommandText = "UsuarioXPay_Insert";
                SQLComando.Parameters.AddWithValue("@UserId", userId);
                SQLComando.Parameters.AddWithValue("@Nombre", nombre);
                SQLComando.Parameters.AddWithValue("@Apellido", apellido);
                SQLComando.Parameters.AddWithValue("@Email", email);
                SQLComando.Parameters.AddWithValue("@Celular", celular);
                SQLComando.Parameters.AddWithValue("@PasswordHash", passwordHash);                
                
                SQLComando.Connection.Open();

                using (SqlDataReader sqlReader = SQLComando.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            respcode = sqlReader["RESPCODE"].ToString();
                            desccode = sqlReader["DESCCODE"].ToString();

                            GrabaLog($"RespCode: {respcode}, Desccode: {desccode}", "UsuarioXPay_Insert");
                        }
                    }
                    else
                    {
                        GrabaLog("El SP no devolvió registros.", "UsuarioXPay_Insert");
                        return 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                GrabaLog("Ocurrio un error: " + ex.Message, "UsuarioXPay_Insert");
                return 1;
            }
            finally
            {
                conn.Close();
            }

        }

        public int UpdateUsuario(string userId, string nombre, string apellido, string email, string celular, ref string respcode, ref string desccode)
        {
            SqlConnection conn = new SqlConnection();
            SqlCommand SQLComando = new SqlCommand();

            try
            {
                string APPNAME = "ReferenciaXPayAPI";
                string APPKEY = @"Software\ANTAD";
                string llave = APPKEY + "\\" + APPNAME;
                string stringConn;

                // Abrir la clave del registro en HKLM
                using (RegistryKey regVersion = Registry.LocalMachine.OpenSubKey(llave, false))
                {
                    if (regVersion != null)
                    {
                        stringConn = regVersion.GetValue("BD_Def", "").ToString();
                        GrabaLog("Valor ConnectionString: " + stringConn, "***");
                    }
                    else
                    {
                        stringConn = String.Empty;
                        GrabaLog("La clave no existe en el registro.", "***");
                    }
                }

                GrabaLog(stringConn, "ConnectionString: ");

                conn.ConnectionString = stringConn;
                SQLComando.Connection = conn;
                SQLComando.CommandType = CommandType.StoredProcedure;
                SQLComando.CommandText = "UsuarioXPay_Edit";                
                SQLComando.Parameters.AddWithValue("@Nombre", nombre);
                SQLComando.Parameters.AddWithValue("@Apellido", apellido);
                SQLComando.Parameters.AddWithValue("@Email", email);
                SQLComando.Parameters.AddWithValue("@Celular", celular);
                SQLComando.Parameters.AddWithValue("@UserId", userId);                

                SQLComando.Connection.Open();

                using (SqlDataReader sqlReader = SQLComando.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            respcode = sqlReader["RESPCODE"].ToString();
                            desccode = sqlReader["DESCCODE"].ToString();

                            GrabaLog($"RespCode: {respcode}, Desccode: {desccode}", "UsuarioXPay_Edit");
                        }
                    }
                    else
                    {
                        GrabaLog("El SP no devolvió registros.", "UsuarioXPay_Edit");
                        return 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                GrabaLog("Ocurrio un error: " + ex.Message, "UsuarioXPay_Edit");
                return 1;
            }
            finally
            {
                conn.Close();
            }

        }

        public int DeleteUsuario(string userId, string nombre, string apellido, string email, string celular, ref string respcode, ref string desccode)
        {
            SqlConnection conn = new SqlConnection();
            SqlCommand SQLComando = new SqlCommand();

            try
            {
                string APPNAME = "ReferenciaXPayAPI";
                string APPKEY = @"Software\ANTAD";
                string llave = APPKEY + "\\" + APPNAME;
                string stringConn;

                // Abrir la clave del registro en HKLM
                using (RegistryKey regVersion = Registry.LocalMachine.OpenSubKey(llave, false))
                {
                    if (regVersion != null)
                    {
                        stringConn = regVersion.GetValue("BD_Def", "").ToString();
                        GrabaLog("Valor ConnectionString: " + stringConn, "***");
                    }
                    else
                    {
                        stringConn = String.Empty;
                        GrabaLog("La clave no existe en el registro.", "***");
                    }
                }

                GrabaLog(stringConn, "ConnectionString: ");

                conn.ConnectionString = stringConn;
                SQLComando.Connection = conn;
                SQLComando.CommandType = CommandType.StoredProcedure;
                SQLComando.CommandText = "UsuarioXPay_Delete";                
                SQLComando.Parameters.AddWithValue("@UserId", userId);

                SQLComando.Connection.Open();

                using (SqlDataReader sqlReader = SQLComando.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            respcode = sqlReader["RESPCODE"].ToString();
                            desccode = sqlReader["DESCCODE"].ToString();

                            GrabaLog($"RespCode: {respcode}, Desccode: {desccode}", "UsuarioXPay_Delete");
                        }
                    }
                    else
                    {
                        GrabaLog("El SP no devolvió registros.", "UsuarioXPay_Delete");
                        return 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                GrabaLog("Ocurrio un error: " + ex.Message, "UsuarioXPay_Delete");
                return 1;
            }
            finally
            {
                conn.Close();
            }

        }
    }


}

