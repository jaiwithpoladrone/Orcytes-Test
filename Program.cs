using CSharpFileUpload.Models;
using CSharpFileUpload.SupportClass;

namespace CSharpFileUpload
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                string serverUrl = @"https://localhost:5001/";
                serverUrl = @"https://localhost:7042/";
                bool completed = false;
                Console.WriteLine("Server connected: " + serverUrl);
                string message = "", apiUrl = "", token = "";
                //Console.WriteLine("Enter Token");
                //string? token = Console.ReadLine();
                //if (string.IsNullOrEmpty(token))
                //{
                //    return;
                //}

                #region Authenticate
                apiUrl = @$"{serverUrl}api/Account/gettoken";
                FileTask authenticateTask = new FileTask(apiUrl);
                token = authenticateTask.AuthToken;
                Console.WriteLine("Token received: " + token);
                #endregion

                #region Upload
                message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Upload job started.";
                Console.WriteLine(message);
                apiUrl = @$"{serverUrl}api/Upload";
                string path = @"C:\Users\Jaiprashanth\Downloads\convert\18\tile_8192_1_1.png";
               // path = @"C:\Users\Jaiprashanth\Downloads\test2.mp4";

                FileTask newUploadTask = new FileTask(apiUrl, token);
                completed = newUploadTask.UploadFileAsOctetStream(path);
                Console.WriteLine();
                message = completed ? "Upload completed." : "Upload failed.";
                Console.WriteLine(message);

                message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Upload job ended.";
                Console.WriteLine(message);
                #endregion

                #region Download
                //message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Download job started.";
                //Console.WriteLine(message);

                //apiUrl = @$"http://app.airamap.com/Desktop/ProjectList";
                //FileTask newRetrieveTask = new FileTask(apiUrl, token);

                //JsonData[] retrievedList = newRetrieveTask.RetrieveAiramapList();
                //completed = retrievedList.Length > 0;
                //message = completed ? "Retrieved Airamap Download list successfully" : "Not successful to retrieve list.";
                //Console.WriteLine(message);
                //int filesDownload = 0;
                //if (completed)
                //{

                //    FileTask newDownloadTask = new FileTask(apiUrl, token);
                //    filesDownload = newDownloadTask.DownloadAllFiles(serverUrl, retrievedList);
                //}
                //message = filesDownload > 0 ? $"Download {filesDownload} files successfully" : "No files downloaded";


                //message = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}: Download job ended.";
                //Console.WriteLine(message);

                #endregion
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.ReadKey();
            }



           
        }
    }
}