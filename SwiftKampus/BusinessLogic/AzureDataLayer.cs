using Microsoft.WindowsAzure.Storage.Blob;
using SwiftKampus.Abstractions;
using SwiftKampus.Services;
using System;
using System.Web;

namespace SwiftKampus.BusinessLogic
{
    public class AzureDataLayer : IAzureDataLayer
    {
        private readonly AzureBlob _blob;
        
        public AzureDataLayer()
        {
            _blob = new AzureBlob();
        }


        public string AzureBlobUploadFile(HttpPostedFileBase file)
        {
            if (file.ContentLength > 0)
            {

                CloudBlobContainer blobContainer = _blob.GetCloudBlobContainer();
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(file.FileName);
                blob.UploadFromStream(file.InputStream);
                return "Successful";
            }
            return "UnSuccessful";
        }

        public string AzureBlobDeleteFile(string fileName)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                Uri uri = new Uri(fileName);
                string filename = System.IO.Path.GetFileName(uri.LocalPath);
                CloudBlobContainer blobContainer = _blob.GetCloudBlobContainer();
                CloudBlockBlob blob = blobContainer.GetBlockBlobReference(filename);
                blob.Delete();
                return "Successful";
            }
            return "UnSuccessful";

        }
    }
}