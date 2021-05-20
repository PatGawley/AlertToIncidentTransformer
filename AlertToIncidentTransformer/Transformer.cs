using System;
using System.Collections.Generic;
using System.Text;

namespace AlertToIncidentTransformer
{
    public interface ITransformer
    {
        AlertResponse Transform(Alert alert);
    }
    public class Transformer : ITransformer
    {
        private readonly IIncidentManagementService _incidentManagementService;

        private readonly Dictionary<(string Product, string Component, string Fault), (string CatalogItem, string CmdbItem, string Fault, Priority Priority)> _fullMapAlertToIncident
            = new Dictionary<(string Product, string Component, string Fault), (string CatalogItem, string CmdbItem, string Fault, Priority Priority)>();

        private readonly Dictionary<(string Product, string Component), (string CatalogItem, string CmdbItem, string Fault, Priority Priority)> _mapComponentToIncident 
            = new Dictionary<(string Product, string Component), (string CatalogItem, string CmdbItem, string Fault, Priority Priority)>();

        private readonly (string CatalogItem, string CmdbItem, string Fault, Priority Priority) UNKNOWN_MAPPING
            = ("Integration Unlisted", "Unlisted", "Unlisted", Priority.P3);

        private void BuildMapping()
        {
            _fullMapAlertToIncident.Add(("HHT", "Authentication", "Excessive POST 500s"), ("Integration", "HHT", "Authentication Failures", Priority.P1));

            _mapComponentToIncident.Add(("HHT","Catalog"), ("Integration", "HHT", "Catalog Failures", Priority.P1));
        }
        
        public Transformer(IIncidentManagementService componentStateEvaluator)
        {
            _incidentManagementService = componentStateEvaluator;
            BuildMapping();
        }


        public AlertResponse Transform(Alert alert)
        {
            AlertResponse alertResponse;

            var incident = MapIncidentToAlert(alert);

            var componentState = _incidentManagementService.RecordIncident(incident);

            if (componentState.wasAlreadyDown)
            {
                var incidentActivityResponse = new IncidentActivity(incident.Id, alert.Details, alert.CreatedOn);
                alertResponse = new AlertResponse(null, incidentActivityResponse);
            }
            else
            {
                alertResponse = new AlertResponse(incident, null);
            }
            
            return alertResponse;
        }

        private Incident MapIncidentToAlert(Alert alert)
        {
            Incident incident;

            if (_fullMapAlertToIncident.ContainsKey((alert.Product, alert.Component, alert.Fault)))
            {
                var mappedIncident = _fullMapAlertToIncident[(alert.Product, alert.Component, alert.Fault)];
                incident = new Incident(mappedIncident.CatalogItem,
                    mappedIncident.CmdbItem,
                    mappedIncident.Fault,
                    alert.Details,
                    alert.CreatedOn,
                    mappedIncident.Priority);

                return incident;
            }
            if (_mapComponentToIncident.ContainsKey((alert.Product, alert.Component)))
            {
                var mappedIncident = _mapComponentToIncident[(alert.Product, alert.Component)];
                incident = new Incident(mappedIncident.CatalogItem,
                    mappedIncident.CmdbItem,
                    mappedIncident.Fault,
                    alert.Details,
                    alert.CreatedOn,
                    mappedIncident.Priority);
                
                return incident;
            }
            
            incident = new Incident(UNKNOWN_MAPPING.CatalogItem,
                    UNKNOWN_MAPPING.CmdbItem,
                    UNKNOWN_MAPPING.Fault,
                    alert.Details,
                    alert.CreatedOn,
                    UNKNOWN_MAPPING.Priority);
            
            return incident;
        }

    }

    public enum Priority
    {
        P1Plus,
        P1,
        P2,
        P3
    }

    public readonly struct Alert
    {
        public Alert(string product,
            string component,
            string fault,
            DateTime createdOn,
            string details)
        {
            Product = product;
            Component = component;
            Fault = fault;
            CreatedOn = createdOn;
            Details = details;
        }
        public string Product { get; }
        public string Component { get; }
        public string Fault { get; }
        public DateTime CreatedOn { get; }
        public string Details { get; }
    }

    public readonly struct AlertResponse
    {
        public AlertResponse(Incident? incidentResponse, 
            IncidentActivity? incidentActivityResponse)
        {
            IncidentResponse = incidentResponse;
            IncidentActivityResponse = incidentActivityResponse;
        }
        public bool IsIncident => IncidentResponse != null;
        public bool IsIncidentActivity => IncidentActivityResponse != null;

        public Incident? IncidentResponse { get; }
        public IncidentActivity? IncidentActivityResponse { get; }
    }

    

    public readonly struct IncidentActivity
    {
        public IncidentActivity(string id, string details, DateTime createdOn)
        {
            Id = id;
            Details = details;
            CreatedOn = createdOn;
        }
        public string Id { get; }
        public string Details { get; }
        public DateTime CreatedOn { get; }
    }
}
