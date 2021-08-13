# 
[![LinkedIn][linkedin-shield]][linkedin-url]


<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/uvzz/IERat">
    <img src="https://github.com/uvzz/IERat/raw/master/logo_size.jpg" alt="Logo" width="165" height="165">
  </a>
  
  <h3 align="center">A simple C2 framework against browser isolation proxies</h3>
  
  <h5 align="center">(for testing / research purposes only!)</h5>
  
  
<p align="center">
    <img src="https://i.imgur.com/cezBrFT.png" alt="poc">
  </a>  

<!-- ABOUT THE PROJECT -->
## About The Project

While I was penetration testing networks that use browser isolation proxies, I noticed that my malware is unable to communicate with its C2 server, although I used a cloudfront.com domain using domain fronting. I could access it via a web browser without any problem.
After a little research on browser isolation products, I found that these proxy servers inject JavaScript files to the browser, enforcing their policy and changing the entire structure of the DOM. Therefore, the server responses will go through the proxy and will be re-rendered as images/SVG so only the browser will be able to understand them.

But, I noticed that what’s inside the <head> tag, such as the page title and <link> tags will remain untouched (**not tested yet on all solutions**) There, a possible C2  communication would be:
  
Client-> HTTP requests via browser automation -> C2 server -> HTTP page with a payload inside the headtag, e.g. favicon value -> browser isolation proxy -> browser controllered by client -> client
  
The client uses a COM object and sends requests using Internet explorer, then extracts the favicon base64 value from the C2 server responses, which is controlled by the user to send commands through a simple console UI.
This can be further developed to upload/download files, use HTTPS, and convert the client to a PowerShell script.
  
<p align="center">
  <img src="https://github.com/uvzz/IERat/blob/master/antiscanme.png?raw=true" alt="antiscanme" width="435" height="592.5">
</p>

### Prerequisites

Set the BaseURL address to your C2 server address before compling the client in IERat.cs
  
.NET Framework 4.8 for the client.
  
.NET Core 5.0 for the server.

### Installation

To run the server on Linux, you should install dotnet-runtime-5.0

Example for Kali/Debian:

* Install and run the server on port 443:
  ```
  wget https://packages.microsoft.com/config/debian/10/packages-microsoft-prod.deb
  sudo dpkg -i packages-microsoft-prod.deb
  sudo apt update
  sudo apt install -y dotnet-runtime-5.0
  sudo dotnet IERatServer.dll --ip=* --port=443
  ```
  
* All compiled module DLL files should reside inside the Modules directory of the server. They are set to compile inside this folder in the project configuration.

  
<!-- Features -->
## Features
IERat$ help

Management Commands
===================
list - Print information about the active agents
history - Print information about commands and their results
interact <agent number> - interact with an agent
timeout <number> - set the inactivity time in seconds for agent disconnections
clear - clear the CLI from messages
exit - kill the server

File Commands
===================
ls - list folders and files in the agent's current directory
mv <source> <destination> - move a file or a folder, change a file's name
cp <source> <destination> - copy a file or a folder
del <destination> - delete a file or a folder
upload <location on server> - upload a file to the agent's current directory
download <location on agent> - download a file to to the loot folder
exec <file to run> - execute a file

Spy Shit
=========
screenshot - take a screenshot of the active desktop
keylogger <start/stop/collect/clear> - start/stop a keylogger thread, collect results or clear them
capture_camera - capture a webcam image of the user jerking off or something...

Credentials
============
chrome - get all chrome passwords

Privileges
==========
uacbypass_taskmanager - Bypass UAC using the taskmanager cleanup task method

Command execution
==========
shell - run a shell command using cmd.exe (not recommended)

  
<!-- ROADMAP -->
## Roadmap

* HTTPS Support
* Persistence
* VM detection
* Load and run PowerShell scripts from RAM using Automation runspace

<!-- MARKDOWN LINKS & IMAGES -->
<!-- https://www.markdownguide.org/basic-syntax/#reference-style-links -->
[contributors-shield]: https://img.shields.io/github/contributors/othneildrew/Best-README-Template.svg?style=for-the-badge
[contributors-url]: https://github.com/othneildrew/Best-README-Template/graphs/contributors
[forks-shield]: https://img.shields.io/github/forks/othneildrew/Best-README-Template.svg?style=for-the-badge
[forks-url]: https://github.com/othneildrew/Best-README-Template/network/members
[stars-shield]: https://img.shields.io/github/stars/othneildrew/Best-README-Template.svg?style=for-the-badge
[stars-url]: https://github.com/othneildrew/Best-README-Template/stargazers
[issues-shield]: https://img.shields.io/github/issues/othneildrew/Best-README-Template.svg?style=for-the-badge
[issues-url]: https://github.com/othneildrew/Best-README-Template/issues
[license-shield]: https://img.shields.io/github/license/othneildrew/Best-README-Template.svg?style=for-the-badge
[license-url]: https://github.com/othneildrew/Best-README-Template/blob/master/LICENSE.txt
[linkedin-shield]: https://img.shields.io/badge/-LinkedIn-black.svg?style=for-the-badge&logo=linkedin&colorB=555
[linkedin-url]: https://www.linkedin.com/in/yuval-moravchick-4311b550/
[product-screenshot]: images/screenshot.png
