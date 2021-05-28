using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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
            var closeIncident = 
                new CloseIncidentAction(Id, _incidents);

            return ActionAndPersist(closeIncident);

        }

        private interface IAction
        {
            bool Invoke();
        }

        private class CloseIncidentAction : IAction
        {
            private readonly string _id;
            private readonly List<Incident> _incidents;

            public CloseIncidentAction(string id, List<Incident> incidents)
            {
                _id = id;
                _incidents = incidents;

            }
            public bool Invoke()
            {
                return
                    _incidents.RemoveAll(i => i.Id == _id) == 1;
            }
        }

        private class RecordIncidentAction : IAction
        {
            private readonly List<Incident> _incidents;
            private readonly Incident _incident;

            public RecordIncidentAction(Incident incident, List<Incident> incidents)
            {
                _incidents = incidents;
                _incident = incident;
            }

            public bool Invoke()
            {
                Expression<Func<Incident, bool>> isAlreadyDown = i => i.CmdbItem == _incident.CmdbItem;
                var matchByCmdbItem = isAlreadyDown.Compile();

                if (_incidents.Any(matchByCmdbItem))
                {
                    return false;
                }

                _incidents.Add(_incident);

                return true;

            }
        }

        private bool ActionAndPersist(IAction action)
        {
            var success = false;

            CheckState();
            try
            {
                if (_incidentRepository.AcquireLock())
                {
                    var isRemoved =
                    action.Invoke();

                    if (isRemoved)
                    {
                        success =
                            Persist();
                    }
                }

            }
            catch (Exception)
            {
                _incidentRepository.ReleaseLock();
                throw;
            }

            _incidentRepository.ReleaseLock();
            return success;
        }

        public (bool wasAlreadyDown, Incident incident) RecordIncident(Incident incident)
        {
            var recordIncidentAction = 
                new RecordIncidentAction(incident, _incidents);

            var wasAlreadyDown = 
                !ActionAndPersist(recordIncidentAction);

            Incident affectedIncident;

            if (wasAlreadyDown)
            {
                _incidents = _incidentRepository.GetIncidents();
                affectedIncident = _incidents.First(i => i.CmdbItem == incident.CmdbItem);
            }
            else
            {
                affectedIncident = incident;
            }

            return (wasAlreadyDown, affectedIncident);

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
