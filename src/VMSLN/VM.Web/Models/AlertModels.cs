using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace VM.Web.Models
{
    public class Essentials
    {
        [JsonPropertyName("alertId")] public string AlertId { get; set; }

        [JsonPropertyName("alertRule")] public string AlertRule { get; set; }

        [JsonPropertyName("severity")] public string Severity { get; set; }

        [JsonPropertyName("signalType")] public string SignalType { get; set; }

        [JsonPropertyName("monitorCondition")] public string MonitorCondition { get; set; }

        [JsonPropertyName("monitoringService")]
        public string MonitoringService { get; set; }

        [JsonPropertyName("alertTargetIDs")] public List<string> AlertTargetIDs { get; set; }

        [JsonPropertyName("originAlertId")] public string OriginAlertId { get; set; }

        [JsonPropertyName("firedDateTime")] public DateTime FiredDateTime { get; set; }

        [JsonPropertyName("resolvedDateTime")] public DateTime ResolvedDateTime { get; set; }

        [JsonPropertyName("description")] public string Description { get; set; }

        [JsonPropertyName("essentialsVersion")]
        public string EssentialsVersion { get; set; }

        [JsonPropertyName("alertContextVersion")]
        public string AlertContextVersion { get; set; }
    }

    public class Dimension
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("value")] public string Value { get; set; }
    }

    public class AllOf
    {
        [JsonPropertyName("metricName")] public string MetricName { get; set; }

        [JsonPropertyName("metricNamespace")] public string MetricNamespace { get; set; }

        [JsonPropertyName("operator")] public string Operator { get; set; }

        [JsonPropertyName("threshold")] public string Threshold { get; set; }

        [JsonPropertyName("timeAggregation")] public string TimeAggregation { get; set; }

        [JsonPropertyName("dimensions")] public List<Dimension> Dimensions { get; set; }

        [JsonPropertyName("metricValue")] public double MetricValue { get; set; }
    }

    public class Condition
    {
        [JsonPropertyName("windowSize")] public string WindowSize { get; set; }

        [JsonPropertyName("allOf")] public List<AllOf> AllOf { get; set; }
    }

    public class AlertContext
    {
        [JsonPropertyName("properties")] public object Properties { get; set; }

        [JsonPropertyName("conditionType")] public string ConditionType { get; set; }

        [JsonPropertyName("condition")] public Condition Condition { get; set; }
    }

    public class Data
    {
        [JsonPropertyName("essentials")] public Essentials Essentials { get; set; }

        [JsonPropertyName("alertContext")] public AlertContext AlertContext { get; set; }
    }

    public class AlertModel
    {
        [JsonPropertyName("schemaId")] public string SchemaId { get; set; }

        [JsonPropertyName("data")] public Data Data { get; set; }
    }
}

/* SAMPLE PAYLOAD
 {
  "schemaId": "azureMonitorCommonAlertSchema",
  "data": {
    "essentials": {
      "alertId": "/subscriptions/<subscription ID>/providers/Microsoft.AlertsManagement/alerts/b9569717-bc32-442f-add5-83a997729330",
      "alertRule": "WCUS-R2-Gen2",
      "severity": "Sev3",
      "signalType": "Metric",
      "monitorCondition": "Resolved",
      "monitoringService": "Platform",
      "alertTargetIDs": [
        "/subscriptions/<subscription ID>/resourcegroups/pipelinealertrg/providers/microsoft.compute/virtualmachines/wcus-r2-gen2"
      ],
      "originAlertId": "3f2d4487-b0fc-4125-8bd5-7ad17384221e_PipeLineAlertRG_microsoft.insights_metricAlerts_WCUS-R2-Gen2_-117781227",
      "firedDateTime": "2019-03-22T13:58:24.3713213Z",
      "resolvedDateTime": "2019-03-22T14:03:16.2246313Z",
      "description": "",
      "essentialsVersion": "1.0",
      "alertContextVersion": "1.0"
    },
    "alertContext": {
      "properties": null,
      "conditionType": "SingleResourceMultipleMetricCriteria",
      "condition": {
        "windowSize": "PT5M",
        "allOf": [
          {
            "metricName": "Percentage CPU",
            "metricNamespace": "Microsoft.Compute/virtualMachines",
            "operator": "GreaterThan",
            "threshold": "25",
            "timeAggregation": "Average",
            "dimensions": [
              {
                "name": "ResourceId",
                "value": "3efad9dc-3d50-4eac-9c87-8b3fd6f97e4e"
              }
            ],
            "metricValue": 7.727
          }
        ]
      }
    }
  }
} 
 */