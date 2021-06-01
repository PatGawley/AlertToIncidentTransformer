using Azure.Storage.Blobs;
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
        private bool _isLocked = false;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _containerClient;
        private readonly BlobClient _blobClient;
        private readonly BlobLeaseClient _blobLeaseClient;


        public IncidentRepository(string storageConnectionString, string incidentContainer = "incidents", string incidentFile = "currentIncidents.json")
        {
            _blobServiceClient = new BlobServiceClient(storageConnectionString);
            _containerClient = _blobServiceClient.GetBlobContainerClient(incidentContainer);
            _blobClient = _containerClient.GetBlobClient(incidentFile);
            _blobLeaseClient = _blobClient.GetBlobLeaseClient();

        }

        public bool AcquireLock()
        {
            _blobLeaseClient.Acquire(TimeSpan.FromSeconds(1));
            _isLocked = true;
            return true;
        }

        public bool ReleaseLock()
        {
            _blobLeaseClient.Release();
            _isLocked = false;
            return true;
        }

        public async Task<List<Incident>> GetIncidents()
        {
            var incidents = new List<Incident>();

            if (await _containerClient.ExistsAsync())
            {
                if (await _blobClient.ExistsAsync())
                {

                    var response = await _blobClient.DownloadAsync();

                    incidents = await JsonSerializer.DeserializeAsync<List<Incident>>(response.Value.Content);

                }
            }

            return incidents;
        }


        public bool IsLocked() => _isLocked;

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
