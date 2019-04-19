#!/bin/bash

# Initial parameters
SERVER=http://[JENKINS_URL]:8080

USER=[JENKINS_USER]
APITOKEN=[JENKINS_USER_TOKEN]

CHANGELIST_NUMBER=$1
USERNAME=$2
###

if [ -z "$CHANGELIST_NUMBER" ]; then
	echo "First Argument Changelist number is empty"
	exit -1
fi

if [ -z "$USERNAME" ]; then
	echo "Second Argument Username is empty"
	exit -1
fi

curl --user $USER:$APITOKEN -d "token=c934caacc9ac76f666c768486668fe1d&ChangelistNumber=$CHANGELIST_NUMBER&Username=$USERNAME" $SERVER/job/P4RemoteHook/buildWithParameters

