using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AlertToIncidentTransformer
{
    public interface IIncidentRepository
    {
        Task<List<Incident>> GetIncidents();
        bool AcquireLock();
        bool IsLocked();
        Task<bool> PostIncidents(List<Incident> incidents);
        bool ReleaseLock();
    }

    public class IncidentRepository : IIncidentRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly BlobClient _blobClient;
        private readonly BlobLeaseClient _blobLeaseClient;
        private readonly TimeSpan _leaseTime;
        private BlobLease _lease;


        public IncidentRepository(string storageConnectionString, string incidentContainer = "incidents", string incidentFile = "currentIncidents.json", int leaseTimeInSeconds = 15)
        {
            _blobServiceClient = new BlobServiceClient(storageConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(incidentContainer);
            _blobClient = _containerClient.GetBlobClient(incidentFile);
            _blobLeaseClient = _blobClient.GetBlobLeaseClient();
            _leaseTime = TimeSpan.FromSeconds(leaseTimeInSeconds);
        }

        public bool AcquireLock()
        {
            if (!_containerClient.Exists())
            {
                _containerClient.Create();   
            }
            if (!_blobClient.Exists())
            {
                var emptyStream = new MemoryStream();
                _blobClient.Upload(emptyStream);
            }
            _lease = _blobLeaseClient.Acquire(_leaseTime);
            return true;
        }

        private void CreateEmptyIncidentsFileIfOneDoesNotExist()
        {
            if (!_containerClient.Exists())
            {
                _containerClient.Create();
            }
            if (!_blobClient.Exists())
            {
                var emptyStream = new MemoryStream();
                _blobClient.Upload(emptyStream);
            }
        }
        public bool ReleaseLock()
        {
            _blobLeaseClient.Release();
            _lease = null;
            return IsLocked();
        }

        public async Task<List<Incident>> GetIncidents()
        {
            var incidents = new List<Incident>();

            if (await _containerClient.ExistsAsync())
            {
                if (await _blobClient.ExistsAsync())
                {
                    Stream fileContents = new MemoryStream();
                    
                    var response = await _blobClient.DownloadToAsync(fileContents);

                    
                    fileContents.Position = 0;
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    incidents = await JsonSerializer.DeserializeAsync<List<Incident>>(fileContents, options);

                }
            }

            return incidents;
        }


        public bool IsLocked()
        {
            try
            {
                CreateEmptyIncidentsFileIfOneDoesNotExist();
                _blobClient.SetTags(new Dictionary<string, string>() { { "LeaseCheck", "1" } });
            }
            catch (Exception ex)
            {

                return true;
            }
            
            return false;
        }

        public async Task<bool> PostIncidents(List<Incident> incidents)
        {
            try
            {
                if (!await _containerClient.ExistsAsync())
                {
                    await _containerClient.CreateAsync();
                }

                using (Stream incidentsToUpload = new MemoryStream())
                {

                    await JsonSerializer.SerializeAsync(incidentsToUpload, incidents);
                    incidentsToUpload.Position = 0;
                    var response = await _blobClient.UploadAsync(incidentsToUpload, true);

                    return true;
                }
            }
            catch (Exception)
            {
                return false;
                //TODO: record exceptions here
            }
         
        }
    }
}
