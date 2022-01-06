using System.Net.Http.Headers;
using System.Text;
using CSharpFileUpload.Models;
using Flurl;
using Flurl.Http;

namespace CSharpFileUpload.SupportClass
{

    public class FileTask
    {
        public string AuthToken { get; set; }
        private readonly HttpClient ClientForHttp;
        public string ServerAPIUrl { get; set; }
        public FileTask(string url, string token = "")
        {
            AuthToken = token;
            ServerAPIUrl = url;
            ClientForHttp = new HttpClient();
            FlurlHttp.Configure(settings =>
            {
                settings.Redirects.Enabled = true; // default true
                settings.Redirects.AllowSecureToInsecure = true; // default false
                settings.Redirects.ForwardAuthorizationHeader = true; // default false
                                                                      //settings.Redirects.MaxAutoRedirects = 5; // default 10 (consecutive)
                                                                      // settings.ConnectionLeaseTimeout = new TimeSpan(0, 2, 0);
                                                                      //settings.HttpClientFactory = new OHttpClientFactory();
            });
        }
        public FileTask(string url)
        {
            ServerAPIUrl = url;
            ClientForHttp = new HttpClient();
            FlurlHttp.Configure(settings =>
            {
                settings.Redirects.Enabled = true; // default true
                settings.Redirects.AllowSecureToInsecure = true; // default false
                settings.Redirects.ForwardAuthorizationHeader = true; // default false
                                                                      //settings.Redirects.MaxAutoRedirects = 5; // default 10 (consecutive)
                                                                      // settings.ConnectionLeaseTimeout = new TimeSpan(0, 2, 0);
                                                                      //settings.HttpClientFactory = new OHttpClientFactory();
            });
            AuthToken = GetAuthToken();
        }
        #region AuthenticateToken

        public string GetAuthToken()
        {
            using (var request = new HttpRequestMessage(HttpMethod.Get, ServerAPIUrl))
            {
                string resultStr = "";
                Task<IFlurlResponse> taskAuthenticate = ServerAPIUrl.PostJsonAsync(new { Username = "Jai", Password = "test12345" });
                taskAuthenticate.Wait();
                resultStr = taskAuthenticate.Result.ToString();
                using (var streamReader = new StreamReader(taskAuthenticate.Result.ResponseMessage.Content.ReadAsStream()))
                {
                    resultStr = streamReader.ReadToEnd();
                }
                return resultStr;
            }
        }
        #endregion


        #region Upload Functions
        public bool UploadFile(string path)
        {

            bool confirmation = false;
            // Get File from File Path
            string fileName = Path.GetFileName(path);

            // Convert File into Byte

            string[] fileNameSplitArr = fileName.Split(".");
            string fileExtension = "";
            dynamic data_bytes;
            if (fileNameSplitArr.Length > 1)
            {
                fileExtension = fileNameSplitArr[fileNameSplitArr.Length - 1].ToLower();
            }

            List<string> imageExtensions = new List<string>() { "png", "jpg", "jpeg", "gif" };
            if (imageExtensions.Contains(fileExtension))
            {
                data_bytes = EncodeImageData(path);
            }
            else
            {
                data_bytes = EncodeFileData(path);
            }


            // Post File to Web Api 
            Task newTask = UploadFileToCloud(data_bytes, fileName);
            //newTask.Start();
            newTask.Wait();
            // Verify Upload
            if (newTask.IsCompleted)
            {
                confirmation = true;
            }
            return confirmation;
        }
        private async Task UploadFileToCloud(dynamic data, string fileName)
        {

            using (ClientForHttp)
            {
                // Bearer Token header if needed
                ClientForHttp.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
                ClientForHttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("multipart/form-data"));

                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent(data), "binary_data");
                    content.Add(new StringContent(fileName), "file_name");
                    var stringTask = ClientForHttp.PostAsync(ServerAPIUrl, content);
                    var msg = await stringTask;
                    Console.Write(msg);
                }
            }
        }

        private async Task UploadFileBytesToCloud(byte[] data, string fileName)
        {

            using (ClientForHttp)
            {
                // Bearer Token header if needed
                ClientForHttp.DefaultRequestHeaders.Add("Authorization", "Bearer " + AuthToken);
                ClientForHttp.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

                using (var content = new ByteArrayContent(data))
                {
                    var byteTask = ClientForHttp.PostAsync(ServerAPIUrl + $"?fileName={fileName}", content);
                    var msg = await byteTask;
                    //Console.Write(msg);
                }

            }
        }

        public bool UploadFileAsOctetStream(string path)
        {
            bool confirmation = false;
            // Get File from File Path
            string fileName = Path.GetFileName(path);

            // Convert File into Byte

            string[] fileNameSplitArr = fileName.Split(".");
            string fileExtension = "";
            dynamic data_bytes;
            if (fileNameSplitArr.Length > 1)
            {
                fileExtension = fileNameSplitArr[fileNameSplitArr.Length - 1].ToLower();
            }


            data_bytes = EncodeFileDataToBytes(path);



            // Post File to Web Api 
            Task newTask = UploadFileBytesToCloud(data_bytes, fileName);
            //newTask.Start();
            newTask.Wait();
            // Verify Upload
            if (newTask.IsCompleted)
            {
                confirmation = true;
            }
            return confirmation;
        }
        #endregion


        #region Download Functions
        public JsonData[] RetrieveAiramapList()
        {
            string jsonResult = "";
            Task<CustomResponseBody> GetJsonMessage = GetDownloadList();
            GetJsonMessage.Wait(50000);
            if (GetJsonMessage.IsCompletedSuccessfully)
            {
                JsonData[] data = GetJsonMessage.Result.Data;
                if (data.Length > 0)
                {
                    return data;
                }
            }

            return new JsonData[0];
        }

        private async Task<CustomResponseBody> GetDownloadList()
        {

            using (var request = new HttpRequestMessage(HttpMethod.Get, ServerAPIUrl))
            {
                return await ServerAPIUrl.WithHeaders(new { Authorization = AuthToken, Accept = "application/json", User_Agent = "Flurl" }).GetJsonAsync<CustomResponseBody>();
            }
        }

        public int DownloadAllFiles(string serverUrl, JsonData[] arr)
        {
            int fileCount = 0;
            try
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    // Get Tile List
                    // Download the files necessary
                    Task<string> getInfoTask = GetInfoFromTileList(arr[i].TileUri.Tilemap, arr[i].TileUri.BlobToken);
                    getInfoTask.Wait();
                    string[] file_list = Array.Empty<string>();
                    if (getInfoTask.IsCompletedSuccessfully)
                    {
                        file_list = getInfoTask.Result.Split("\n");
                    }
                    if (file_list.Length > 0)
                    {
                        int gc_safe_count = 0;
                        for (int j = 0; j < file_list.Length; j++)
                        {
                            string formattedPath = file_list[j].Replace("\r", "").Replace(@"\", @"/");
                            string temp_file_uri = $"{arr[i].TileUri.Endpoint + formattedPath + arr[i].TileUri.BlobToken}";
                            byte[] data_bytes = DownloadData(temp_file_uri);
                            SaveDataToFile(arr[i].Name, formattedPath, data_bytes);
                            gc_safe_count++;
                            if (gc_safe_count > 5)
                            {
                                gc_safe_count = 0;
                                GC.Collect();
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return fileCount;
        }

        public async Task<string> GetInfoFromTileList(string tileList, string blobToken)
        {
            List<string> fileList = new List<string>();
            string full_link = $"{tileList}{blobToken}";
            return await full_link.GetStringAsync();
        }

        public byte[] DownloadData(string uri)
        {
            Task<byte[]> getResultStringTask = uri.GetBytesAsync();
            getResultStringTask.Wait();
            return getResultStringTask.Result;
        }
        public void SaveDataToFile(string parent_folder_name, string full_stored_path, byte[] data_bytes)
        {
            try
            {
                string[] rootFolders = full_stored_path.Split("/");
                string fileName = full_stored_path.Split("/")[full_stored_path.Split("/").Length - 1];
                string[] fileNameSplitArr = fileName.Split(".");
                string fileExtension = "";

                if (fileNameSplitArr.Length > 1)
                {
                    fileExtension = fileNameSplitArr[fileNameSplitArr.Length - 1].ToLower();
                }
                var folderName = Path.Combine("Resources", "Files");
                var pathToSave = Path.Combine(Directory.GetCurrentDirectory(), folderName);

                // Check Parent Folder Exists
                string parent_path = Path.Combine(pathToSave, parent_folder_name);
                if (!Directory.Exists(parent_path))
                {
                    Directory.CreateDirectory(parent_path);
                }
                string final_path = parent_path;
                for (int i = 0; i < rootFolders.Length - 1; i++)
                {
                    final_path = Path.Combine(final_path, rootFolders[i]);
                    if (!Directory.Exists(final_path))
                    {
                        Directory.CreateDirectory(final_path);
                    }
                }
                final_path = Path.Combine(final_path, fileName);
                File.WriteAllBytes(final_path, data_bytes);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        #endregion
        #region Utility
        private string EncodeFileData(string path)
        {
            byte[] data = File.ReadAllBytes(path);
            return Encoding.UTF8.GetString(data);
        }

        public string EncodeImageData(string path)
        {
            byte[] bytes = File.ReadAllBytes(path);
            return Convert.ToBase64String(bytes);
        }

        public byte[] EncodeFileDataToBytes(string path)
        {
            return File.ReadAllBytes(path);
        }


        public byte[] DecodeFileData(string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }

        public byte[] DecodeImageData(string data)
        {
            return Convert.FromBase64String(data);

        }
        #endregion
    }
}
