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
            List<FileManagerDirectoryContent> FileData = new List<FileManagerDirectoryContent>();
            FileData = this.operation.GetFile("/", false, null);

            List<string> SubFolders = this.operation.initialResponse.CommonPrefixes;
            for (int i = 0; i < SubFolders.Count; i++)
            {
                string commonPrefix = SubFolders[i];
                var index = commonPrefix.IndexOf('/');
                var path = commonPrefix.Substring(index);
                FileData.AddRange(this.operation.GetFile(path, false, null));

                // Get the additional files and add them to the end of the list
                List<string> NestedFiles = this.operation.initialResponse.CommonPrefixes;
                SubFolders.AddRange(NestedFiles);
            }
            return FileData;
        }
    }
}
