set EngineDir=%1
set ProjectDir=%2

set BUILD=%EngineDir%\Build\BatchFiles\

set TARGETdir=%3
set ProjectName=%4
set Platform=%5
set Configuration=%6
set AdditionalCommandline=%~7

echo Build Started

call %BUILD%\RunUAT BuildCookRun -nocompileeditor -nop4 -project=%ProjectDir%\%ProjectName%.uproject -cook -stage -archive -archivedirectory=%TARGETdir% -package -clientconfig=%Configuration% -ue4exe=UE4Editor-Cmd.exe %AdditionalCommandline% -compressed -iterativecooking -pak -prereqs -nodebuginfo -targetplatform=%Platform% -build -CrashReporter -utf8output -compile

echo Build Done