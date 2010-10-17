using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace FishnSpots
{
	public class Log
	{
		enum FSLogLevel
		{
			Fatal,
			Error,
			Warn,
			Info,
			Debug,
			Trace
		}
		static void FSLogMessage(String File, Int32 Line, FSLogLevel LogLevel, String fmtStr)
		{
			String LogStr = "";
			System.Diagnostics.Debug.Print(LogStr + ":" + File + "(" + Line.ToString() + "):" + fmtStr);
		}
		static void FSLogMessageArgs(String File, Int32 Line, FSLogLevel LogLevel, String fmtStr, params object[] args)
		{
			String LogStr = "";
			System.Diagnostics.Debug.Print(LogStr + ":" + File + "(" + Line.ToString() + "):" + fmtStr, args);
		}
		static void Fatal(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Debug, FormatString);
		}
		static void Fatal(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Debug, FormatString, args);
		}
		static void Error(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Error, FormatString);
		}
		static void Error(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Error, FormatString, args);
		}
		static void Warn(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Warn, FormatString);
		}
		static void Warn(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Warn, FormatString, args);
		}
		static void Info(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Info, FormatString);
		}
		static void Info(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Info, FormatString, args);
		}
		static void Debug(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Debug, FormatString);
		}
		static void Debug(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Debug, FormatString, args);
		}
		static void Trace(String FormatString)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Trace, FormatString);
		}
		static void Trace(String FormatString, object[] args)
		{
			FSLogMessageArgs("", 0, FSLogLevel.Trace, FormatString, args);
		}
	}
}
