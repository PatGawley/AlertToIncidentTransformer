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
    }

    public class IncidentRepository : IIncidentRepository
    {
        private bool _isLocked = false;

        public bool AcquireLock()
        {
            _isLocked = true;
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
