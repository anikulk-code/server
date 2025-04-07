using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace ChatAPI
{
    public class FilesAPIs
    {
        private readonly ILogger<FilesAPIs> _logger;
        private readonly BlobAPI blobAPI;

        public FilesAPIs(ILogger<FilesAPIs> logger, BlobAPI blobAPI)
        {
            _logger = logger;
            this.blobAPI = blobAPI;
        }

        [Function("UploadFile")]
        public async Task<HttpResponseData> UoloadFile(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            var contentType = req.Headers.GetValues("Content-Type").FirstOrDefault();
            var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value;

            var reader = new MultipartReader(boundary, req.Body);
            string response;
            var section = await reader.ReadNextSectionAsync();

            if (section is null || !ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var disposition))
            {
                var error = req.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteStringAsync("Invalid form-data");
                return error;
            }

            var fileName = HeaderUtilities.RemoveQuotes(disposition.FileName).Value;
            if (string.IsNullOrEmpty(fileName) || string.IsNullOrWhiteSpace(fileName))
            {
                var error = req.CreateResponse(HttpStatusCode.BadRequest);
                await error.WriteStringAsync("File name is missing");
                return error;
            }


            response = await blobAPI.UploadFileAsync(section.Body, fileName);

            var successResponse = req.CreateResponse(HttpStatusCode.OK);
            await successResponse.WriteStringAsync(response ?? "");
            return successResponse;
        }

        [Function("DeleteFile")]
        public async Task<HttpResponseData> DeleteFile(
       [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "DeleteFile/{fileName}")] HttpRequestData req,
       string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                var bad = req.CreateResponse(HttpStatusCode.BadRequest);
                await bad.WriteStringAsync("Filename is required.");
                return bad;
            }

            bool deleted = await blobAPI.DeleteFileAsync(fileName);

            var response = req.CreateResponse(deleted ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            await response.WriteStringAsync(deleted ? "Deleted" : "File not found");
            return response;
        }


        [Function("ListFiles")]
        public async Task<HttpResponseData> ListFiles(
       [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
       string fileName)
        {
            var blobNames = await blobAPI.ListFileNames();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(blobNames);
            return response;
        }
    }
}
