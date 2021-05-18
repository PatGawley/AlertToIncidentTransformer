using System;
using System.Collections.Generic;
using System.Text;

namespace AlertToIncidentTransformer
{
    public interface IIncidentManagementService
    {
        (bool wasAlreadyDown, Incident incident) RecordIncident(Incident incident);
        List<Incident> ActiveIncidents();
        bool CloseIncident(string Id);

    }
    public class IncidentManagementService : IIncidentManagementService
    {
        public List<Incident> ActiveIncidents()
        {
            throw new NotImplementedException();
        }

        public bool CloseIncident(string Id)
        {
            throw new NotImplementedException();
        }

        public (bool wasAlreadyDown, Incident incident) RecordIncident(Incident incident)
        {
            throw new NotImplementedException();
        }
    }
}
