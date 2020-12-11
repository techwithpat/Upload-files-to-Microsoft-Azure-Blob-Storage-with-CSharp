using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;

namespace AzureBlobStorage
{
    class MainClass
    {
        static void Main(string[] args)
        {
            var config = GetConfiguration();

            var files = GetFiles(config["AzureStorage:SourceFolder"]);                          
            if (!files.Any())
            {
                Console.WriteLine("Nothing to process");
                return;
            }

            UploadFiles(files, config["AzureStorage:ConnectionString"], config["AzureStorage:Container"]);
        }

        static IConfigurationRoot GetConfiguration()
            => new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json")
                .Build();

        static IEnumerable<FileInfo> GetFiles(string sourceFolder)
            => new DirectoryInfo(sourceFolder)
                .GetFiles()
                .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        
        static void UploadFiles(
            IEnumerable<FileInfo> files,
            string connectionString,
            string container)
        {            
            var containerClient = new BlobContainerClient(connectionString, container);

            Console.WriteLine("Uploading files to blob storage");

            foreach (var file in files)
            {
                try
                {
                    var blobClient = containerClient.GetBlobClient(file.Name);
                    using (var fileStream = File.OpenRead(file.FullName))
                    {
                        blobClient.Upload(fileStream);
                    }

                    Console.WriteLine($"{file.Name} uploaded");

                    File.Delete(file.FullName);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }        
    }
}
