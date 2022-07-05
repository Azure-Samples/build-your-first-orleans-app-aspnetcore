# Build your first Orleans app using ASP.NET Core 6.0

This repository contains the sample project for the Microsoft Learn module [Build your first Orleans app using ASP.NET Core 6.0](https://docs.microsoft.com/en-us/learn/modules/orleans-build-your-first-app/3-exercise-setting-up-project). The app demonstrates basic Orleans concepts, such as working with Grains, Silos and persistent state. These concepts are used to build a simple URL shortener utility app. The project also shows how to integrate these features with a basic web service using a Minimal API.

## Features

* Orleans integration using Grains and Silos.
* Persistent state to save the shortened URLs.
* Web service endpoints to create and redirect shortened URLs.

## Getting Started

### Prerequisites

- [.NET 6.0](http://dotnet.microsoft.com)

### Quickstart

1. Run the command `git clone https://github.com/Azure-Samples/build-your-first-orleans-app-aspnetcore.git` in your preferred terminal to clone the project to a folder on your computer.
1. Run the command `cd build-your-first-orleans-app-aspnetcore/OrleansURLShortener` to navigate down into the correct project folder.
1. Run the command `dotnet run` from your editor or terminal. The app should build and launch in the browser, and then display a `Hello World` message to verify it is working correctly.
1. Open the project in your preferred editor to begin modifying the project. There are two other existing endpoints at `/shorten` and `/go` to create and use redirects.
