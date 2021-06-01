using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using AlertToIncidentTransformer;

namespace AlertToIncidentTransformerTests
{
    public class IncidentRepositoryUnitTests
    {
        [Fact]
        public void GivenNoIncidents_WhenGetIncidents_ThenNoIcidentsReturned()
        {
            var givenNoIncidents = new IncidentRepository("UseDevelopmentStorage=true;", "givennoincidents-whengetincidents-thennoicidentsreturned");

            var whenGetIncidents = givenNoIncidents.GetIncidents().Result;

            whenGetIncidents.Count.Should().Be(0);
        }
    }
}
