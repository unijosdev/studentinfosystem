using SwiftKampus.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

public class FileStorage 
{
    readonly string _path;

    /// Save file implementation for local file storage.
    /// It returns a SaveUpdateStorageVm model as result
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public StorageResult SaveFile(HttpPostedFileBase file)
    {
        var result = new StorageResult();
        if (CheckFile(file))
        {
            try
            {
                var extension = "." + file.FileName.Split('.')[file.FileName.Split('.').Length - 1];
                var fileName = Guid.NewGuid().ToString() + extension;

                string targetFolder = HttpContext.Current.Server.MapPath("~/UploadedFiles/");
                string targetPath = Path.Combine(targetFolder, fileName);
                file.SaveAs(targetPath);
                result.Status = true;
                result.Message = "Successful";
                result.FullPath = fileName;
                result.FileName = fileName;
            }
            catch (Exception e)
            {
                result.Message = e.Message;
                result.Status = false;
            }
        }
        return result;
    }

    /// <summary>
    /// Check if a file is not null by returning true or false.
    /// </summary>
    /// <param name="file"></param>
    /// <returns></returns>
    public bool CheckFile(HttpPostedFileBase file)
    {
        if (file != null && file.ContentLength > 0)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Delete file implementation for both local file storage.
    /// It returns true if operation is successful
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public void DeleteFile(string fileName)
    {
        string targetFolder = HttpContext.Current.Server.MapPath("~/UploadedFiles/");
        fileName = Path.Combine(targetFolder, fileName);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    /// <summary>
    /// Update file implementation for both local file storage.
    /// It returns a SaveUpdateStorageVm model as result
    /// </summary>
    /// <param name="file"></param>
    /// <param name="fullPath"></param>
    /// <returns></returns>
    public StorageResult UpdateFile(HttpPostedFileBase file, string fullPath)
    {
        DeleteFile(fullPath);
        var result = SaveFile(file);
        return result;

    }


}