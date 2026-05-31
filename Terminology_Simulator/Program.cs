using Terminology_Simulator.Presentation;
using Terminology_Simulator.Services;
using Terminology_Simulator.Storage;

IStorageService storageService = new JsonStorageService("data/appstate.json");
TermManager termManager = new(storageService);
TrainingService trainingService = new(termManager);
ConsoleMenu consoleMenu = new(termManager, trainingService);

consoleMenu.Run();