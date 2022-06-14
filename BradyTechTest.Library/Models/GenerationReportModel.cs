using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace BradyTechTest.Library.Models
{
    [Serializable, XmlRoot("GenerationReport")]
    public class GenerationReportModel
    {
        [XmlElement("Wind")]
        public Wind WindData { get; set; }

        [XmlElement("Gas")]
        public Gas GasData { get; set; }

        [XmlElement("Coal")]
        public Coal CoalData { get; set; }




        public class Wind
        {
            [XmlElement("WindGenerator")]
            public List<WindGenerator> WindGenerators { get; set; }

            public class WindGenerator : Generator
            {
                public string Location { get; set; }

            }
        }

        public class Gas
        {
            [XmlElement("GasGenerator")]
            public List<GasGenerator> GasGenerators { get; set; }

            public class GasGenerator : Generator
            {                
                public decimal EmissionsRating { get; set; }
            }
        }

        public class Coal
        {
            [XmlElement("CoalGenerator")]
            public List<CoalGenerator> CoalGenerators { get; set; }

            public class CoalGenerator : Generator
            {
               public decimal TotalHeatInput { get; set; }

               public decimal ActualNetGeneration { get; set; }

               public decimal EmissionsRating { get; set; }              
            }
        }

        public class Generator
        {
            public string Name { get; set; }

            [XmlElement("Generation")]
            public Generation GenerationData { get; set; }


            public class Generation
            {

                [XmlElement("Day")]
                public List<Day> Days { get; set; }

                public class Day
                {
                    [XmlElement("Date")]
                    public DateTime Date { get; set; }

                    public decimal Energy { get; set; }

                    public decimal Price { get; set; }
                }
            }
        }
    }
}
