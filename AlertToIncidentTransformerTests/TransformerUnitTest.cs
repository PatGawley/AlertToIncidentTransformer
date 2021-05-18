using AlertToIncidentTransformer;
using System;
using Xunit;
using FluentAssertions;
using Moq;
using System.Collections.Generic;

namespace AlertToIncidentTransformerTests
{
    public class TransformerUnitTest
    {
        private readonly Mock<IIncidentManagementService> _incidentManagementService = new Mock<IIncidentManagementService>();

        [Theory]
        [InlineData("HHT", "Authentication", "Excessive POST 500s", "Integration", "HHT", "Authentication Failures", Priority.P1, "Alert Details")]
        public void GivenAFullyMappedAlert_WhenTransformed_ThenCorrectlyMappedIncidentResponse(string Product, string Component, string Fault, string CatalogItem, 
            string CmdbItem, string ResponseFault, Priority priority,
            string alertDetails)
        {
            var faultDatetime = new DateTime(2008, 5, 1, 8, 30, 52);

            var givenAFullyMappedAlert = new Alert(Product, Component, Fault, faultDatetime, alertDetails);

            var whenTransformed = new Transformer(_incidentManagementService.Object);

            var thenCorrectlyMappedIncidentResponse = whenTransformed.Transform(givenAFullyMappedAlert);

            thenCorrectlyMappedIncidentResponse.IsIncident.Should().BeTrue();
            thenCorrectlyMappedIncidentResponse.IsIncidentActivity.Should().BeFalse();
            thenCorrectlyMappedIncidentResponse.IncidentActivityResponse.Should().BeNull();
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CatalogItem.Should().Be(CatalogItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CmdbItem.Should().Be(CmdbItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Fault.Should().Be(ResponseFault);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Description.Should().Be(alertDetails);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CreatedOn.Should().Be(faultDatetime);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Priority.Should().Be(priority);
        }

        [Theory]
        [InlineData("HHT", "Catalog", "Excessive POST 500s", "Integration", "HHT", "Catalog Failures", Priority.P1, "Alert Details")]
        public void GivenAPartiallyMappedAlert_WhenTransformed_ThenCorrectlyMappedIncidentResponse(string Product, string Component, string Fault, string CatalogItem,
            string CmdbItem, string ResponseFault, Priority priority,
            string alertDetails)
        {
            var faultDatetime = new DateTime(2008, 5, 1, 8, 30, 52);

            var givenAPartiallyMappedAlert = new Alert(Product, Component, Fault, faultDatetime, alertDetails);

            var whenTransformed = new Transformer(_incidentManagementService.Object);

            var thenCorrectlyMappedIncidentResponse = whenTransformed.Transform(givenAPartiallyMappedAlert);

            thenCorrectlyMappedIncidentResponse.IsIncident.Should().BeTrue();
            thenCorrectlyMappedIncidentResponse.IsIncidentActivity.Should().BeFalse();
            thenCorrectlyMappedIncidentResponse.IncidentActivityResponse.Should().BeNull();
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CatalogItem.Should().Be(CatalogItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CmdbItem.Should().Be(CmdbItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Fault.Should().Be(ResponseFault);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Description.Should().Be(alertDetails);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CreatedOn.Should().Be(faultDatetime);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Priority.Should().Be(priority);
        }
        [Theory]
        [InlineData("NewService", "NewServiceComponent", "Excessive POST 500s", "Integration Unlisted", "Unlisted", "Unlisted", Priority.P3, "Alert Details")]
        public void GivenAnUnMappedAlert_WhenTransformed_ThenCorrectlyMappedIncidentResponse(string Product, string Component, string Fault, string CatalogItem,
            string CmdbItem, string ResponseFault, Priority priority,
            string alertDetails)
        {
            var faultDatetime = new DateTime(2008, 5, 1, 8, 30, 52);

            var givenAnUnMappedAlert = new Alert(Product, Component, Fault, faultDatetime, alertDetails);

            var whenTransformed = new Transformer(_incidentManagementService.Object);

            var thenCorrectlyMappedIncidentResponse = whenTransformed.Transform(givenAnUnMappedAlert);

            thenCorrectlyMappedIncidentResponse.IsIncident.Should().BeTrue();
            thenCorrectlyMappedIncidentResponse.IsIncidentActivity.Should().BeFalse();
            thenCorrectlyMappedIncidentResponse.IncidentActivityResponse.Should().BeNull();
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CatalogItem.Should().Be(CatalogItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CmdbItem.Should().Be(CmdbItem);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Fault.Should().Be(ResponseFault);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Description.Should().Be(alertDetails);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.CreatedOn.Should().Be(faultDatetime);
            thenCorrectlyMappedIncidentResponse.IncidentResponse.Value.Priority.Should().Be(priority);
        }

        [Theory]
        [InlineData("HHT", "Authentication", "Excessive POST 500s", "Integration", "Alert Details")]
        public void GivenAFullyMappedAlert_WhenComponentInFailureAndTransformed_ThenCorrectlyMappedIncidentActivityResponse(string Product, string Component, string Fault, string ResponseFault,
            string alertDetails)
        {
            var faultDatetime = new DateTime(2008, 5, 1, 8, 30, 52);
            
            var givenAFullyMappedAlert = new Alert(Product, Component, Fault, faultDatetime, alertDetails);
            var givenSecondFullyMappedAlert = new Alert(Product, Component, Fault, faultDatetime.AddSeconds(1), alertDetails);

            var whenTransformed = new Transformer(_incidentManagementService.Object);

            var mappedIncident = new Incident("Integration", "HHT", "Authentication Failures", Fault, faultDatetime, Priority.P1);

            _incidentManagementService.Setup(e => e.RecordIncident(It.IsAny<Incident>())).Returns((false, mappedIncident));
            _ = whenTransformed.Transform(givenAFullyMappedAlert);
            _incidentManagementService.Setup(e => e.RecordIncident(It.IsAny<Incident>())).Returns((true, mappedIncident));

            var thenCorrectlyMappedIncidentActivityResponse = whenTransformed.Transform(givenSecondFullyMappedAlert);

            thenCorrectlyMappedIncidentActivityResponse.IsIncident.Should().BeFalse();
            thenCorrectlyMappedIncidentActivityResponse.IsIncidentActivity.Should().BeTrue();
            thenCorrectlyMappedIncidentActivityResponse.IncidentActivityResponse.Should().NotBeNull();
            thenCorrectlyMappedIncidentActivityResponse.IncidentActivityResponse.Value.Details.Should().Be(alertDetails);
            thenCorrectlyMappedIncidentActivityResponse.IncidentActivityResponse.Value.Id.Should().NotBeNullOrEmpty();
            thenCorrectlyMappedIncidentActivityResponse.IncidentActivityResponse.Value.CreatedOn.Should().Be(faultDatetime.AddSeconds(1));
        }

    }

    public class TestIncidentManagementService : IIncidentManagementService
    {
        private readonly IncidentManagementServiceMode _mode;
        private int _recordIncidentCount = 0;

        public TestIncidentManagementService(IncidentManagementServiceMode mode)
        {
            _mode = mode;
        }

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
            _recordIncidentCount++;

            switch (_mode)
            {
                case IncidentManagementServiceMode.ReturnWasNotAlreadyDown:
                    return (false, new Incident());
                case IncidentManagementServiceMode.ReturnWasNotAlreadyDownAfterFirstCall:
                    if (_recordIncidentCount == 1)
                        return (false, new Incident());
                    return (true, new Incident());
                default:
                    throw new NotImplementedException();
            }

        }

        public (bool wasAlreadyDown, string Id, string details) RecordIncident(string product, string catalog, string details)
        {
            throw new NotImplementedException();
        }
    }

    public enum IncidentManagementServiceMode
    {
        ReturnWasNotAlreadyDown,
        ReturnWasNotAlreadyDownAfterFirstCall
    }
}
