using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace CheckFolderAndMailService
{
    partial class FolderMailerService : ServiceBase
    {
#pragma warning disable CS0618 // Type or member is obsolete
        private static readonly string FOLDER_PATH = ConfigurationSettings.AppSettings["File_Location"];
        private static readonly string USER_NAME = ConfigurationSettings.AppSettings["Username_Sender"];
        private static readonly string PASSWORD = ConfigurationSettings.AppSettings["Password_Sender"];
        private static readonly string USER_NAME_RECIEVER = ConfigurationSettings.AppSettings["Username_Reciever"];
#pragma warning restore CS0618 // Type or member is obsolete
        //private static readonly string FOLDER_PATH_NEW_FILES = FOLDER_PATH + @"New Files\";
        private static readonly string FOLDER_PATH_LOG = FOLDER_PATH + @"M3-2Log.txt";

        private List<String> Filepaths;
        private List<String> LastIntervalFilePaths;   //Paths of files in last interval 
        private List<long> LastIntervalFileSizes;     //Size of files in last interval in bytes
        private String EmailBody;
        System.Timers.Timer timeDelay;

        public FolderMailerService()
        {
            InitializeComponent();
            timeDelay = new System.Timers.Timer();
            timeDelay.Interval = 900000;                     //15 minutes 900000
                                                             //30 seconds 30000
            timeDelay.Elapsed += new System.Timers.ElapsedEventHandler(ChainOfWorkerProcesses);
        }

        private void ChainOfWorkerProcesses(object sender, ElapsedEventArgs e)
        {
            GetAllFiles();
            CheckIfDifferentAndMakeEmailBody();
            SendEmail();
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

        private void GetAllFiles()
        {
            try
            {
                String[] wrapperArray = Directory.GetFiles(FOLDER_PATH, "*.*", SearchOption.TopDirectoryOnly);

                Filepaths = wrapperArray.OfType<String>().ToList();
                //for Debugging
                foreach (var path in Filepaths)
                {
                    LogService("File Name: " + path);
                }
            }
            catch (Exception e)
            {
                LogService("Exception detected in GetAllFiles()");
            }
        }

        private void CheckIfDifferentAndMakeEmailBody()
        {
            foreach (var path in Filepaths)
            {
                try
                {
                    if (!LastIntervalFilePaths.Contains(path))
                    {
                        EmailBody+="New File detected: " + path + "\nSize: " + new FileInfo(path).Length + "\n\n";
                        LogService("Saving in email body: " + path + " Size: " + new FileInfo(path).Length);
                    }
                    else if (LastIntervalFileSizes[LastIntervalFilePaths.IndexOf(path)] != new FileInfo(path).Length)
                    {
                        EmailBody+="Modified File Detected: " + path + "\nSize: " + new FileInfo(path).Length + "\n\n";
                        LogService("Saving in email body; Change in size of: " + path + " Size: " + new FileInfo(path).Length);
                        
                    }

                }
                catch (Exception e)
                {
                    LogService("Problem in copying file in CheckIfDifferentAndCopy() Function");
                }
            }
        }


        private void SendEmail()
        {
            if (EmailBody != null)
            {
                LogService("///////////////////////////////New mail\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\\ \n");
                LogService("Mail should send now or somethingss wrong");
                SmtpClient SmtpServer = new SmtpClient("smtp.live.com");
                var mail = new MailMessage();

                mail.From = new MailAddress(USER_NAME);
                mail.To.Add(USER_NAME_RECIEVER);
                mail.Subject = "Changes in folder " + FOLDER_PATH;
                mail.IsBodyHtml = true;
                mail.Body = EmailBody.ToString(); ;


                try
                {

                    SmtpServer.Port = 587;
                    SmtpServer.UseDefaultCredentials = false;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(USER_NAME, PASSWORD);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    LogService("Mail sent!");


                }
                catch (Exception inSendingMail)
                {
                    LogService("Invalid operations exception probably in SendMail() " + inSendingMail);
                }
            }
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
            catch (Exception files)
            {
                LogService("Problem in Copying previous files " + files);
            }

        }

        private void EndOfInterval()
        {
            Filepaths.Clear();
            EmailBody = null;
            EmailBody+="hi";
            LogService("TESTING:" + EmailBody);
            EmailBody = null;

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
            // TODO: Add code here to perform any tear-down necessary to stop your service.
            LogService("Service Stopping! ");
        }
    }
}
