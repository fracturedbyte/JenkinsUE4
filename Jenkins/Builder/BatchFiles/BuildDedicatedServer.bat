set EngineDir=%1
set ProjectDir=%2

set BUILD=%EngineDir%\Build\BatchFiles\

set TARGETdir=%3
set ProjectName=%4
set Platform=%5
set Configuration=%6
set AdditionalCommandline=%~7

echo Building the dedicated server...

call %BUILD%\RunUAT BuildCookRun -project=%ProjectDir%/%ProjectName%.uproject -noP4 -platform=%Platform% -clientconfig=%Configuration% -serverconfig=%Configuration% -cook -server -serverplatform=Win64 -noclient -build -stage -pak -archive -archivedirectory=%TARGETdir% -noxge %AdditionalCommandline%

echo Done