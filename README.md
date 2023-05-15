[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/fedeantuna/clean-solution-template/build.yml?style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/.github/workflows/build.yml)
[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/fedeantuna/clean-solution-template/package.yml?label=package\&style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/.github/workflows/package.yml)
[![Codacy Code Analysis](https://img.shields.io/codacy/grade/ff9e3c8e39824582be03f19769d3b6ad?style=flat-square)](https://www.codacy.com/gh/fedeantuna/clean-solution-template/dashboard?utm_source=github.com\&utm_medium=referral\&utm_content=fedeantuna/clean-solution-template\&utm_campaign=Badge_Grade)
[![Codacy Code Coverage](https://img.shields.io/codacy/coverage/ff9e3c8e39824582be03f19769d3b6ad?style=flat-square)](https://www.codacy.com/gh/fedeantuna/clean-solution-template/dashboard?utm_source=github.com\&utm_medium=referral\&utm_content=fedeantuna/clean-solution-template\&utm_campaign=Badge_Coverage)
[![NuGet](https://img.shields.io/nuget/v/CleanSolutionTemplate?style=flat-square)](https://www.nuget.org/packages/CleanSolutionTemplate/)
[![GitHub](https://img.shields.io/github/license/fedeantuna/clean-solution-template?style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/LICENSE)

# Clean Solution Template

This template is based on Jason Taylor's [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)

Icon design credit goes to [@antunamirna](https://www.instagram.com/antunamirna/)

## Getting started

The most simple way to obtain this template is by installing the NuGet package.

First make sure that you have the [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) installed on your machine.

Then run `dotnet new -i CleanSolutionTemplate` to install the template.

At this moment, the supported way to create a solution using this template is from the command line. Using the template from an IDE (like Visual Studio or Rider) is not supported. To create a solution using this template simply run `dotnet new cst -n <SolutionName>`

Don't forget to change this README accordingly to your project and to review the LICENSE.

## Running PostgresSQL locally

The easiest and simplest way to run a local DB is using Docker, the connection string for the PostgresSQL DB in the `appsettings.Development.json` will allow you to connect to the container that gets created by running `docker run --name cst-postgres -e POSTGRES_PASSWORD=password -p 5432:5432 -d postgres:15.3-alpine3.18`

## Static Analysis and Online Coverage Report

This template uses Codacy as the tool for code analysis and reporting on test coverage. In order to set it up follow the steps described in the docs:

*   https://docs.codacy.com/getting-started/codacy-quickstart/
*   https://docs.codacy.com/coverage-reporter/
*   https://docs.codacy.com/repositories-configure/integrations/github-integration/#configuring
*   https://docs.codacy.com/faq/general/how-do-i-block-merging-prs-using-codacy-as-a-quality-gate/

## Local Coverage Report

Running the coverage report locally can be done by executing the test coverage scripts inside the `scripts` directory. Both are the same script, one written in Powershell and the other one in Bash. These scripts make use of the report-generator tool, please visit the [ReportGenerator GitHub](https://github.com/danielpalme/ReportGenerator) to know more about the project.

## Structure Overview

The project is divided as follows:

*   The `scripts` directory is where all scripts should be placed. By default four come with the template to run the tests and generate a coverage and mutation report locally.
*   The `src` directory is where all the source code should be placed. By default four projects are included here. The Api project is the default Presentation Layer, then we have the Application, Domain and Infrastructure layers.
*   The `tests` directory is where all the code for the tests should be placed. By default four Unit Test projects corresponding to a `src` project, an Integration Test and an End to End Test are placed here.

## Layers Overview

### Domain

This will contain all entities, enums, exceptions, interfaces, types and logic specific to the domain layer.

### Application

This layer contains all application logic. It is dependent on the domain layer, but has no dependencies on any other layer or project. This layer defines interfaces that are implemented by outside layers. For example, if the application need to access a notification service, a new interface would be added to application and an implementation would be created within infrastructure.

### Infrastructure

This layer contains classes for accessing external resources such as file systems, web services, smtp, and so on. These classes should be based on interfaces defined within the application layer.

### Presentation

This layer depends on both the Application and Infrastructure layers, however, the dependency on Infrastructure is only to support dependency injection. Therefore only Startup.cs should reference Infrastructure.

## Tests Overview

### Unit Tests

Our definition of Unit Test is a test that takes a layer in isolation and mocks every external dependency or those that belongs to a different layer. We rely on DI to obtain the SUTs and the configuration for it can be found in the ServiceProviderBuilder class. These tests will run isolated from one another, making xUnit a great tool for the job. No test should depend on the execution or state of a previous test. Everything that relates to the database will be done using an InMemory Database.

### Integration Tests

Our definition of Integration Test is a test that takes the Domain, Application and Infrastructure layers and mocks only external dependencies. We rely on DI to obtain the SUTs and the configuration for it can be found in the ServiceProviderBuilder class. These tests will run sequentially, each class is allowed to keep state and rely on the state of the previous execution, making NUnit a great tool for the job. Tests within a class shouldn't depend on the execution or state of tests in different classes. Everything that relates to the database will be run within a Docker container using a real database.

### End to End Tests

Our definition of End to End Test is a test that takes all the layers and external dependencies. We do not mock. We rely on a Test Server to run our API. These tests will run sequentially, each class is allowed to keep state and rely on the state of the previous execution, making NUnit a great tool for the job. Tests within a class shouldn't depend on the execution or state of tests in different classes. Everything that relates to the database will be run against a database environment for development/testing, separated from production but as close as possible to it.

## Problems or Suggestions

If you run into any issue or have an idea to improve the template, simply create an issue on this repo explaining what the problem/suggestion is.

## License

This project is licensed with the [MIT license](https://github.com/fedeantuna/clean-solution-template/blob/main/LICENSE).
