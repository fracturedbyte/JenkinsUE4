@echo off

rem This variable is important, we are setting up jenkins home directory
set JENKINS_HOME=%~dp0\JenkinsHome\

if not exist %JENKINS_HOME% (
	mkdir %JENKINS_HOME%
	)
	
call java -jar jenkins.war -httpPort=8080

pause