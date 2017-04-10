# Folder-Monitoring-Windows-Service
This project has two windows services running together.
Checks a particular folder for changes, all changes are copied to a new pre designated folder.
Also, user is notified via email of any and all changes that are happening. 

For First Service, first fill in following details in app config:
1) Folder location


For Second Service, first fill in the following details in app config:
1) Folder location
2) User name 
3) Password
4) Recievers email

If you are viewing in visual studio, run using "debug" setting to not have to install.
If you are installing the service, use release build from release folder.

Tested using hotmail to gmail communication. 

P.S: If you want the see how the programs behave when there is no change, change location of log file.
