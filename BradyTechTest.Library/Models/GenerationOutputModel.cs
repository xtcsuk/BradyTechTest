using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BradyTechTest.Library.Models
{
    [Serializable, XmlRoot("GenerationOutput")]
    public class GenerationOutputModel
    {
        [XmlElement("Totals")]
        public Totals TotalsData { get; set; }

        public MaxEmissionGenerators MaxEmissionGeneratorsData { get; set; }

        [XmlElement("ActualHeatRates")]
        public ActualHeatRates ActualHeatRatesData { get; set; }

        public class Totals
        {
            [XmlElement("Generator")]
            public List<Generator> Generators { get; set; }

            public class Generator
            {
                public string Name { get; set; }

                public decimal Total { get; set; }
            }
        }
        public class MaxEmissionGenerators
        {
            [XmlElement("Day")]
            public List<Day> Days { get; set; }

            public class Day
            {
                public string Name { get; set; }

                [XmlElement("Date")]
                public DateTime Date { get; set; }

                public decimal Emission { get; set; }
            }
        }

        public class ActualHeatRates
        {
            [XmlElement("ActualHeatRate")]
            public List<ActualHeatRate> ActualHeatRateData { get; set; }

            public class ActualHeatRate
            {
                public string Name { get; set; }

                public long HeatRate { get; set; }
            }
        }
    }
}
