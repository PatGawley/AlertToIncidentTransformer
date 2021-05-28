using System;
using System.Collections.Generic;
using System.Text;

namespace AlertToIncidentTransformer
{
    public interface IIncidentRepository
    {
        List<Incident> GetIncidents();
        bool AcquireLock();
        bool IsLocked();
        bool PostIncidents(List<Incident> incidents);
        bool ReleaseLock();
    }

    public class IncidentRepository : IIncidentRepository
    {
        private bool _isLocked = false;
        private readonly string _storageConnectionString, _incidentContainer;

        public IncidentRepository(string storageConnectionString, string incidentContainer = "incidents")
        {
            _storageConnectionString = storageConnectionString;
            _incidentContainer = incidentContainer;
        }

        public bool AcquireLock()
        {
            _isLocked = true;
            return true;
        }

        public bool ReleaseLock()
        {
            _isLocked = false;
            return true;
        }

        public List<Incident> GetIncidents()
        {
            return new List<Incident>();
        }

        public bool IsLocked() => _isLocked;

        public bool PostIncidents(List<Incident> incidents) => _isLocked;
    }
}
