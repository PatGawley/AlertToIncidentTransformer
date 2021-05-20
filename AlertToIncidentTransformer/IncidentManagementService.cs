using System;
using System.Collections.Generic;
using System.Linq;

namespace AlertToIncidentTransformer
{
    public interface IIncidentManagementService
    {
        (bool wasAlreadyDown, Incident incident) RecordIncident(Incident incident);
        List<Incident> ActiveIncidents();
        bool CloseIncident(string Id);
        bool Initialise();

    }
    public class IncidentManagementService : IIncidentManagementService
    {
        private readonly IIncidentRepository _incidentRepository;
        private List<Incident> _incidents;
        private bool _initialised = false;

        public List<Incident> ActiveIncidents() => _incidents;

        public IncidentManagementService(IIncidentRepository incidentRepository)
        {
            _incidents = new List<Incident>();
            _incidentRepository = incidentRepository;
        }

        private void CheckState()
        {
            if (!_initialised) throw new InvalidOperationException("Incident Management Service Is Not Initialised");
        }
        private bool Persist()
        {
            return _incidentRepository.PostIncidents(_incidents);
        }

        public bool Initialise()
        {

            if (_incidentRepository.AcquireLock())
            {
                _incidents = _incidentRepository.GetIncidents();
                _initialised = true;
            }

            return _initialised;
        }

        public bool CloseIncident(string Id)
        {
            CheckState();

            var isRemoved = 
                _incidents.RemoveAll(i => i.Id == Id) == 1;

            if (!isRemoved)
                return false;

            var isPersisted = 
                Persist();

            return isPersisted;
        }

        public (bool wasAlreadyDown, Incident incident) RecordIncident(Incident incident)
        {
            CheckState();
            return (false, incident);
        }
    }

    public readonly struct Incident
    {
        public Incident(string catalogItem,
            string cmdbItem, string fault, string description, DateTime createdOn,
            Priority priority)
        {
            CatalogItem = catalogItem;
            CmdbItem = cmdbItem;
            Fault = fault;
            Description = description;
            CreatedOn = createdOn;
            Priority = priority;
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; }
        public string CatalogItem { get; }
        public string CmdbItem { get; }
        public string Fault { get; }
        public string Description { get; }
        public DateTime CreatedOn { get; }
        public Priority Priority { get; }
    }
}
