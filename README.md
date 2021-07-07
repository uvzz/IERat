# 
[![LinkedIn][linkedin-shield]][linkedin-url]


<!-- PROJECT LOGO -->
<br />
<p align="center">
  <a href="https://github.com/uvzz/IERat">
    <img src="https://github.com/uvzz/IERat/raw/master/logo_size.jpg" alt="Logo" width="165" height="165">
  </a>

  <h3 align="center">A simple C2 framework against browser isolation proxies</h3>

<!-- TABLE OF CONTENTS -->
<details open="open">
  <summary>Table of Contents</summary>
  <ol>
    <li>
      <a href="#about-the-project">About The Project</a>
      <ul>
        <li><a href="#built-with">Built With</a></li>
      </ul>
    </li>
    <li>
      <a href="#getting-started">Getting Started</a>
      <ul>
        <li><a href="#prerequisites">Prerequisites</a></li>
        <li><a href="#installation">Installation</a></li>
      </ul>
    </li>
    <li><a href="#usage">Usage</a></li>
    <li><a href="#roadmap">Roadmap</a></li>
    <li><a href="#contributing">Contributing</a></li>
    <li><a href="#license">License</a></li>
    <li><a href="#contact">Contact</a></li>
    <li><a href="#acknowledgements">Acknowledgements</a></li>
  </ol>
</details>



<!-- ABOUT THE PROJECT -->
## About The Project

While I was penetration testing networks that use browser isolation proxies, I noticed that my malware is unable to communicate with its C2 server, although I used a cloudfront.com domain using domain fronting. I could access it via a web browser without any problem.
After a little research on browser isolation products, I found that these proxy servers inject JavaScript files to the browser, enforcing their policy and changing the entire structure of the DOM. Therefore, the server responses will go through the proxy and will be re-rendered as images/SVG so only the browser will be able to understand them.

But, I noticed that what’s inside the <head> tag, such as the page title and <link> tags will remain untouched. There, a possible C2  communication would be:
  
Client-> HTTP requests via browser automation -> C2 server -> HTTP page with a payload inside the headtag, e.g. favicon value -> browser isolation proxy -> browser controllered by client -> client
  
The client uses a COM object and sends requests using Internet explorer, then extracts the favicon base64 value from the C2 server responses, which is controlled by the user to send commands through a simple console UI.
This can be further developed to upload/download files, use HTTPS, and convert the client to a PowerShell script.
  
<p align="center">
  <img src="https://github.com/uvzz/IERat/blob/master/antiscanme.png?raw=true" alt="antiscanme" width="290" height="395">
</a>

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

<!-- USAGE EXAMPLES -->
## Usage

  * Example with CrowdStrike sensor:
<video controls autoplay>
  <source src="https://github.com/uvzz/IERat/blob/master/2021-07-07_21-31-51.mp4" type="video/mp4">
 </video>
  
<!-- ROADMAP -->
## Roadmap

- Add HTTPS Support.
- File uploads/downloads.
- Screenshot option.
- Webcam capture.
- Keylogger.

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
