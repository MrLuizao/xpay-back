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
    public class generarReferenciaNumerica
    {

        public int generarBD(string referencia, ref string respcode, ref string referenciaNumerica)
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
                SQLComando.CommandText = "ReferenciaNumericaXPay_Generar";
                SQLComando.Parameters.AddWithValue("@Referencia", referencia);
                SQLComando.Connection.Open();

                using (SqlDataReader sqlReader = SQLComando.ExecuteReader())
                {
                    if (sqlReader.HasRows)
                    {
                        while (sqlReader.Read())
                        {
                            respcode = sqlReader["RESPCODE"].ToString();
                            referenciaNumerica = sqlReader["ReferenciaFinal"].ToString();

                            GrabaLog($"RespCode: {respcode}, RefNum: {referenciaNumerica}", "ReferenciaNumericaXPay_Generar");
                        }
                    }
                    else
                    {
                        GrabaLog("El SP no devolvió registros.", "ReferenciaNumericaXPay_Generar");
                        return 1;
                    }
                }

                return 0;
            }
            catch (Exception ex)
            {
                GrabaLog("Ocurrio un error: " + ex.Message, "InsertAbonoDB");
                return 1;
            }
            finally
            {
                conn.Close();
            }

        }

        public void ObtenerCampos(
    string Referencia,
    ref string RegPat,
    ref string PerPag,
    ref string Origen,
    ref string FSUA,
    ref string FechVenc,
    ref string ImpIMSS,
    ref string ImpRCV,
    ref string ImpAPV,
    ref string ImpACV)
        {
            RegPat = Referencia.Substring(0, 1) + Base36aBase10(Referencia.Substring(1, 7)).ToString();
            PerPag = Base36aBase10(Referencia.Substring(8, 4)).ToString();
            Origen = Base36aBase10(Referencia.Substring(12, 1)).ToString();
            FSUA = Base36aBase10(Referencia.Substring(13, 4)).ToString();
            FechVenc = FechaVenc(Referencia.Substring(17, 4));
            ImpIMSS = Base36aBase10(Referencia.Substring(21, 7)).ToString();
            ImpRCV = Base36aBase10(Referencia.Substring(28, 7)).ToString();
            ImpAPV = Base36aBase10(Referencia.Substring(35, 7)).ToString();
            ImpACV = Base36aBase10(Referencia.Substring(42, 7)).ToString();
        }

        private long Base36aBase10(string base36)
        {
            long valor = 0;

            for (int i = 0; i < base36.Length; i++)
            {
                char car = base36[i];
                valor = valor * 36 + CodifBase36(car);
            }

            return valor;
        }

        private int CodifBase36(char car)
        {
            string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return guarismosB36.IndexOf(car);
        }

        private string FechaVenc(string fechaCodificada)
        {
            int fechaEntero = Convert.ToInt32(fechaCodificada);

            int año = 2009 + (fechaEntero / 372);
            fechaEntero = fechaEntero % 372;

            int mes = (fechaEntero / 31) + 1;
            int dia = (fechaEntero % 31) + 1;

            return año.ToString() + mes.ToString("00") + dia.ToString("00");
        }

        public int ValidaReferencia(string Referencia, ref string Respcode)
        {
            int i;
            int[] Arreglo = new int[53];
            int[] Arreglo2 = new int[53];
            int j = 49;
            int SumaRP = 0;
            int SumaP = 0;
            int SumaO = 0;
            int SumaSUA = 0;
            int SumaFV = 0;
            int SumaIMSS = 0;
            int SumaRCV = 0;
            int SumaAPV = 0;
            int SumaACV = 0;
            int SegRP = 0;
            int SegP = 0;
            int SegO = 0;
            int SegSUA = 0;
            int SegFV = 0;
            int SegIMSS = 0;
            int SegRCV = 0;
            int SegAPV = 0;
            int SegACV = 0;
            int Result;
            string CodigoV = string.Empty;

            string XX = "";

            // Valor por defecto
            int ValidaReferencia = 0;

            if (Referencia.Length != 53)
            {
                ValidaReferencia = 1;
                Respcode = "12";

                GrabaLog("Longitud de referencia inválida","ERR");
                //Console.WriteLine("<RETURN> PARA CONTINUAR");
                //XX = Console.ReadLine();

                return ValidaReferencia;
            }

            //
            // Obtener los dos arreglos de longitud 49
            //
            j = 49;
            for (i = 0; i <= 48; i++)
            {
                Arreglo[i] = CodifBase36(Referencia[i]);
                Arreglo2[i] = Arreglo[i] * j;
                j = j - 1;
            }

            //
            // Calcular las sumas por segmento
            //

            // REGISTRO PATRONAL
            for (i = 0; i <= 7; i++)
                SumaRP += Arreglo2[i];

            // PERIODO DE PAGO
            for (i = 8; i <= 11; i++)
                SumaP += Arreglo2[i];

            // ORIGEN
            SumaO = Arreglo2[12];

            // FOLIO SUA
            for (i = 13; i <= 16; i++)
                SumaSUA += Arreglo2[i];

            // FECHA VENCIMIENTO
            for (i = 17; i <= 20; i++)
                SumaFV += Arreglo2[i];

            // IMPORTE IMSS
            for (i = 21; i <= 27; i++)
                SumaIMSS += Arreglo2[i];

            // IMPORTE RCV
            for (i = 28; i <= 34; i++)
                SumaRCV += Arreglo2[i];

            // IMPORTE APV
            for (i = 35; i <= 41; i++)
                SumaAPV += Arreglo2[i];

            // IMPORTE ACV
            for (i = 42; i <= 48; i++)
                SumaACV += Arreglo2[i];

            //
            // Ajustar sumas según el algoritmo
            //
            SegRP = (SumaRP % 881) * 883;
            SegP = (SumaP % 883) * 881;
            SegO = (SumaO % 881) * 883;
            SegSUA = (SumaSUA % 883) * 881;
            SegFV = (SumaFV % 881) * 883;
            SegIMSS = (SumaIMSS % 883) * 881;
            SegRCV = (SumaRCV % 881) * 883;
            SegAPV = (SumaAPV % 883) * 881;
            SegACV = (SumaACV % 881) * 883;

            //
            // Calcular resultado final
            //
            Result = 1679615 - SegRP - SegP - SegO - SegSUA - SegFV - SegIMSS - SegRCV - SegAPV - SegACV;
            Result = Math.Abs(Result);

            GrabaLog("Result: " + Result.ToString(),"");

            //
            // Obtener los dígitos verificadores
            //
            CodigoV = CodigoVerificacion(Result);

            GrabaLog("CodigoV: " + CodigoV,"");

            if (CodigoV == Referencia.Substring(49))
            {
                ValidaReferencia = 0;
                Respcode = "00";
            }
            else
            {
                ValidaReferencia = 1;
                Respcode = "14";
            }

            GrabaLog("RespCode: " + Respcode,"");
            //Console.WriteLine("<ENTER> PARA CONTINUAR");
            //XX = Console.ReadLine();

            return ValidaReferencia;
        }

        private string CodigoVerificacion(int numero)
        {
            //
            // Convierte el número decimal a base 36 (4 caracteres menos significativos)
            //
            string guarismosB36 = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            string Res = string.Empty;

            Res = guarismosB36[numero % 36] + Res;
            numero = numero / 36;

            Res = guarismosB36[numero % 36] + Res;
            numero = numero / 36;

            Res = guarismosB36[numero % 36] + Res;
            numero = numero / 36;

            Res = guarismosB36[numero % 36] + Res;
            numero = numero / 36;

            string CodigoVerificacion = Res;

            //
            // Actualización no documentada del algoritmo
            //
            if (numero > 0)
            {
                CodigoVerificacion = CodigoVerificacion.Substring(0, 3);
                CodigoVerificacion = guarismosB36[numero % 36] + CodigoVerificacion;
            }

            return CodigoVerificacion;
        }


    }

    public class generaReferenciaNumericaModel
    {
        public string referencia { get; set; }
    }

    public class generaReferenciaNumericaResponse
    {
        public string respcode { get; set; }
        public string referenciaNumerica { get; set; }
        public DateTime? vigencia { get; set; }
        public double? monto { get; set; }
    }


}