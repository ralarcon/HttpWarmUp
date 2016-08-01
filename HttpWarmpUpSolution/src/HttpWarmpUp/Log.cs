using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace HttpWarmpUp
{
    public static class Log
    {
        static TraceSwitch eventLogSwitch = new System.Diagnostics.TraceSwitch("HttpWarmUpLogTraceSwitch", "Http Warm Up Application");

        public static StringBuilder sbWarnings = new StringBuilder();
        public static StringBuilder sbErrors = new StringBuilder();
        public static StringBuilder sbInfo = new StringBuilder();

        public static void WriteTabbedWarning(int tabCount, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Out.Write(new String('\t', tabCount));
            Console.Out.WriteLine(text, args);
            Console.ForegroundColor = currentColor;

            sbWarnings.AppendLine(String.Format(text, args));   
        }

        public static void FlushInfoToEventLog()
        {
            try
            {
                if (eventLogSwitch.TraceInfo && sbInfo.Length > 0)
                {
                    string logData = GetLogData(sbInfo);
                    Trace.IndentLevel = 0;
                    Trace.TraceInformation("Registered Info Messages:\n{0}", logData);
                }
            }
            catch(Exception ex)
            {
                Log.WriteTabbedError(0, "No se han podido escribir los eventos Informativos al visor de enventos. Revise la configuración. Sección: <system.diagnostics>. Más información en: http://msdn.microsoft.com/en-us/library/1txedc80(VS.71).aspx\n\nExcepción que se ha producido:{0}", ex.ToString());

            }
        }

        public static void FlushWarningsToEventLog()
        {
            try
            {
                if (eventLogSwitch.TraceWarning && sbWarnings.Length > 0)
                {
                    string logData = GetLogData(sbWarnings);
                    Trace.IndentLevel = 0;
                    Trace.TraceWarning("Registered Warnings:\n{0}", logData);
                }
            }
            catch (Exception ex)
            {
                Log.WriteTabbedError(0, "No se han podido escribir los eventos de Aviso al visor de enventos. Revise la configuración. Sección: <system.diagnostics>. Más información en: http://msdn.microsoft.com/en-us/library/1txedc80(VS.71).aspx\n\nExcepción que se ha producido:{0}", ex.ToString());

            }
        }

        public static void FlushErrorsToEventLog()
        {
            try
            {
                if (eventLogSwitch.TraceError && sbErrors.Length > 0)
                {
                    string logData = GetLogData(sbErrors);
                    Trace.IndentLevel = 0;
                    Trace.TraceError("Registered Errors:\n{0}", logData);
                }
            }
            catch (Exception ex)
            {
                Log.WriteTabbedError(0, "No se han podido escribir los eventos de Error al visor de enventos. Revise la configuración. Sección: <system.diagnostics>. Más información en: http://msdn.microsoft.com/en-us/library/1txedc80(VS.71).aspx\n\nExcepción que se ha producido:{0}", ex.ToString());

            }
        }

        private static string GetLogData(StringBuilder sb)
        {
            string logData = String.Empty;
            if (sb.Length > 15000)
            {
                logData = sb.ToString().Substring(0, 15000) + "[... truncated ...]";
            }
            else
            {
                logData = sb.ToString();
            }
            return logData;
        }

        public static void WriteTabbedInfo(int tabCount, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Out.Write(new String('\t', tabCount));
            Console.Out.WriteLine(text, args);
            Console.ForegroundColor = currentColor;
            sbInfo.AppendLine(String.Format(text, args));
        }

        public static void WriteTabbedError(int tabCount, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Out.Write(new String('\t', tabCount));
            Console.Out.WriteLine(text, args);
            Console.ForegroundColor = currentColor;

            sbErrors.AppendLine(String.Format(text, args));
        }

        public static void WriteTabbedText(int tabCount, string text, params object[] args)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Out.Write(new String('\t', tabCount));
            Console.Out.WriteLine(text, args);
            Console.ForegroundColor = currentColor;

            sbInfo.AppendLine(String.Format(text, args));
        }
    }
}
