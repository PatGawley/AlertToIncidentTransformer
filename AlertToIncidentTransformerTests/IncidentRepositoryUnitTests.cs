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
            var givenNoIncidents = new IncidentRepository("UseDevelopmentStorage=true;", "GivenNoIncidents_WhenGetIncidents_ThenNoIcidentsReturned");

            var whenGetIncidents = givenNoIncidents.GetIncidents().Result;


        }
    }
}
