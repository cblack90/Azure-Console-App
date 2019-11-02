using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;

namespace TestAzure
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Azure Blob Storage - .Net quickstart sample\n");

            //Rum the examples asynchronously, wait for the results before proceeding
            ProcessAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press any key to exit the sample application.");
            Console.ReadLine();
        }
        private static async Task ProcessAsync()
        {
            // Retrieve the connection string for use with the application. The storage 
            // connection string is stored in an environment variable on the machine 
            // running the application called CONNECT_STR. If the 
            // environment variable is created after the application is launched in a 
            // console or with Visual Studio, the shell or application needs to be closed
            // and reloaded to take the environment variable into account.
            string storageConnectionString = Environment.GetEnvironmentVariable("CONNECT_STR");

            //Check whether the connection string can be parsed.
            CloudStorageAccount storageAccount;
            if(CloudStorageAccount.TryParse(storageConnectionString, out storageAccount))
            {
                // If the connection string is valid, proceed with operations against Blob
                // storage here.
                // ADD OTHER OPERATIONS HERE

                //Create the CloudBlobClient that represents the Blob sotrage endpoint for the storage account.
                CloudBlobClient cloudBlobClient = storageAccount.CreateCloudBlobClient();

                //create a containt called 'testingfcv' and append a GUID value to it to make the name uniqued.
                CloudBlobContainer cloubBlobContainer = cloudBlobClient.GetContainerReference("testfcv" + Guid.NewGuid().ToString());
                await cloubBlobContainer.CreateAsync();

                //Set the permission so the blobs are public.
                BlobContainerPermissions permissions = new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                };
                await cloubBlobContainer.SetPermissionsAsync(permissions);

                //Create a file in your local MyDocuments folder to upload to a blob.
                string localPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string localFileName = "QuickStart_" + Guid.NewGuid().ToString() + ".txt";
                string sourceFile = Path.Combine(localPath, localFileName);
                //Write text to the file.
                File.WriteAllText(sourceFile, "Hello, World!");

                Console.WriteLine("Temp file = {0}", sourceFile);
                Console.WriteLine("Uploading to Blob storage as blob '{0}'", localFileName);
                // Get a reference to the blob address, then upload the file to the blob.
                // Use the value of localFileName fore the blob name.
                CloudBlockBlob cloudBlockBlob = cloubBlobContainer.GetBlockBlobReference(localFileName);
                await cloudBlockBlob.UploadFromFileAsync(sourceFile);

                //List the blobs in the container.
                Console.WriteLine("List blobs in container.");
                BlobContinuationToken blobContinuationToken = null;
                do
                {
                    var results = await cloubBlobContainer.ListBlobsSegmentedAsync(null, blobContinuationToken);
                    //Get the value of the continuation token returned by thelisting call.
                    blobContinuationToken = results.ContinuationToken;
                    foreach (IListBlobItem item in results.Results)
                    {
                        Console.WriteLine(item.Uri);
                    }
                } while (blobContinuationToken != null); //Loop while the continuation token is not null.

                // Download the blob to a local file, using the reference created earlier.
                // Append the string "_DOWNLOADED" before the .txt extension so that you 
                // can see both files in MyDocuments.
                string destinationFile = sourceFile.Replace(".txt", "_DOWNLOADED.txt");
                Console.WriteLine("Downloading blob to {0}", destinationFile);
                await cloudBlockBlob.DownloadToFileAsync(destinationFile, FileMode.Create);

                Console.WriteLine("Press the 'Enter' key to delete the example files, " + "example containter, and exit the application.");
                Console.ReadLine();
                //Clean up the resources. This includes the containter and the two temp files.
                Console.WriteLine("Deleting the container");
                if (cloubBlobContainer != null)
                {
                    await cloubBlobContainer.DeleteIfExistsAsync();
                }
                Console.WriteLine("Deleting the source, and downloaded files");
                File.Delete(sourceFile);
                File.Delete(destinationFile);
            }
            else
            {
                //Otherwise, let the user know that they need to define the environment variable. " 
                Console.WriteLine("A connectin string has has not been defined in the system environment variables. " + "Add an environment variable named 'CONNECT_STR' with your storage " + "connection string as a value.");
                Console.WriteLine("Press any key to exit the application.");
                Console.ReadLine();
            }
        }
    }
}
