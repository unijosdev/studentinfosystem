using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace SwiftKampus.Services
{
    public class AzureBlob
    {       
        public CloudBlobContainer GetCloudBlobContainer()
        {
            string connString = "DefaultEndpointsProtocol=https;AccountName=unijosdemostorage;AccountKey=j/SFLxcOgPgTJR4rZ8nXX840SSw3jVzHuBBmfeQiu/TnbATPWoyd3vHAa/FgXoajvsLtq9R54FFosVIllSiZkg==;EndpointSuffix=core.windows.net";
            string destContainer = "unijosblobstorage";

            // Get a reference to the storage account  
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(destContainer);
            if (blobContainer.CreateIfNotExists())
            {
                blobContainer.SetPermissions(new BlobContainerPermissions
                {
                    PublicAccess = BlobContainerPublicAccessType.Container
                });

            }
            return blobContainer;

        }

    }
}