using log4net;
using log4net.Config;

using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Commons.Utilities;


/// <summary>
/// ALL
/// DEBUG
/// INFO
/// WARN
/// ERROR,FATAL
/// </summary>
public enum LogTypes
{
    Debug,
    Warning,
    Info,
    Data,
    Error,
    Exception,
}

/// <summary>
/// log4net config 파일 불러오기
/// </summary>
public static class LogFactory
{
    public const string ConfigFileName = "log4net.config";
    public static string LogPath { get; private set; }
    /// <summary>
    /// 로그 삭제 기간
    /// </summary>
    public static int LogDelPeriod { get; private set; } = 30;


    public static void Configure()
    {
        Type type = typeof(LogFactory);
        //FileInfo assemblyDirectory = AssemblyInfo.GetCodeBaseDirectory(type);
        //string path = Path.Combine(assemblyDirectory.FullName, ConfigFileName);
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var path = Path.Combine(baseDirectory, ConfigFileName);

        var configFile = new FileInfo(path);
        XmlConfigurator.ConfigureAndWatch(configFile);
        var log = LogManager.GetLogger(type);
        log.ToString();

        LogPath = baseDirectory + @"Logs\";
    }
    /// <summary>
    /// 로그 삭제 기간 설정
    /// </summary>
    /// <param name="delPeriod"></param>
    public static void SetDelPeriod(int delPeriod)
    {
        LogDelPeriod = delPeriod;
    }
}

/// <summary>
/// Logger 확장 (Logger Level 추가)
/// </summary>
public static class LoggerExtensions
{
    static readonly log4net.Core.Level DataLevel = new log4net.Core.Level(300000, "Data");

    public static void Data(this ILog log, string message, Exception ex)
    {
        log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DataLevel, message, ex);
    }

    public static void DataFormat(this ILog log, string message, params object[] args)
    {
        string formattedMessage = string.Format(message, args);
        log.Logger.Log(MethodBase.GetCurrentMethod().DeclaringType, DataLevel, formattedMessage, null);
    }
}

/// <summary>
/// xcopy /Y /R "$(ProjectDir)..\Common\log4net.config" "$(ProjectDir)"
/// 
/// LEVEL [ALL DEBUG INFO WARN ERROR FATAL OFF]
/// </summary>
public static class Logger
{
    private static readonly object locker = new object();
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    static Logger()
    {
        //XmlConfigurator.Configure();
        LogFactory.Configure();
    }

    public static void WriteLog(LogTypes logTypes, string logMessage, Exception ex = null)
    {
        lock (locker)
        {
            switch (logTypes)
            {
                case LogTypes.Debug     : Log.Debug(logMessage, ex); break;
                case LogTypes.Warning   : Log.Warn (logMessage, ex); break;
                case LogTypes.Info      : Log.Info (logMessage, ex); break;
                case LogTypes.Data      : Log.Data (logMessage, ex); break;
                case LogTypes.Error     : Log.Error(logMessage, ex); break;
                default:
                    WriteException(logMessage, ex);
                    break;
            }

            DeleteLogFiles();
        }
    }

    public static void WriteLog(LogTypes logTypes, Exception ex)
    {
        WriteLog(logTypes, string.Empty, ex);
    }

    /// <summary>
    /// The write line.
    /// </summary>
    /// <param name="message">
    /// The message.
    /// </param>
    public static void WriteLogAndTrace(LogTypes logTypes, string logMessage, Exception ex = null)
    {
        WriteLog(logTypes, logMessage, ex);
        string debug = logTypes >= LogTypes.Error ? "[{0} Error] {1} \r\n{2}" : string.Empty;
        if (ex != null)
            Debug.WriteLine(debug, GetMethodInfoStrings.GetMethodName(2), logMessage, ex);
        else
        {
            //Debug.WriteLine(logMessage);
            Debug.WriteLine(
                "[" + MethodBase.GetCurrentMethod().ReflectedType.FullName + "] " +
                "[" + MethodBase.GetCurrentMethod().Name + "] " +
                logMessage);
        }
    }

    private static void WriteException(string logMessage, Exception ex)
    {
        var dataString = string.Empty;
        if (ex != null)
        {
            dataString += logMessage;
            dataString += $"\nError Message : {ex.Message}";
            if (null != ex.InnerException)
                dataString += $"\nInnerException Message : {ex.InnerException.Message}";
        }
        else
        {
            dataString += logMessage;
        }

        Log.Fatal(dataString, ex);
    }


    #region Debug Write

    /// <summary>
    /// TRACE method.
    /// </summary>
    /// <param name="originClass">
    /// The origin class of the message.
    /// </param>
    /// <param name="priority">
    /// The priority of the message.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="obj">
    /// String object will be replaced with count in the message.
    /// </param>
    public static void Trace(object originClass, int priority, string message, params object[] obj)
    {
#if DEBUG
        DoTrace(obj, message, originClass, priority);
#endif
    }

    /// <summary>
    /// The TRACE method.
    /// </summary>
    /// <param name="priority">
    /// Priority of the message.
    /// </param>
    /// <param name="message">
    /// The Message.
    /// </param>
    /// <param name="obj">
    /// String object will be replaced with count in the message.
    /// </param>
    public static void Trace(int priority, string message, params object[] obj)
    {
        Trace((object)null, priority, message, obj);
    }

    /// <summary>
    /// TRACE method.
    /// </summary>
    /// <param name="message">
    /// The Message.
    /// </param>
    /// <param name="obj">
    /// String object will be replaced with count in the message.
    /// </param>
    public static void Trace(string message, params object[] obj)
    {
        Trace(-1, message, obj);
    }

    /// <summary>
    /// The do trace.
    /// 디버깅용 트레이스 함수.
    /// </summary>
    /// <param name="obj">
    /// The obj.
    /// </param>
    /// <param name="message">
    /// The message.
    /// </param>
    /// <param name="originClass">
    /// The origin class.
    /// </param>
    /// <param name="priority">
    /// The priority.
    /// </param>
    private static void DoTrace(object[] obj, string message, object originClass, int priority)
    {
        var count = 0;
        foreach (var strObject in obj)
        {
            string strSubFormat = "{" + count + "}";

            message = strObject == null ? message.Replace(strSubFormat, "null") : message.Replace(strSubFormat, strObject.ToString());

            count++;
        }

        string className = string.Empty;

        if (originClass != null)
        {
            className = originClass.GetType().Name;
        }

        var overallMessage = new StringBuilder();

        if (originClass != null)
        {
            overallMessage.Append("[" + className + "] ");
        }

        overallMessage.Append(message);
        if (priority != -1)
        {
            overallMessage.Append("(Priority : " + priority + ")");
        }

        // System.Diagnostics.Trace.WriteLine(overallMessage.ToString());
        Debug.WriteLine(overallMessage.ToString());
    }

    #endregion //Debug Write

    /// <summary>
    /// 기간 후 로그파일 삭제
    /// </summary>
    private static void DeleteLogFile()
    {
        try
        {
            var folderPath = LogFactory.LogPath;
            var dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo != null)
            {
                foreach (var dir in dirInfo.EnumerateDirectories())
                {
                    if (dir.CreationTime < DateTime.Now.AddDays(-LogFactory.LogDelPeriod))
                        dir.Delete(true);
                }
            }
            dirInfo = null;
        }
        catch (Exception ex)
        {
            WriteException("Log File Delete Error", ex);
        }
    }

    /// <summary>
    /// 기간 후 로그파일 삭제 (파일단위)
    /// </summary>
    private static void DeleteLogFiles()
    {
        try
        {
            var folderPath = LogFactory.LogPath;
            var dirInfo = new DirectoryInfo(folderPath);
            if (dirInfo != null)
            {
                foreach (var file in dirInfo.EnumerateFiles())
                {
                    if (file.CreationTime < DateTime.Now.AddDays(-LogFactory.LogDelPeriod))
                        file.Delete();
                }
            }
            dirInfo = null;
        }
        catch (Exception ex)
        {
            WriteException("Log File Delete Error", ex);
        }
    }
}

/// <summary>
/// The get method info strings.
/// </summary>
public class GetMethodInfoStrings
{
    /// <summary>
    /// The get method name.
    /// </summary>
    /// <param name="frame">
    /// The frame.
    /// </param>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetMethodName(int frame = 1)
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(frame);

        if (stackFrame == null)
        {
            return "Service";
        }

        return null != stackFrame.GetMethod() ? stackFrame.GetMethod().Name : string.Empty;
    }

    /// <summary>
    /// The get file name.
    /// </summary>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetFileName()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(1);

        return stackFrame.GetFileName() ?? string.Empty;
    }

    /// <summary>
    /// The get file line number.
    /// </summary>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetFileLineNumber()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(1);

        return 0 != stackFrame.GetFileLineNumber() ? stackFrame.GetFileLineNumber().ToString() : string.Empty;
    }

    /// <summary>
    /// The get caller method name.
    /// </summary>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetCallerMethodName()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(2);

        return null != stackFrame.GetMethod() ? stackFrame.GetMethod().Name : string.Empty;
    }

    /// <summary>
    /// The get calller file name.
    /// </summary>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetCalllerFileName()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(2);

        return stackFrame.GetFileName() ?? string.Empty;
    }

    /// <summary>
    /// The get caller file line number.
    /// </summary>
    /// <returns>
    /// The <see cref="string"/>.
    /// </returns>
    public static string GetCallerFileLineNumber()
    {
        var stackTrace = new StackTrace(true);
        var stackFrame = stackTrace.GetFrame(2);

        return 0 != stackFrame.GetFileLineNumber() ? stackFrame.GetFileLineNumber().ToString() : string.Empty;
    }
}
