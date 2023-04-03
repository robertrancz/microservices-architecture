using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Threading;

namespace Catalog.Service.Metrics
{
    public class OtelMetrics
    {
        //Catalog items meters
        private  Counter<int> CatalogItemsAddedCounter { get; }
        private  Counter<int> CatalogItemsDeletedCounter { get; }
        private  Counter<int> CatalogItemsUpdatedCounter { get; }
        private  ObservableGauge<int> TotalCatalogItemsGauge { get; }
        private int _totalCatalogItems = 0;

        public string MetricName { get; }

        public OtelMetrics(string meterName = "Catalog")
        {
            var meter = new Meter(meterName);
            MetricName = meterName;

            CatalogItemsAddedCounter = meter.CreateCounter<int>("catalog-items-added", "Item");
            CatalogItemsDeletedCounter = meter.CreateCounter<int>("catalog-items-deleted", "Item");
            CatalogItemsDeletedCounter = meter.CreateCounter<int>("catalog-items-deleted", "Item");
            CatalogItemsUpdatedCounter = meter.CreateCounter<int>("catalog-items-updated", "Item");
            TotalCatalogItemsGauge = meter.CreateObservableGauge<int>("total-catalog-items", () => new[] { new Measurement<int>(_totalCatalogItems) });
        }


        // Catalog items meters
        public void AddCatalogItems() => CatalogItemsAddedCounter.Add(1);
        public void DeleteCatalogItems() => CatalogItemsDeletedCounter.Add(1);
        public void UpdateCatalogItems() => CatalogItemsUpdatedCounter.Add(1);
        public void IncreaseTotalCatalogItems() => _totalCatalogItems++;
        public void DecreaseTotalCatalogItems() => _totalCatalogItems--;
    }
}