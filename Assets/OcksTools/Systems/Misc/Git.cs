using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Git : MonoBehaviour
{
    public static string GitLocation = @"C:\Program Files\Git\bin\git.exe";
    public static string Command(string pathtorepo, string cmd)
    {
        //this is a really dumb way to do this
        ProcessStartInfo gitInfo = new ProcessStartInfo();
        gitInfo.CreateNoWindow = true;
        gitInfo.RedirectStandardError = true;
        gitInfo.RedirectStandardOutput = true;
        gitInfo.UseShellExecute = false;
        gitInfo.FileName = GitLocation;

        Process gitProcess = new Process();
        gitInfo.Arguments = cmd;
        gitInfo.WorkingDirectory = pathtorepo;

        gitProcess.StartInfo = gitInfo;
        gitProcess.Start();

        string stderr_str = gitProcess.StandardError.ReadToEnd();  // pick up STDERR
        string stdout_str = gitProcess.StandardOutput.ReadToEnd(); // pick up STDOUT

        gitProcess.WaitForExit();
        gitProcess.Close();
        if (stderr_str != null && stderr_str.Length>3) return $"{stderr_str}";
        return stdout_str;
    }


}
