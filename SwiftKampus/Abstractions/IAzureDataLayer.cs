using System.Web;

namespace SwiftKampus.Abstractions
{
    public interface IAzureDataLayer
    {
        string AzureBlobDeleteFile(string fileName);
        string AzureBlobUploadFile(HttpPostedFileBase file);
    }
}