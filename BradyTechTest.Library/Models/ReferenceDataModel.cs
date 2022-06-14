using System;
using System.Xml.Serialization;

namespace BradyTechTest.Library.Models
{
    [Serializable, XmlRoot("ReferenceData")]
    public class ReferenceDataModel
    {
        [XmlElement("Factors")]
        public Factors FactorsData { get; set; }

        public class Factors
        {
            public Factor ValueFactor { get; set; }

            public Factor EmissionsFactor { get; set; }

            public class Factor
            {
                public decimal High { get; set; }

                public decimal Medium { get; set; }

                public decimal Low { get; set; }
            }
        }
    }

}
