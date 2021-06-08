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

        public void GivenNoIncidents_WhenPostXIncidents_ThenXIncidentsReuturned()
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
            var givenNoIncidents = new IncidentRepository("UseDevelopmentStorage=true;", "givennoincidents-whenpostxincidents");

            Fixture fixture = new Fixture();

            for (int x = 1; x <= 100; x++)
            {
                var incidents = fixture.CreateMany<Incident>(x).ToList();

                var whenPostXIncidents = givenNoIncidents.PostIncidents(incidents).Result;

                var thenXIncidentsReturnedWithCorrectDetails = givenNoIncidents.GetIncidents().Result;

                thenXIncidentsReturnedWithCorrectDetails.Should().BeEquivalentTo(incidents);


            }

        }
        [Fact]
        public void GivenNotIsLocked_WhenAcquireLock_ThenIsLocked()
        {
            var givenNotIsLocked = new IncidentRepository("UseDevelopmentStorage=true;", "givennotislocked-whenacquirelock-thenislocked");

            var whenAcquireLock = givenNotIsLocked.AcquireLock();

            whenAcquireLock.Should().BeTrue();

            var thenIsLocked = givenNotIsLocked.IsLocked();

            thenIsLocked.Should().BeTrue();

        }
        [Fact]
        public void GivenNewIncidentRepository_WhenChecked_ThenIsNotLocked()
        {
            var givenNewIncidentRepository = new IncidentRepository("UseDevelopmentStorage=true;", "givennewincidentrepository-whenchecked-thenisnotlocked");

            var whenChecked = givenNewIncidentRepository.IsLocked();

            var thenIsNotLocked = !whenChecked;

            thenIsNotLocked.Should().BeTrue();

        }
        [Fact]
        public void GivenNewIncidentRepository_WhenLockAcquiredAndLockReleased_ThenIsNotLocked()
        {
            var givenNewIncidentRepository = new IncidentRepository("UseDevelopmentStorage=true;", "givennewincidentrepository-whenlockacquiredandlockreleased");

            var whenLockAcquiredAndLockReleased = givenNewIncidentRepository.AcquireLock();
            whenLockAcquiredAndLockReleased = givenNewIncidentRepository.ReleaseLock();

            var thenIsNotLocked = !givenNewIncidentRepository.IsLocked();

            thenIsNotLocked.Should().BeTrue();

        }
        [Fact]
        public void GivenAcquiredLock_WhenPostIncidents_ThenFalseReturned()
        {
            var givenAcquiredLock = new IncidentRepository("UseDevelopmentStorage=true;", "givenacquiredlock-whenpostincidents-thenfalseisreturned");
            givenAcquiredLock.AcquireLock();

            var whenPostIncidents = givenAcquiredLock.PostIncidents(new List<Incident>()).Result;

            var thenFalseReturned = (whenPostIncidents == false);

            thenFalseReturned.Should().BeTrue();
        }

        [Fact]
        public void GivenAcquiredLockThenReleased_WhenPostIncidents_ThenTrueReturned()
        {
            var givenAcquiredLockThenReleased = new IncidentRepository("UseDevelopmentStorage=true;", "givenacquiredlockthenreleased-whenpostincidents");
            givenAcquiredLockThenReleased.AcquireLock();
            givenAcquiredLockThenReleased.ReleaseLock();

            var whenPostIncidents = givenAcquiredLockThenReleased.PostIncidents(new List<Incident>()).Result;

            var thenTrueReturned = (whenPostIncidents == true);

            thenTrueReturned.Should().BeTrue();
        }
    }
}
