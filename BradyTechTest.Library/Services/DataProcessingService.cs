using BradyTechTest.Library.Models;
using BradyTechTest.Library.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using static BradyTechTest.Library.Models.GenerationOutputModel;
using static BradyTechTest.Library.Models.GenerationOutputModel.ActualHeatRates;
using static BradyTechTest.Library.Models.GenerationOutputModel.Totals;

namespace BradyTechTest.Library.Services
{
    public class DataProcessingService : IDataProcessingService
    {
        public Totals TotalGenerationValue(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var totalDailyGenerationValue = new Totals()
            {
                Generators = TotalGenerationValues(generationReportModel, referenceDataModel)
            };

            return totalDailyGenerationValue;
        }

        private List<Generator> TotalGenerationValues(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var totalOffShoreWindValue = new Generator();
            var totalOnShoreWindValue = new Generator();

            foreach (var generator in TotalOffShoreWindDailyGenerationValue(generationReportModel, referenceDataModel))
            {
                if (!string.IsNullOrWhiteSpace(generator.Name) && generator.Name.ToLower().Contains("offshore"))
                {
                    totalOffShoreWindValue = generator;
                }
                else if (!string.IsNullOrWhiteSpace(generator.Name) && generator.Name.ToLower().Contains("onshore"))
                {
                    totalOnShoreWindValue = generator;
                }
            }
            return new List<Generator>()
            {
                totalOffShoreWindValue,
                totalOnShoreWindValue,
                TotalGasDailyGenerationValue(generationReportModel, referenceDataModel),
                TotalCoalDailyGenerationValue(generationReportModel, referenceDataModel)
            };
        }

        public MaxEmissionGenerators HighestDailyEmissions(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var maxEmissionGenerators = new List<MaxEmissionGenerators.Day>();

            var dailyGasEmissions = DailyGasEmissions(generationReportModel, referenceDataModel);
            var dailyCoalEmissions = DailyCoalEmissions(generationReportModel, referenceDataModel);

            if (dailyGasEmissions?.Count > 1)
            {
                foreach (var dailyGasEmission in dailyGasEmissions)
                {
                    foreach (var dailyCoalEmission in dailyCoalEmissions)
                    {
                        if (dailyGasEmission.Date == dailyCoalEmission.Date)
                        {
                            if (dailyGasEmission.Emission > dailyCoalEmission.Emission)
                            {
                                maxEmissionGenerators.Add(new MaxEmissionGenerators.Day
                                {
                                    Date = dailyGasEmission.Date,
                                    Emission = dailyGasEmission.Emission,
                                    Name = dailyGasEmission.Name
                                });
                            }
                            else
                            {
                                maxEmissionGenerators.Add(new MaxEmissionGenerators.Day
                                {
                                    Date = dailyCoalEmission.Date,
                                    Emission = dailyCoalEmission.Emission,
                                    Name = dailyCoalEmission.Name
                                });
                            }
                        }
                    }
                }
            }

            return new MaxEmissionGenerators { Days = maxEmissionGenerators };
        }

        public ActualHeatRates ActualHeatRate(GenerationReportModel generationReportModel)
        {
            var ActualHeatRates = new ActualHeatRates()
            {
                ActualHeatRateData = CoalActualHeatRates(generationReportModel)
            };

            return ActualHeatRates;
        }

        private IList<Generator> TotalOffShoreWindDailyGenerationValue(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            string offshore = "";
            string onshore = "";
            decimal totalOffShoreWindValue = 0;
            decimal totalOnShoreWindValue = 0;

            if (generationReportModel?.WindData?.WindGenerators.Count > 0)
            {
                foreach (var generator in generationReportModel.WindData.WindGenerators)
                {

                    if (!string.IsNullOrWhiteSpace(generator.Location) && generator.Location.ToLower().Equals("offshore"))
                    {
                        offshore = generator.Name;

                        foreach (var day in generator.GenerationData.Days)
                        {
                            totalOffShoreWindValue += day.Price * day.Energy * referenceDataModel.FactorsData.ValueFactor.Low;
                        }
                    }
                    else if (!string.IsNullOrWhiteSpace(generator.Location) && generator.Location.ToLower().Equals("onshore"))
                    {
                        onshore = generator.Name;

                        foreach (var day in generator.GenerationData.Days)
                        {
                            totalOnShoreWindValue += day.Price * day.Energy * referenceDataModel.FactorsData.ValueFactor.High;
                        }
                    }
                };
            }

            return new List<Generator> 
            { 
                new Generator { Name = offshore, Total = totalOffShoreWindValue },
                new Generator { Name = onshore, Total = totalOnShoreWindValue }
            };
        }

        private Generator TotalGasDailyGenerationValue(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            string name = "";
            decimal totalGasDailyGenerationValue = 0;

            if (generationReportModel?.GasData?.GasGenerators.Count > 0)
            {
                foreach (var generator in generationReportModel.GasData.GasGenerators)
                {
                    name = generator.Name;
                    foreach (var day in generator.GenerationData.Days)
                    {
                        totalGasDailyGenerationValue += day.Price * day.Energy * referenceDataModel.FactorsData.ValueFactor.Medium;
                    }

                }
            }

            return new Generator() { Name = name, Total = totalGasDailyGenerationValue };
        }

        private Generator TotalCoalDailyGenerationValue(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            string name = "";
            decimal totalCoalDailyGenerationValue = 0;

            if (generationReportModel?.CoalData?.CoalGenerators.Count > 0)
            {
                foreach (var generator in generationReportModel.CoalData.CoalGenerators)
                {
                    name = generator.Name;
                    foreach (var day in generator.GenerationData.Days)
                    {
                        totalCoalDailyGenerationValue += day.Price * day.Energy * referenceDataModel.FactorsData.ValueFactor.Medium;
                    }

                }
            }

            return new Generator() { Name = name, Total = totalCoalDailyGenerationValue };
        }

        private IList<MaxEmissionGenerators.Day> DailyGasEmissions(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var dailyGasEmissions = new List<MaxEmissionGenerators.Day>();
           var emissionsFactorMedium = referenceDataModel.FactorsData.EmissionsFactor?.Medium ?? decimal.Zero;
            if (generationReportModel?.GasData?.GasGenerators.Count > 0)
            {
                foreach (var generator in generationReportModel.GasData.GasGenerators)
                {
                    foreach (var day in generator.GenerationData.Days)
                    {
                        dailyGasEmissions.Add(new MaxEmissionGenerators.Day
                        {
                            Date = day.Date,
                            Emission =
                            day.Energy *
                            generator.EmissionsRating *
                            emissionsFactorMedium,//referenceDataModel.FactorsData.EmissionsFactor.Medium,
                            Name = generator.Name
                        });
                    }

                };
            }

            return dailyGasEmissions;
        }

        private IList<MaxEmissionGenerators.Day> DailyCoalEmissions(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var dailyCoalEmissions = new List<MaxEmissionGenerators.Day>();

            if (generationReportModel?.CoalData?.CoalGenerators.Count > 0)
            {
                foreach (var generator in generationReportModel.CoalData.CoalGenerators)
                {
                    foreach (var day in generator.GenerationData.Days)
                    {
                        dailyCoalEmissions.Add(new MaxEmissionGenerators.Day
                        {
                            Date = day.Date,
                            Emission =
                            day.Energy *
                            generator.EmissionsRating *
                            referenceDataModel.FactorsData.EmissionsFactor.High,
                            Name = generator.Name
                        });
                    }

                };
            }

            return dailyCoalEmissions;
        }
        private List<ActualHeatRate> CoalActualHeatRates(GenerationReportModel generationReportModel)
        {
            var coalGenerators = new List<ActualHeatRate>();

            if (generationReportModel?.CoalData?.CoalGenerators.Count > 0)
            {
                foreach (var coalGenerator in generationReportModel.CoalData.CoalGenerators)
                {
                    coalGenerators.Add(
                        new ActualHeatRate
                        {
                            Name = coalGenerator.Name,
                            HeatRate = coalGenerator.TotalHeatInput > 0 && coalGenerator.ActualNetGeneration > 0
                            ? (int)coalGenerator.TotalHeatInput / (int)coalGenerator.ActualNetGeneration
                            : 0
                        });
                }

            }

            return coalGenerators;
        }
    }
}
