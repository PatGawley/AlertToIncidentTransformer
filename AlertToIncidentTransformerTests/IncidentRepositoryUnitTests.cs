using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using FluentAssertions;
using AlertToIncidentTransformer;
using AutoFixture;
using System.Linq;

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

        [Fact]

        public void GivenNoIncidents_WhenPostXIncidents_ThenXIncidentsReuturned ()
        {
            var givenNoIncidents = new IncidentRepository("UseDevelopmentStorage=true;", "givennoincidents-whenpostxincidents-thenxincidentsreturned");
            
            Fixture fixture = new Fixture();

            for (int x = 1; x <= 100; x++)
            {
                var incidents = fixture.CreateMany<Incident>(x).ToList();

                var whenPostXIncidents = givenNoIncidents.PostIncidents(incidents).Result;

                var thenXIncidentsReturned = givenNoIncidents.GetIncidents().Result;

                thenXIncidentsReturned.Count.Should().Be(x);

            }
            
        }

        [Fact]

        public void GivenNoIncidents_WhenPostXIncidents_ThenXIncidentsReturnedWithCorrectDetails()
        {
            var givenNoIncidents = new IncidentRepository("UseDevelopmentStorage=true;", "givennoincidents-whenpostxincidents-thenxincidentsreturnedwithcorrectdetails");

            Fixture fixture = new Fixture();

            for (int x = 1; x <= 100; x++)
            {
                var incidents = fixture.CreateMany<Incident>(x).ToList();

                var whenPostXIncidents = givenNoIncidents.PostIncidents(incidents).Result;

                var thenXIncidentsReturnedWithCorrectDetails = givenNoIncidents.GetIncidents().Result;

                thenXIncidentsReturnedWithCorrectDetails.Should().BeEquivalentTo(incidents);


            }

        }
    }
}
