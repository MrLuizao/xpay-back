using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Text;
using System.Globalization;
using ReferenciaXPayAPI_Core.Models;

namespace ReferenciaXPayAPI_Core.Logic
{
    public class ReferenciaLogic
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _logPath;

        public ReferenciaLogic(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetValue<string>("BD_Def") ?? string.Empty;
            _logPath = _configuration.GetValue<string>("LogFiles") ?? string.Empty;
        }

        public void GrabaLog(string text, string flag)
        {
            if (string.IsNullOrEmpty(_logPath)) return;

            try
            {
                DateTime fecha = DateTime.Now;
                string nombreArchivo = $"ReferenciaXPayAPI_{fecha:yyyyMMdd}.txt";
                string nombreArchivoCompleto = Path.Combine(_logPath, nombreArchivo);

                StringBuilder messageLog = new StringBuilder();
                messageLog.Append(DateTime.Now.ToString(CultureInfo.InvariantCulture));
                messageLog.Append("\t");
                messageLog.Append(flag);
                messageLog.Append("\t");
                messageLog.Append(text);

                // Ensure directory exists
                if (!Directory.Exists(_logPath))
                {
                    Directory.CreateDirectory(_logPath);
                }

                File.AppendAllLines(nombreArchivoCompleto, new[] { messageLog.ToString() });
            }
            catch (Exception ex)
            {
                // In a real app we might log to console or another system if file fails
                Console.WriteLine($"Error writing log: {ex.Message}");
            }
        }

        public int GenerarBD(string referencia, out string respcode, out string referenciaNumerica)
        {
            respcode = string.Empty;
            referenciaNumerica = string.Empty;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                using (SqlCommand sqlComando = new SqlCommand("ReferenciaNumericaXPay_Generar", conn))
                {
                    try
                    {
                        sqlComando.CommandType = CommandType.StoredProcedure;
                        sqlComando.Parameters.AddWithValue("@Referencia", referencia);
                        conn.Open();

                        using (SqlDataReader sqlReader = sqlComando.ExecuteReader())
                        {
                            if (sqlReader.HasRows)
                            {
                                while (sqlReader.Read())
                                {
                                    respcode = sqlReader["RESPCODE"]?.ToString() ?? string.Empty;
                                    referenciaNumerica = sqlReader["ReferenciaFinal"]?.ToString() ?? string.Empty;

                                    GrabaLog($"RespCode: {respcode}, RefNum: {referenciaNumerica}", "ReferenciaNumericaXPay_Generar");
                                }
                                return 0;
                            }
                            else
                            {
                                GrabaLog("El SP no devolvió registros.", "ReferenciaNumericaXPay_Generar");
                                return 1;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        GrabaLog("Ocurrio un error: " + ex.Message, "InsertAbonoDB");
                        return 1;
                    }
                }
            }
        }

        public void ObtenerCampos(string referencia, out string regPat, out string perPag, out string origen, out string fsua, out string fechVenc, out string impImss, out string impRcv, out string impApv, out string impAcv)
        {
            regPat = referencia.Substring(0, 1) + Base36aBase10(referencia.Substring(1, 7)).ToString();
            perPag = Base36aBase10(referencia.Substring(8, 4)).ToString();
            origen = Base36aBase10(referencia.Substring(12, 1)).ToString();
            fsua = Base36aBase10(referencia.Substring(13, 4)).ToString();
            fechVenc = GetFechaVenc(referencia.Substring(17, 4));
            impImss = Base36aBase10(referencia.Substring(21, 7)).ToString();
            impRcv = Base36aBase10(referencia.Substring(28, 7)).ToString();
            impApv = Base36aBase10(referencia.Substring(35, 7)).ToString();
            impAcv = Base36aBase10(referencia.Substring(42, 7)).ToString();
        }

        private long Base36aBase10(string base36)
        {
            long valor = 0;
            const string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for (int i = 0; i < base36.Length; i++)
            {
                char car = base36[i];
                int index = guarismosB36.IndexOf(car);
                if (index != -1)
                {
                    valor = valor * 36 + index;
                }
            }

            return valor;
        }

        private string GetFechaVenc(string fechaCodificada)
        {
            if (!int.TryParse(fechaCodificada, out int fechaEntero)) return "00000000";

            int año = 2009 + (fechaEntero / 372);
            fechaEntero = fechaEntero % 372;

            int mes = (fechaEntero / 31) + 1;
            int dia = (fechaEntero % 31) + 1;

            return $"{año}{mes:00}{dia:00}";
        }

        public int ValidaReferencia(string referencia, out string respcode)
        {
            respcode = "00";
            if (referencia.Length != 53)
            {
                respcode = "12";
                GrabaLog("Longitud de referencia inválida", "ERR");
                return 1;
            }

            int[] arreglo = new int[49];
            long sumaRP = 0, sumaP = 0, sumaO = 0, sumaSUA = 0, sumaFV = 0, sumaIMSS = 0, sumaRCV = 0, sumaAPV = 0, sumaACV = 0;

            const string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            for (int i = 0; i < 49; i++)
            {
                int val = guarismosB36.IndexOf(referencia[i]);
                arreglo[i] = val * (49 - i);
            }

            // Sums
            for (int i = 0; i <= 7; i++) sumaRP += arreglo[i];
            for (int i = 8; i <= 11; i++) sumaP += arreglo[i];
            sumaO = arreglo[12];
            for (int i = 13; i <= 16; i++) sumaSUA += arreglo[i];
            for (int i = 17; i <= 20; i++) sumaFV += arreglo[i];
            for (int i = 21; i <= 27; i++) sumaIMSS += arreglo[i];
            for (int i = 28; i <= 34; i++) sumaRCV += arreglo[i];
            for (int i = 35; i <= 41; i++) sumaAPV += arreglo[i];
            for (int i = 42; i <= 48; i++) sumaACV += arreglo[i];

            long segRP = (sumaRP % 881) * 883;
            long segP = (sumaP % 883) * 881;
            long segO = (sumaO % 881) * 883;
            long segSUA = (sumaSUA % 883) * 881;
            long segFV = (sumaFV % 881) * 883;
            long segIMSS = (sumaIMSS % 883) * 881;
            long segRCV = (sumaRCV % 881) * 883;
            long segAPV = (sumaAPV % 883) * 881;
            long segACV = (sumaACV % 881) * 883;

            long result = Math.Abs(1679615 - segRP - segP - segO - segSUA - segFV - segIMSS - segRCV - segAPV - segACV);
            GrabaLog("Result: " + result, "");

            string codigoV = GetCodigoVerificacion((int)result);
            GrabaLog("CodigoV: " + codigoV, "");

            if (codigoV == referencia.Substring(49))
            {
                respcode = "00";
                return 0;
            }
            else
            {
                respcode = "14";
                return 1;
            }
        }

        private string GetCodigoVerificacion(int numero)
        {
            const string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            int original = numero;

            for (int i = 0; i < 4; i++)
            {
                res.Insert(0, guarismosB36[numero % 36]);
                numero /= 36;
            }

            string codigoVerificacion = res.ToString();

            if (numero > 0)
            {
                codigoVerificacion = guarismosB36[numero % 36] + codigoVerificacion.Substring(0, 3);
            }

            return codigoVerificacion;
        }
    }
}
