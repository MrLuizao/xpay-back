using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace ReferenciaXPayAPI
{
    public class Utilidades
    {
        public static void GrabaLog(string text, string flag)
        {
            string APPNAME = "ReferenciaXPayAPI";
            string APPKEY = @"Software\ANTAD";
            string llave = APPKEY + "\\" + APPNAME;
            string rutaLog;

            // Abrir la clave del registro en HKLM
            using (RegistryKey regVersion = Registry.LocalMachine.OpenSubKey(llave, false))
            {
                if (regVersion != null)
                {
                    rutaLog = regVersion.GetValue("LogFiles", "").ToString();                    
                }
                else
                {
                    rutaLog = String.Empty;                    
                }
            }
            
            string nombreArchivo = string.Empty;
            string nombreArchivoCompleto = string.Empty;
            StreamWriter stream;
            DateTime fecha = DateTime.Now;
            StringBuilder messageLog = new StringBuilder();

            try
            {
                nombreArchivo = "ReferenciaXPayAPI_" + fecha.Year.ToString()
                                          + fecha.Month.ToString().PadLeft(2, '0')
                                          + fecha.Day.ToString().PadLeft(2, '0')
                                          + ".txt";

                nombreArchivoCompleto = rutaLog + @"\" + nombreArchivo;

                messageLog.Append(DateTime.Now.ToString());
                messageLog.Append("\t");
                messageLog.Append(flag);
                messageLog.Append("\t");
                messageLog.Append(text);

                if (!File.Exists(nombreArchivoCompleto))
                {
                    stream = File.CreateText(nombreArchivoCompleto);
                    stream.WriteLine(messageLog);
                    stream.Flush();
                    stream.Close();
                }
                else
                {
                    stream = File.AppendText(nombreArchivoCompleto);
                    stream.WriteLine(messageLog);
                    stream.Flush();
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("AppMessage: Error en la aplicación, favor de verificarlo. " + ex.Message);
                //GrabaLog("AppError: Error en la aplicación, favor de verificarlo. " + ex.Message.ToString(), "Err Log");
            }

        }
    }
}