using BradyTechTest.Library.Models;
using static BradyTechTest.Library.Models.GenerationOutputModel;

namespace BradyTechTest.Library.Services.Interfaces
{
    public interface IDataProcessingService
    {
        Totals TotalGenerationValue(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel);

        MaxEmissionGenerators HighestDailyEmissions(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel);

        ActualHeatRates ActualHeatRate(GenerationReportModel generationReportModel);
    }
}
