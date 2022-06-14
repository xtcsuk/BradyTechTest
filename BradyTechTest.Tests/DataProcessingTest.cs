using BradyTechTest.Library.Models;
using BradyTechTest.Library.Services;
using FluentAssertions;
using FluentAssertions.Execution;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using static BradyTechTest.Library.Models.GenerationReportModel;
using static BradyTechTest.Library.Models.GenerationReportModel.Coal;
using static BradyTechTest.Library.Models.GenerationReportModel.Gas;
using static BradyTechTest.Library.Models.GenerationReportModel.Generator;
using static BradyTechTest.Library.Models.GenerationReportModel.Generator.Generation;
using static BradyTechTest.Library.Models.GenerationReportModel.Wind;
using static BradyTechTest.Library.Models.ReferenceDataModel;
using static BradyTechTest.Library.Models.ReferenceDataModel.Factors;

namespace BradyTechTest.Tests
{
    public class DataProcessingTest
    {
# region Private fields

        private GenerationReportModel _generationReport;
        private ReferenceDataModel _referenceData;
        private string windOffshore = "Wind[Offshore]";
        private string windOnshore = "Wind[Onshore]";
        private string gas = "Gas";
        private string coal = "Coal";
        private decimal _totalOffshoreWindDailyGenerationValue = 1662.617445705m;
        private decimal _totalOnshoreWindDailyGenerationValue = 4869.453917394m;
        private decimal _totalGasDailyGenerationValue = 8512.254605520m;
        private decimal _totalCoalDailyGenerationValue = 5341.716526632m;
        private DateTime _coalHighestDailyEmissionDate = DateTime.Parse("2017-01-01T00:00:00+00:00");
        private decimal _coalHighestDailyEmission = 137.175004008m;
        private DateTime _gasHighestDailyEmissionDate = DateTime.Parse("2017-01-03T00:00:00+00:00");
        private decimal _gasHighestDailyEmission = 5.132380700m;
        private int _heatRate = 1;

# endregion

        [SetUp]
        public void SetUp()
        {
            /*ToDo move code into private method and assign result to the private var*/
            _generationReport = new GenerationReportModel
            {
                WindData = new Wind
                {
                    WindGenerators = new List<WindGenerator>
                    {
                        new WindGenerator
                        {
                            Name = windOffshore,
                            GenerationData = new Generation
                            {
                                Days = new List<Day>
                                {
                                    new Day
                                    {
                                        Energy = 100.368m, Price = 20.148m
                                    },
                                    new Day
                                    {
                                        Energy = 90.843m, Price = 25.516m
                                    },
                                    new Day
                                    {
                                        Energy = 87.843m, Price = 22.015m
                                    }
                                }
                            },
                            Location = "Offshore"
                        },
                        new WindGenerator
                        {
                            Name = windOnshore,
                            GenerationData = new Generation
                            {
                                Days = new List<Day>
                                {
                                    new Day
                                    {
                                        Energy = 56.578m, Price = 29.542m
                                    },
                                    new Day
                                    {
                                        Energy = 48.540m, Price = 22.954m
                                    },
                                    new Day
                                    {
                                        Energy = 98.167m, Price = 24.059m
                                    }
                                }
                            },
                            Location = "Onshore"
                        }
                    }
                },
                GasData = new Gas
                {
                    GasGenerators = new List<GasGenerator>
                    {
                        new GasGenerator
                        {
                            Name = gas,
                            EmissionsRating = 0.038m,
                            GenerationData = new Generation
                            {
                                Days = new List<Day>
                                {
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-01T00:00:00+00:00"),
                                        Energy = 259.235m,
                                        Price = 15.837m
                                    },
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-02T00:00:00+00:00"),
                                        Energy = 235.975m,
                                        Price = 16.556m
                                    },
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-03T00:00:00+00:00"),
                                        Energy = 240.325m,
                                        Price = 17.551m
                                    }
                                }
                            }
                        }
                    }
                },
                CoalData = new Coal
                {
                    CoalGenerators = new List<CoalGenerator>
                    {
                        new CoalGenerator
                        {
                            Name = coal,
                            TotalHeatInput = 11.815m,
                            ActualNetGeneration = 11.815m,
                            EmissionsRating = 0.482m,
                            GenerationData = new Generation
                            {
                                Days = new List<Day>
                                {
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-01T00:00:00+00:00"),
                                        Energy = 350.487m,
                                        Price = 10.146m
                                    },
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-02T00:00:00+00:00"),
                                        Energy = 348.611m,
                                        Price = 11.815m
                                    },
                                    new Day
                                    {
                                        Date = DateTime.Parse("2017-01-03T00:00:00+00:00"),
                                        Energy = 0m,
                                        Price = 11.815m
                                    }
                                }
                            }
                        }
                    }
                }
            };

            /*ToDo move code into private method and assign result to the private var*/
            _referenceData = new ReferenceDataModel()
            {
                FactorsData = new Factors
                {
                    ValueFactor = new Factor
                    {
                        High = 0.946m,
                        Medium = 0.696m,
                        Low = 0.265m
                    },
                    EmissionsFactor = new Factor()
                    {
                        High = 0.812m,
                        Medium = 0.562m,
                        Low = 0.312m
                    }
                }     
            };
        }

#region Tests
        [Test]
        public void Calculate_DailyGeneration_Values()
        {
            var serivce = new DataProcessingService();

            var results = serivce.TotalGenerationValue(_generationReport, _referenceData);
            var totalOffshoreWindDailyGenerationValue = Math.Round(results.Generators.Where(r => r.Name == windOffshore).Sum(r => r.Total), 9);
            var totalOnshoreWindDailyGenerationValue = Math.Round(results.Generators.Where(r => r.Name == windOnshore).Sum(r => r.Total), 9);
            var totalGasDailyGenerationValue = Math.Round(results.Generators.Where(r => r.Name == gas).Sum(r => r.Total), 9);
            var totalCoalDailyGenerationValue = Math.Round(results.Generators.Where(r => r.Name == coal).Sum(r => r.Total), 9);

            using (new AssertionScope())
            {
                _totalOffshoreWindDailyGenerationValue.Should().Be(totalOffshoreWindDailyGenerationValue);
                _totalOnshoreWindDailyGenerationValue.Should().Be(totalOnshoreWindDailyGenerationValue);
                _totalGasDailyGenerationValue.Should().Be(totalGasDailyGenerationValue);
                _totalCoalDailyGenerationValue.Should().Be(totalCoalDailyGenerationValue);
            }
        }

        [Test]
        public void Calculate_CoalHighestDailyEmissions_Date()
        {
            var serivce = new DataProcessingService(); /*ToDo declare private var */

            var results = serivce.HighestDailyEmissions(_generationReport, _referenceData);
            var coalHighestDailyEmissionDate = results.Days.Where(r => r.Name == coal).Select(r => r.Date).FirstOrDefault();

            _coalHighestDailyEmissionDate.Should().Be(coalHighestDailyEmissionDate);
        }

        [Test]
        public void Calculate_CoalHighestDailyEmissions_Values()
        {
            var serivce = new DataProcessingService();

            var results = serivce.HighestDailyEmissions(_generationReport, _referenceData);
            var coalHighestDailyEmission = results.Days.Where(r => r.Name == coal).Select(r => r.Emission).FirstOrDefault();

            _coalHighestDailyEmission.Should().Be(coalHighestDailyEmission);
        }

        [Test]
        public void Calculate_GasHighestDailyEmissions_Date()
        {
            var serivce = new DataProcessingService();

            var results = serivce.HighestDailyEmissions(_generationReport, _referenceData);
            var gasHighestDailyEmissionDate = results.Days.Where(r => r.Name == gas).Select(r => r.Date).FirstOrDefault();

            _gasHighestDailyEmissionDate.Should().Be(gasHighestDailyEmissionDate);
        }

        [Test]
        public void Calculate_GasHighestDailyEmissions_Values()
        {
            var serivce = new DataProcessingService();

            var results = serivce.HighestDailyEmissions(_generationReport, _referenceData);
            var gasHighestDailyEmission = results.Days.Where(r => r.Name == gas).Select(r => r.Emission).FirstOrDefault();

            _gasHighestDailyEmission.Should().Be(gasHighestDailyEmission);
        }

        [Test]
        public void Calculate_ActualHeatRate_Values()
        {
            var serivce = new DataProcessingService();

            var results = serivce.ActualHeatRate(_generationReport);
            var heatRate = results.ActualHeatRateData.Where(r => r.Name == coal).Select(r => r.HeatRate).FirstOrDefault();

            _heatRate.Should().Be((int)heatRate);
        }

        #endregion
    }
}
