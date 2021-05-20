using AlertToIncidentTransformer;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using System.Linq;
using Moq;

//https://hamidmosalla.com/2018/08/16/xunit-control-the-test-execution-order/
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace AlertToIncidentTransformerTests
{
    public class IncidentManagementServiceUnitTests
    {
        private readonly Mock<IIncidentRepository> _incidentRepository = new Mock<IIncidentRepository>();

        [Fact]
        public void GivenNoActiveIncidents_WhenRecordIncident_ThenAlreadyDownIsFalse()
        {
            var givenNoActiveIncidents = NoActiveIncidents();

            var whenRecordIncident = givenNoActiveIncidents.RecordIncident(TestIncident());

            var thenAlreadyDownIsFalse = whenRecordIncident.wasAlreadyDown;

            thenAlreadyDownIsFalse.Should().BeFalse();

        }
        [Fact]
        public void GivenAnActiveIncident_WhenClassReinstantiated_ThenIncidentIsPersisted()
        {
            var activeIncident = TestIncident();
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            incidentManagementService = StartupIncidentManagementService(new List<Incident>() { activeIncident });

            var thenIncidentIsPersisted = incidentManagementService.ActiveIncidents().First(i => i.Id == activeIncident.Id);

            thenIncidentIsPersisted.Should().NotBeNull();

        }

        private IncidentManagementService NoActiveIncidents()
        {
            var incidentManagementService = StartupIncidentManagementService(new List<Incident>());

            foreach(var incident in incidentManagementService.ActiveIncidents())
            {
                incidentManagementService.CloseIncident(incident.Id);
            }
            return incidentManagementService;
        }

        private IncidentManagementService StartupIncidentManagementService(List<Incident> baseLineIncidents)
        {
            _incidentRepository.Setup(r => r.AcquireLock()).Returns(true);
            _incidentRepository.Setup(r => r.GetIncidents()).Returns(baseLineIncidents);
            var incidentManagementService = new IncidentManagementService(_incidentRepository.Object);
            incidentManagementService.Initialise();

            return incidentManagementService;
        }
        

        private Incident TestIncident()
        {
            return new Incident(catalogItem: "Catalog Item",
               cmdbItem: "Cmdb Item",
               fault: "It's bolloxed",
               description: "Totally bolloxed",
               createdOn: DateTime.Parse("2021-05-18"),
               priority: Priority.P2);
        }
    }
}
