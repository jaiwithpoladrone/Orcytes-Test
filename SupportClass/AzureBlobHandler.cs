using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Diagnostics;

namespace CSharpFileUpload.SupportClass
{
    public class AzureBlobHandler
    {
        BlobServiceClient Client { get; set; }
        StorageTransferOptions TransferOptions { get; set; }
        //BlobContainerClient ContainerClient { get; set; }
        public string DownloadPath { get; set; }
        public AzureBlobHandler(string uri)
        {
            Uri endPointUri = new Uri(uri);
            Client = new BlobServiceClient(endPointUri);
            DownloadPath = Path.Combine("Resources", "Files");
            // Specify the StorageTransferOptions
            TransferOptions = new StorageTransferOptions
            {
                // Set the maximum number of workers that 
                // may be used in a parallel transfer.
                MaximumConcurrency = 8,

                // Set the maximum length of a transfer to 50MB.
                MaximumTransferSize = 50 * 1024 * 1024
            };
        }

        public async Task<int> DownloadBlob(string containerName)
        {
            int count = 0;
            try
            {

                Stopwatch timer = Stopwatch.StartNew();
                BlobContainerClient container = Client.GetBlobContainerClient(containerName);
                List<BlobItem> result = new List<BlobItem>(); // Create a queue of tasks that will each upload one file.
                var tasks = new Queue<Task<Response>>();
                // List all blobs in the container
                foreach (BlobItem blobItem in container.GetBlobs())
                {
                    string fileName = DownloadPath + blobItem.Name;
                    Console.WriteLine($"Downloading {blobItem.Name} to {DownloadPath}");
                    BlobClient blob = container.GetBlobClient(blobItem.Name);


                    // Add the download task to the queue
                    tasks.Enqueue(blob.DownloadToAsync(fileName, default, TransferOptions));
                    count++; ;
                }
                timer.Stop();
                // Run all the tasks asynchronously.
                await Task.WhenAll(tasks);

                // Report the elapsed time.
                timer.Stop();
                Console.WriteLine($"Downloaded {count} files in {timer.Elapsed.TotalSeconds} seconds");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            return count;
        }
    }
}
