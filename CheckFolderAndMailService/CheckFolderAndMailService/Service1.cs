using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CheckFolderAndMailService
{
    public partial class FolderService : ServiceBase
    {

#pragma warning disable CS0618 // Type or member is obsolete
        private static readonly string FOLDER_PATH = ConfigurationSettings.AppSettings["File_Location"];
        //private static readonly string USER_NAME = ConfigurationSettings.AppSettings["Username_Sender"];
        //private static readonly string PASSWORD = ConfigurationSettings.AppSettings["Password_Sender"];
#pragma warning restore CS0618 // Type or member is obsolete

        private static readonly string FOLDER_PATH_NEW_FILES = FOLDER_PATH + @"New Files\";
        private static readonly string FOLDER_PATH_LOG = FOLDER_PATH + @"M3-1Log.txt";
        /*Since log is in the same folder and its
        value increases every interval it will 
        be copied to New Files folder everytime.
        Change it's path if log is to be kept somewhere else
        */

        private List<String> Filepaths;
        private List<String> LastIntervalFilePaths;   //Paths of files in last interval with their size to see if they are editted
        private List<long> LastIntervalFileSizes;
        private bool isChanged = false;
        System.Timers.Timer timeDelay;

        public void FolderService()
        {
            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Interval = 60000;                     //1 minute
            timeDelay.Elapsed += new ElapsedEventHandler(ChainOfWorkerProcesses);
            
        }

        private void ChainOfWorkerProcesses(object sender, ElapsedEventArgs e)
        {
            GetAllFiles();
            CheckIfDifferentAndCopy();
            CopyFilePathsToCheckingArray();
            EndOfInterval();
            

        }

        public void onDebug()
        {
            OnStart(null);
        }

        protected override void OnStart(string[] args)
        {
            LogService("Service starting! ");
            if (Filepaths != null)
                Filepaths.Clear();            //Clear Previously saved paths

            LastIntervalFilePaths = new List<string>();     //To get rid of null pointer exception
            LastIntervalFileSizes = new List<long>();       //Initializing list here because it isnt getting initialized anywhere else
            
            GetAllFiles();
            CopyFilePathsToCheckingArray();
            timeDelay.Enabled = true;
        }

        //Increase Delay by 2 minutes
        private void IncreaseTimeDelay()
        {
            if(timeDelay.Interval < 3600000)
            {
                timeDelay.Interval += 120000;
                LogService("Increasing Delay. New Delay: " + timeDelay.Interval + "\n");
            }

        }

        //Reset Delay back to 1 minute
        private void ResetTimeDelay()
        {
            timeDelay.Interval = 60000;
            LogService("Delay reset to " + timeDelay.Interval + "\n");
        }

        //Gets all files path names from the folder path
        protected void GetAllFiles()
        {
            try
            {
                String[] wrapperArray = Directory.GetFiles(FOLDER_PATH, "*.*", SearchOption.TopDirectoryOnly);

                Filepaths = wrapperArray.OfType<String>().ToList();
                //for Debugging
                foreach(var path in Filepaths)
                {
                    LogService("File Name: " + path);
                }
            }
            catch (Exception e)
            {
                LogService("Exception detected in GetAllFiles()");
            }

        }

        protected void CheckIfDifferentAndCopy()
        {
            foreach(var path in Filepaths)
            {
                try
                {
                    if (!LastIntervalFilePaths.Contains(path))
                    {
                        if (!Directory.Exists(FOLDER_PATH_NEW_FILES))
                        {
                            Directory.CreateDirectory(FOLDER_PATH_NEW_FILES);

                        }
                        LogService("\nCopying: "+path +"\n");
                        File.Copy(path, FOLDER_PATH_NEW_FILES + Path.GetFileName(path), true);
                        isChanged = true;

                    }
                    else if(LastIntervalFileSizes[LastIntervalFilePaths.IndexOf(path)] != new FileInfo(path).Length)
                    {
                        if (!Directory.Exists(FOLDER_PATH_NEW_FILES))
                        {
                            Directory.CreateDirectory(FOLDER_PATH_NEW_FILES);

                        }
                        LogService("\nCopying: " + path + "\n");
                        File.Copy(path, FOLDER_PATH_NEW_FILES + Path.GetFileName(path), true);
                        isChanged = true;
                    }
                   
                }
                catch(Exception e)
                {
                    LogService("Problem in copying file in CheckIfDifferentAndCopy() Function");
                }
                
            }
            if (!isChanged)
                IncreaseTimeDelay();
            else
                ResetTimeDelay();

        }
        

        protected void CopyFilePathsToCheckingArray()
        {

            LastIntervalFilePaths.Clear();
            LastIntervalFileSizes.Clear();
            try
            {
                LastIntervalFilePaths = new List<string>(Filepaths);
                
                foreach (var path in LastIntervalFilePaths)
                {
                    try
                    {

                        if (path != null)
                        {
                            long fileSize = new FileInfo(path).Length;
                            //LogService(""+fileSize);
                            LastIntervalFileSizes.Add(fileSize);
                            
                        }
                    }
                    catch
                    {
                        LogService("Problem in copying size");
                             
                    }
                }
                
            }
            catch(Exception files)
            {
                LogService("Problem in Copying previous files " + files);
            }
            
        }

        private void EndOfInterval()
        {
            Filepaths.Clear();
            isChanged = false;

            LogService("\nEnd of Interval \n");
        }


        private static void LogService(string content)
        {
            try
            {


                FileStream fs = new FileStream(FOLDER_PATH_LOG, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);
                sw.BaseStream.Seek(0, SeekOrigin.End);
                sw.WriteLine(content);
                sw.Flush();
                sw.Close();
            }
            catch (Exception e)
            {

            }
        }

        protected override void OnStop()
        {
            LogService("Service Stopping! ");
        }


    }
}
