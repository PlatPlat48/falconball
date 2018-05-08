using UnityEngine;
using System;
using System.IO;
using System.Diagnostics;


public class FalconIO : MonoBehaviour
{
	private Process process;
	public StreamWriter messageStream;

	void Start() {
		StartProcess();
	}
	void Update() {
		process.StandardInput.WriteLine("");
		//messageStream.WriteLine("asdf");
	}

    void StartProcess()
	{
        try
        {
            process = new Process();
            process.EnableRaisingEvents = false;
            process.StartInfo.FileName = Application.dataPath+"/native/falcon_test_cli";
            process.StartInfo.Arguments = "--z_wall_test";
			process.StartInfo.WorkingDirectory = Application.dataPath+"/native";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += new DataReceivedEventHandler( DataReceived );
            process.ErrorDataReceived += new DataReceivedEventHandler( ErrorReceived );
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            messageStream = process.StandardInput;

            UnityEngine.Debug.Log( "Successfully launched process." );
        }
        catch( Exception e )
        {
            UnityEngine.Debug.LogError( "Unable to launch process: " + e.Message );
        }
    }


    void DataReceived( object sender, DataReceivedEventArgs eventArgs )
	{
		if (eventArgs.Data != null)
			UnityEngine.Debug.Log( eventArgs.Data );
    }


    void ErrorReceived( object sender, DataReceivedEventArgs eventArgs )
	{
		if (eventArgs.Data != null)
        	UnityEngine.Debug.LogError( eventArgs.Data );
    }


    void OnApplicationQuit()
    {
        if( process != null || !process.HasExited )
        {
            process.Kill();
        }
    }
}
