using Syncfusion.EJ2.FileManager.AmazonS3FileProvider;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Syncfusion.EJ2.FileManager.Base;
using Amazon;

namespace EJ2AmazonS3ASPCoreFileProvider.Controllers
{

    [Route("api/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class AmazonS3ProviderController : Controller
    {
        public AmazonS3FileProvider operation;
        public string basePath;
        protected RegionEndpoint bucketRegion;
        public AmazonS3ProviderController(IWebHostEnvironment hostingEnvironment)
        {
            this.basePath = hostingEnvironment.ContentRootPath;
            this.basePath = basePath.Replace("../", "");
            this.operation = new AmazonS3FileProvider();
            this.operation.RegisterAmazonS3("syncfusion-filemanager", "AKIAWH6GYCX3QHD3VSEP", "DL/HmwoO3NHjatmugOLneok9I3XU05ZpnJ7p+FkW", "us-east-1");
        }
        [Route("AmazonS3FileOperations")]
        public object AmazonS3FileOperations([FromBody] FileManagerDirectoryContent args)
        {
            switch ("read")
            {
                case "read":
                    // reads the file(s) or folder(s) from the given path.
                    List<FileManagerDirectoryContent> myList = new List<FileManagerDirectoryContent>();
                    myList = this.operation.GetFile("/", false, null);
                    List<string> files = this.operation.initialResponse.CommonPrefixes;
                    //foreach (string commonPrefix in files)
                    //{
                    //    var index = commonPrefix.IndexOf('/');
                    //    var path = commonPrefix.Substring(index);
                    //    myList.AddRange(this.operation.GetFile(path, false, null));
                    //    files.AddRange(this.operation.initialResponse.CommonPrefixes);
                    //}
                    for (int i = 0; i < files.Count; i++)
                    {
                        string commonPrefix = files[i];
                        var index = commonPrefix.IndexOf('/');
                        var path = commonPrefix.Substring(index);
                        myList.AddRange(this.operation.GetFile(path, false, null));

                        // Get the additional files and add them to the end of the list
                        List<string> additionalFiles = this.operation.initialResponse.CommonPrefixes;
                        files.AddRange(additionalFiles);
                    }
                    return myList;
                case "delete":
                    // deletes the selected file(s) or folder(s) from the given path.
                    return this.operation.ToCamelCase(this.operation.Delete(args.Path, args.Names, args.Data));
                case "copy":
                    // copies the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    return this.operation.ToCamelCase(this.operation.Copy(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
                case "move":
                    // cuts the selected file(s) or folder(s) from a path and then pastes them into a given target path.
                    return this.operation.ToCamelCase(this.operation.Move(args.Path, args.TargetPath, args.Names, args.RenameFiles, args.TargetData, args.Data));
                case "details":
                    // gets the details of the selected file(s) or folder(s).
                    return this.operation.ToCamelCase(this.operation.Details(args.Path, args.Names, args.Data));
                case "create":
                    // creates a new folder in a given path.
                    return this.operation.ToCamelCase(this.operation.Create(args.Path, args.Name, args.Data));
                case "search":
                    // gets the list of file(s) or folder(s) from a given path based on the searched key string.
                    return this.operation.ToCamelCase(this.operation.Search(args.Path, args.SearchString, args.ShowHiddenItems, args.CaseSensitive, args.Data));
                case "rename":
                    // renames a file or folder.
                    return this.operation.ToCamelCase(this.operation.Rename(args.Path, args.Name, args.NewName, false, args.ShowFileExtension, args.Data));
            }
            return null;
        }

        // uploads the file(s) into a specified path
        [Route("AmazonS3Upload")]
        public IActionResult AmazonS3Upload(string path, IList<IFormFile> uploadFiles, string action, string data)
        {
            FileManagerResponse uploadResponse;
            FileManagerDirectoryContent[] dataObject = new FileManagerDirectoryContent[1];
            dataObject[0] = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(data);
            foreach (var file in uploadFiles)
            {
                var folders = (file.FileName).Split('/');
                // checking the folder upload
                if (folders.Length > 1)
                {
                    for (var i = 0; i < folders.Length - 1; i++)
                    {
                        if (!this.operation.checkFileExist(path, folders[i]))
                        {
                            this.operation.ToCamelCase(this.operation.Create(path, folders[i], dataObject));
                        }
                        path += folders[i] + "/";
                    }
                }
            }
            uploadResponse = operation.Upload(path, uploadFiles, action, dataObject);
            if (uploadResponse.Error != null)
            {
                Response.Clear();
                Response.ContentType = "application/json; charset=utf-8";
                Response.StatusCode = Convert.ToInt32(uploadResponse.Error.Code);
                Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = uploadResponse.Error.Message;
            }
            return Content("");
        }

        // downloads the selected file(s) and folder(s)
        [Route("AmazonS3Download")]
        public IActionResult AmazonS3Download(string downloadInput)
        {
            FileManagerDirectoryContent args = JsonConvert.DeserializeObject<FileManagerDirectoryContent>(downloadInput);
            return operation.Download(args.Path, args.Names);
        }

        // gets the image(s) from the given path
        [Route("AmazonS3GetImage")]
        public IActionResult AmazonS3GetImage(FileManagerDirectoryContent args)
        {
            return operation.GetImage(args.Path, args.Id, false, null, args.Data);
        }
    }

}
