set EngineDir=%1
set ProjectDir=%2

set BUILD=%EngineDir%\Build\BatchFiles\

set ProjectName=%3

%BUILD%\Rebuild.bat %ProjectName%Editor Win64 Development %ProjectDir%\%ProjectName%.uproject -waitmutex