set REPOS=%1
set REV=%2
set TXN_NAME=%3

call C:\CI\SVN\DevelopmentTools\CI\Jenkins\Builder\Binaries\UE4BuildHelper.exe -c %REV% -suppresslogs

exit 0