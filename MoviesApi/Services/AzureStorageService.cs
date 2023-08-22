﻿using Microsoft.Extensions.Configuration;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MoviesApi.Services
{
    public class AzureStorageService : IFileStorageService
    {
        private readonly string connectionString;

        public AzureStorageService(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("AzureStorageConnection");
        }

        public async Task DeleteFile(string fileRoute, string containerName)
        {
            if (fileRoute != null)
            {
                var account = CloudStorageAccount.Parse(connectionString);
                var client = account.CreateCloudBlobClient();
                var container = client.GetContainerReference(containerName);

                var blobName = Path.GetFileName(fileRoute);
                var blob = container.GetBlobReference(blobName);
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task<string> EditFile(byte[] content, string extension, string containerName, string fileRoute, string contentType)
        {
            await DeleteFile(fileRoute, containerName);
            return await SaveFile(content, extension, containerName, contentType);
        }

        public async Task<string> SaveFile(byte[] content, string extension, string containerName, string contentType)
        {
            //retrieve a storage account
            var account = CloudStorageAccount.Parse(connectionString);
            //Provides a client-side logical representation of Microsoft Azure Blob storage.
            var client = account.CreateCloudBlobClient();
            //gets a refrence for container in azure via a container name
            var container = client.GetContainerReference(containerName);
            //create container if not exist
            await container.CreateIfNotExistsAsync();
            //set permissions to anonymous to read  Blob only not the rest of container data 
            await container.SetPermissionsAsync(new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
            var filename = $"{Guid.NewGuid()}.{extension}";
            var blob = container.GetBlockBlobReference(filename);
            await blob.UploadFromByteArrayAsync(content, 0, content.Length);
            blob.Properties.ContentType = contentType;
            return blob.Uri.ToString();
        }

    }
}
