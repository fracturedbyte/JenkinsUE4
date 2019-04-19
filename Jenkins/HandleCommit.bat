REM Initial parameters
set SERVER=http://[JENKINS_URL]:8080

set USER=[JENKINS_USER]
set APITOKEN=[JENKINS_USER_TOKEN]

set CHANGELIST_NUMBER=%1
set USERNAME=%2

set CURL_PATH="C:/Program Files/curl-7.61.1-win64-mingw/bin/"

%CURL_PATH%curl.exe --user %USER%:%APITOKEN% -d "token=c934caacc9ac76f666c768486668fe1d&ChangelistNumber=%CHANGELIST_NUMBER%&Username=%USERNAME%" %SERVER%/job/P4RemoteHook/buildWithParameters

exit 0