using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.IO;

namespace AzureDBExport_ConsoleApp
{
    public class BlobMethods
    {
        private string ContainerName;
        private CloudBlobContainer cloudBlobContainer;

        /// <summary>
        /// Contructor to create cloud blob container object
        /// </summary>
        /// <param name="storageAccountName"></param>
        /// <param name="storageAccountKey"></param>
        /// <param name="containerName"></param>
        public BlobMethods(string storageAccountName, string storageAccountKey
                            , string containerName)
        {
            cloudBlobContainer = SetUpContainer(storageAccountName, storageAccountKey, containerName);
            ContainerName = containerName;
        }

        private CloudBlobContainer SetUpContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            string connectionString = string.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net");
            CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse(connectionString);
            CloudBlobClient cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();
            try
            {
                return cloudBlobClient.GetContainerReference(containerName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in getting blob container reference.", ex);
            }
        }
        /// <summary>
        /// Downloads blob file to given path
        /// </summary>
        /// <param name="blobName"></param>
        public void DownloadFileFromBlob(string blobName, string localFilePath)
        {
            CloudBlockBlob blobSource = GetBlobSource(blobName);
            if (blobSource.Exists())
            {
                //string localPath = Path.Combine(localFilePath, blobSource.Name.Replace(@"/", @"\"));
                string localPath = localFilePath;
                string dirPath = Path.GetDirectoryName(localPath);
                if (!Directory.Exists(localPath))
                {
                    Directory.CreateDirectory(localPath);
                }
                blobSource.DownloadToFile(localPath, FileMode.Create);
            }
        }
        private CloudBlockBlob GetBlobSource(string blobName)
        {
            try
            {
                return cloudBlobContainer.GetBlockBlobReference(blobName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in getting blob reference.", ex);
            }
        }
        /// <summary>
        /// Deletes blob,if eixsts, from container
        /// </summary>
        /// <param name="blobName"></param>
        /// <returns>Status of delete request</returns>
        public string DeleteBlob(string blobName)
        {
            CloudBlockBlob blobSource = GetBlobSource(blobName);
            bool blobExisted = false;
            try
            {
                blobExisted = blobSource.DeleteIfExists();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error deleting blob {blobName}.", ex);
            }
            return blobExisted ? "Blob existed. Deleted." : "Blob did not exist.";
        }
    }
}
