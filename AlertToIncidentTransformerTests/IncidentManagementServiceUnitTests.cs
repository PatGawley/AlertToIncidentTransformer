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
        [Theory]
        [InlineData("HHT API")]
        [InlineData("Online API")]
        [InlineData("Catalog API")]
        [InlineData("Tlogs Inbound")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncidentForSameCmdb_ThenAlreadyDownIsTrue(string cmdbItem)
        {
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            var whenRecordIncidentForSameCmdb = TestIncident(cmdbItem);
            var thenAlreadyDownIsTrue = incidentManagementService.RecordIncident(whenRecordIncidentForSameCmdb);

            thenAlreadyDownIsTrue.wasAlreadyDown.Should().BeTrue();
        }

        [Theory]
        [InlineData("HHT API")]
        [InlineData("Online API")]
        [InlineData("Catalog API")]
        [InlineData("Tlogs Inbound")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncidentForSameCmdb_ThenOriginalIncidentIsReturned(string cmdbItem)
        {
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            var whenRecordIncidentForSameCmdb = TestIncident(cmdbItem);
            var thenOriginalIncidentIsReturned = incidentManagementService.RecordIncident(whenRecordIncidentForSameCmdb);

            thenOriginalIncidentIsReturned.incident.Id.Should().Be(activeIncident.Id);
        }

        [Theory]
        [InlineData("HHT API")]
        [InlineData("Online API")]
        [InlineData("Catalog API")]
        [InlineData("Tlogs Inbound")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncidentAndThenCloseIncident_ThenIncidentNoLongerExists(string cmdbItem)
        {
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            incidentManagementService.CloseIncident(activeIncident.Id);

            var thenIncidentNoLongerExists = incidentManagementService.ActiveIncidents();

            thenIncidentNoLongerExists.Any(i => i.Id == activeIncident.Id).Should().BeFalse();
        }

        [Theory]
        [InlineData("HHT API")]
        [InlineData("Online API")]
        [InlineData("Catalog API")]
        [InlineData("Tlogs Inbound")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncident_ThenIncidentExistsAsActiveIncident(string cmdbItem)
        {
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            var thenIncidentExistsAsActiveIncident = incidentManagementService.ActiveIncidents();

            thenIncidentExistsAsActiveIncident.Any(i => i.Id == activeIncident.Id).Should().BeTrue();
        }

        [Theory]
        [InlineData("HHT API","Online API")]
        [InlineData("Online API", "HHT API")]
        [InlineData("Catalog API", "TLogs Inbound")]
        [InlineData("Tlogs Inbound", "Catalog API")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncidentForDifferentCmdb_ThenAlreadyDownIsFalse(string cmdbItem, string cmdbItem2)
        {
            
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            var whenRecordIncidentForDifferentCmdb = TestIncident(cmdbItem2);
            var thenAlreadyDownIsFalse = incidentManagementService.RecordIncident(whenRecordIncidentForDifferentCmdb);

            thenAlreadyDownIsFalse.wasAlreadyDown.Should().BeFalse();
        }

        [Theory]
        [InlineData("HHT API", "Online API")]
        [InlineData("Online API", "HHT API")]
        [InlineData("Catalog API", "TLogs Inbound")]
        [InlineData("Tlogs Inbound", "Catalog API")]
        public void GivenActiveIncidentForCmdbItem_WhenRecordIncidentForDifferentCmdb_ThenOriginalIncidentIsNotReturned(string cmdbItem, string cmdbItem2)
        {
            var activeIncident = TestIncident(cmdbItem);
            var incidentManagementService = NoActiveIncidents();
            _ = incidentManagementService.RecordIncident(activeIncident);

            var whenRecordIncidentForDifferentCmdb = TestIncident(cmdbItem2);
            var thenOriginalIncidentIsNotReturned = incidentManagementService.RecordIncident(whenRecordIncidentForDifferentCmdb);

            thenOriginalIncidentIsNotReturned.incident.Id.Should().NotBe(activeIncident.Id);
        }
        private IncidentManagementService NoActiveIncidents()
        {
            var incidentManagementService = StartupIncidentManagementService(new List<Incident>());

            _incidentRepository.Setup(r => r.PostIncidents(It.IsAny<List<Incident>>())).Returns(true);


            foreach (var incident in incidentManagementService.ActiveIncidents())
            {
                incidentManagementService.CloseIncident(incident.Id);
            }
            return incidentManagementService;
        }

        private IncidentManagementService StartupIncidentManagementService(List<Incident> baseLineIncidents)
        {
            _incidentRepository.Setup(r => r.AcquireLock()).Returns(true);
            _incidentRepository.Setup(r => r.GetIncidents().Result).Returns(baseLineIncidents);
            var incidentManagementService = new IncidentManagementService(_incidentRepository.Object);
            incidentManagementService.Initialise();

            return incidentManagementService;
        }
        

        private Incident TestIncident(string cmdbItem = "Cmdb Item")
        {
            return new Incident(catalogItem: "Catalog Item",
               cmdbItem: cmdbItem,
               fault: "It's bolloxed",
               description: "Totally bolloxed",
               createdOn: DateTime.Parse("2021-05-18"),
               priority: Priority.P2);
        }
    }
}
