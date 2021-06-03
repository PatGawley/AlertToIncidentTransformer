using System;
using System.Collections.Generic;
using System.Text;

namespace AlertToIncidentTransformer
{
    public interface IIncidentAlertingService
    {
        int RaiseNewIncident(Incident incident);

        void UpdateIncident(int id, string details);
    }
    public class IncidentAlertingService : IIncidentAlertingService
    {
        public int RaiseNewIncident(Incident incident)
        {
            throw new NotImplementedException();
        }

        public void UpdateIncident(int id, string details)
        {
            throw new NotImplementedException();
        }
    }
}
