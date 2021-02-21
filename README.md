# Rotate.Pictures
2020-05-21
See article: https://www.codeproject.com/Articles/1204207/Rotating-Picture-Display

## Installation Instructions
Get the source code and compile it, preferably in Release mode.

### Remarks
Code targets .Net Framework 4.8.  You may change the target framework if you do not have or do not wish to install the .Net Framework 4.8

Now that your code is compiled copy the contents of the bin/Release (or bin/Debug) into the machine/directory where you wish to run it from.

Your last step is the modify the configuration file, Rotate.Pictures.exe.config:

In the appSettings section of the configuration file, change the following entries:
- key="Initial Folders"
```
<?xml version="1.0" encoding="utf-8"?>
<configuration>
	<configSections>
		<section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler, log4net" />
	</configSections>
	<appSettings>
		<!-- Directories where the system will look for pictures to display.
					Directories are semicolon separated.  Ex: value="g:\pic;m:\pic" -->
		<add key="Initial Folders" value="M:\Pictures" />
```

The value part of the entry should contain the directory where the pictures are.  Do not worry about subdirectories, the software will crawl through the subdirectories and look for pictures, then display them in a random rotating fashion.

If your pictures are spread over multiple directories, then separate them with a semicolon.  So, if you have pictures in the following directories: “C:\MyPictures”, “D:\YourPictures” and “C:\RandomDirectory\FarOut\FunnyPics” then the **Initial Folders** entry will look like:
```<add key="Initial Folders" value=" C:\MyPictures;D:\YourPictures;C:\RandomDirectory\FarOut\FunnyPics " />```
