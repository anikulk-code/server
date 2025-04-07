﻿using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace ChatAPI
{
    public class BlobAPI
    {
        ILogger<BlobAPI> _logger;
        const string blobUrl = "https://chatfilesani.blob.core.windows.net/";
        const string containerName = "common";
        string accountName = "chatfilesani";
        BlobContainerClient containerClient;
        public BlobAPI(ILogger<BlobAPI> logger)
        {
            _logger = logger;
            var connectionString = Environment.GetEnvironmentVariable("BLOB_CONNECTION_STRING");
            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            containerClient.CreateIfNotExists(PublicAccessType.None);
            this.containerClient = new BlobContainerClient(connectionString, containerName);
        }
        public async Task<string> UploadFileAsync(Stream stream, string name)
        {
            var blobClient = containerClient.GetBlobClient(name);

            //using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, overwrite: true);

            return blobClient.Uri.ToString();
        }

        public async Task<bool> DeleteFileAsync(string fileName)
        {
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.DeleteIfExistsAsync();
        }
    }
}
