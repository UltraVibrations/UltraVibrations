using OtterGui.Classes;
using OtterGui.Log;
using OtterGui.Services;

namespace UltraVibrations.Services;

/// <summary>
/// Any file type that we want to save via SaveService.
/// </summary>
public interface ISavable : ISavable<FilenameService> { }

public class SaveService(Logger log, FrameworkManager frameworkManager, FilenameService filenameService)
    : SaveServiceBase<FilenameService>(log, frameworkManager, filenameService), IService { }
