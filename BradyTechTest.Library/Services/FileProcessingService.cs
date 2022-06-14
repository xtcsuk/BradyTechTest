using BradyTechTest.Library.Models;
using BradyTechTest.Library.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace BradyTechTest.Library.Services
{
    public class FileProcessingService : IFileProcessingService
    {
        private readonly ILogger<FileProcessingService> _log;
        private readonly IConfiguration _config;
        private readonly IDataProcessingService _dataProcessingService;
        private ReferenceDataModel _referenceDataModel;
        private string _referenceDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"Data\ReferenceData.xml");
        private string _InputFolderPath;
        private string _OutputFolderPath;
        private string _ArchiveFolderPath;

        public FileProcessingService(ILogger<FileProcessingService> log, IConfiguration config, IDataProcessingService dataProcessingService)
        {
            _log = log;
            _config = config;
            _dataProcessingService = dataProcessingService;
            _InputFolderPath = _config.GetValue<string>("InputFolder");
            _OutputFolderPath = _config.GetValue<string>("OutputFolder");
            _ArchiveFolderPath = _config.GetValue<string>("InputFolderArchive");
        }
        public void RunInputFileWatcher(string directoryPath)
        {
            using (FileSystemWatcher watcher = new FileSystemWatcher())
            {
                watcher.Path = directoryPath;
                _log.LogInformation("File watcher location for Input folder set to {directoryPath}", Path.GetFullPath(directoryPath));

                watcher.Filter = $"*.xml";

                watcher.Created += OnChanged;

                watcher.EnableRaisingEvents = true;

                _log.LogInformation("File watcher running....");

                Console.WriteLine("Press 'q' and 'Enter' to stop file watcher and quit the application");

                while (Console.Read() != 'q');
            }
        }

        private readonly object myLock = new object();  /*ToDo remove*/
        private void ProcessXmlFile(string fileName)
        {
            _log.LogInformation("File {fileName} placed in input folder", fileName);
            var xmlSerializerReferenceData = new XmlSerializer(typeof(ReferenceDataModel));
            _referenceDataModel = new ReferenceDataModel();

            try
            {
                    using (var reader = new StreamReader(_referenceDataPath))
                    {
                        _log.LogInformation("Processing file {fileName}", fileName);
                        var xmlDeserializerReferenceData = (ReferenceDataModel)xmlSerializerReferenceData.Deserialize(reader);
                        _referenceDataModel.FactorsData = xmlDeserializerReferenceData.FactorsData;
                    }

                    var xmlSerializer = new XmlSerializer(typeof(GenerationReportModel));

                    using (var reader = new StreamReader($"{_InputFolderPath}\\{fileName}.xml"))
                    {
                        var generationReportModel = (GenerationReportModel)xmlSerializer.Deserialize(reader);

                        var generationOutputModel = ProcessGenerationData(generationReportModel, _referenceDataModel);
                        _log.LogInformation("Finished processing file {fileName}", fileName);

                        CreateOutputXmlFile(generationOutputModel, fileName);
                    }

                    MoveInputFileToArchive(fileName);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }


        }

        private void MoveInputFileToArchive(string fileName)
        {
            var date = new StringBuilder(DateTime.UtcNow.ToString());
            date.Replace(' ', '-')
                .Replace('/', '-')
                .Replace(':', '-');
            try
            {
                Directory.Move($"{_InputFolderPath}\\{fileName}.xml", Path.Combine(_ArchiveFolderPath, $"{fileName}-{date}.xml"));
                _log.LogInformation("Moved file {fileName} to location {InputFolderArchive}", fileName, Path.GetFullPath(_ArchiveFolderPath));
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
    
        }

        private void CreateOutputXmlFile(GenerationOutputModel generationOutputModel, string fileName)
        {
            _log.LogInformation("Genrating results file");
            var xmlSerializer = new XmlSerializer(typeof(GenerationOutputModel));

            try
            {
                using (var writer = new StreamWriter($"{_OutputFolderPath}\\{fileName}-Result.xml"))
                {
                    xmlSerializer.Serialize(writer, generationOutputModel);
                    _log.LogInformation("Finished genrating result file");
                    _log.LogInformation("Result file created {fileName}-Result.xml in location {outputFolder}", fileName, Path.GetFullPath(_OutputFolderPath));
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
 
        }

        private GenerationOutputModel ProcessGenerationData(GenerationReportModel generationReportModel, ReferenceDataModel referenceDataModel)
        {
            var generationOutputModel = new GenerationOutputModel();

            try
            {
                _dataProcessingService.TotalGenerationValue(generationReportModel, referenceDataModel);
                generationOutputModel.MaxEmissionGeneratorsData = _dataProcessingService.HighestDailyEmissions(generationReportModel, referenceDataModel);
                generationOutputModel.ActualHeatRatesData = _dataProcessingService.ActualHeatRate(generationReportModel);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }
          

            return generationOutputModel;
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            var fileName = Path.GetFileNameWithoutExtension(e.Name);
            _log.LogInformation("New XML file detected");
            ProcessXmlFile(fileName);
            Console.WriteLine("Press 'q' and 'Enter' to stop file watcher and quit the application"); /* does this have to be here, already declared in main */
        }
    }
}
