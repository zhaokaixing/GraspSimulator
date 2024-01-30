using System;
using System.IO;
using System.Diagnostics;

public class Logger
{
    private Stopwatch _stopwatch;

    //private DateTime _epoch;
    //private DateTime Epoch
    //{
    //    get
    //    {
    //        if (_epoch == null)
    //        {
    //            _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    //        }
    //        _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    //        return _epoch;
    //    }
    //}


    public Logger(string logFilePath)
    {
        if (!logFilePath.EndsWith(".log") && !logFilePath.EndsWith(".csv"))
            logFilePath += ".log";
        LogFilePath = logFilePath;
        if (!File.Exists(LogFilePath))
            File.Create(LogFilePath).Close();
        //WriteLine("New Session Started");
    }

    public string LogFilePath { get; private set; }


    public void StartTimer()
    {
        _stopwatch = new Stopwatch();
        _stopwatch.Start();
    }

    // in milliseconds
    public long TimeElapsedSinceStart()
    {
        _stopwatch.Stop();
        var elapsedTime = _stopwatch.ElapsedMilliseconds;
        _stopwatch.Start();
        return elapsedTime;
    }

    public void WriteLine(object message)
    {
        using (StreamWriter writer = new StreamWriter(LogFilePath, true))
            writer.WriteLine(message.ToString());
    }

}
