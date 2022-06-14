using BradyTechTest.Library.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace BradyTechTest.UI
{
    public class Start
    {
        private readonly ILogger<Start> _log;
        private readonly IConfiguration _config;
        private readonly IFileProcessingService _fileProcessingService;

        public Start(ILogger<Start> log,IConfiguration config, IFileProcessingService fileProcessingService)
        {
            _log = log;
            _config = config;
            _fileProcessingService = fileProcessingService;
            InitialiseApplication();
        }

        private void InitialiseApplication()
        {
            try
            {
                _log.LogInformation("Initialising Application");
                Directory.CreateDirectory(_config.GetValue<string>("InputFolder"));
                Directory.CreateDirectory(_config.GetValue<string>("OutputFolder"));
                Directory.CreateDirectory(_config.GetValue<string>("InputFolderArchive"));

            }
            catch (Exception ex)
            {
                _log.LogError(ex, ex.Message);
            }

        }

        public void Run()
        {
            RunInputFileWatcher(_config.GetValue<string>("InputFolder"));
        }

        private void RunInputFileWatcher(string directoryPath)
        {
            _fileProcessingService.RunInputFileWatcher(directoryPath);
        }

       
    }
}
