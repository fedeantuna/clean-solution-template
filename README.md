[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/fedeantuna/clean-solution-template/Build?style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/.github/workflows/build.yml)
[![GitHub Workflow Status](https://img.shields.io/github/workflow/status/fedeantuna/clean-solution-template/Package?label=package&style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/.github/workflows/package.yml)
[![Mutation testing badge](https://img.shields.io/endpoint?style=flat-square&url=https%3A%2F%2Fbadge-api.stryker-mutator.io%2Fgithub.com%2Ffedeantuna%2Fclean-solution-template%2Fmain)](https://dashboard.stryker-mutator.io/reports/github.com/fedeantuna/clean-solution-template/main)
[![NuGet](https://img.shields.io/nuget/v/CleanSolutionTemplate?style=flat-square)](https://www.nuget.org/packages/CleanSolutionTemplate/)
[![GitHub](https://img.shields.io/github/license/fedeantuna/clean-solution-template?style=flat-square)](https://github.com/fedeantuna/clean-solution-template/blob/main/LICENSE)

# Clean Solution Template

This template is based on Jason Taylor's [Clean Architecture Template](https://github.com/jasontaylordev/CleanArchitecture)

Icon design credit goes to [@antunamirna](https://www.instagram.com/antunamirna/)

## Installation

The most simple way to obtain this template is by installing the NuGet package.

First make sure that you have the [.NET 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0) installed on your machine.

Then run `dotnet new -i CleanSolutionTemplate` to install the template.

At this moment, the supported way to create a solution using this template is from the command line. Using the template from an IDE (like Visual Studio or Rider) is not supported. To create a solution using this template simply run `dotnet new cst -n <SolutionName>`

In order to run the Integration Tests there are a few steps needed before.

1. If not installed already, install [Docker](https://docker.com)
2. Run the following to get the needed Docker Images:
    - ``docker pull testcontainers/ryuk:0.3.4``
    - ``docker pull fedeantuna/test-identity-server:v1.0.1``

## Static Analysis, Online Coverage Report and Online Mutation Report

This template uses DeepSource as the tool for code analysis and reporting on test coverage. In order to set it up, you need to create an account at https://deepsource.io/ and then allow access to your repository. Once that is done, you need to add the DSN to your repository secrets under the name DEEPSOURCE_DSN. You can take a look here https://deepsource.io/docs/dashboard/repo-settings/#dsn to know more about how to do it.

For the Mutation Report this template uses Stryker. You need to sign in with your GitHub Account into https://dashboard.stryker-mutator.io/ and then allow access to your repository. Once that is done follow the instructions on how to set up the corresponding secrets. Remember to modify the `stryker-config.json`files that are in each Source Project to match the name with your actual repository.

## Local Mutation Report

Running the mutation report locally can be done by executing the stryker scripts inside the `scripts` directory. Both are the same script, one written in Powershell and the other one in Bash. These scripts make use of the stryker tool, pleace visit the [Stryker Web](https://stryker-mutator.io/docs/stryker-net/introduction/) to know more about the project.

## Local Coverage Report

Running the coverage report locally can be done by executing the test coverage scripts inside the `scripts` directory. Both are the same script, one written in Powershell and the other one in Bash. These scripts make use of the report-generator tool, please visit the [ReportGenerator GitHub](https://github.com/danielpalme/ReportGenerator) to know more about the project.

## Structure Overview

The project is divided as follows

```
.
|__ CleanSolutionTemplate.sln
|
|__ scripts
|   |__ run_stryker.ps1
|   |__ run_stryker.sh
|   |__ run_test_coverage.ps1
|   |__ run_test_coverage.sh
|
|__ src
|   |__ CleanSolutionTemplate.Api
|       |__ ...
|       |__ stryker-config.json
|   |__ CleanSolutionTemplate.Application
|       |__ ...
|       |__ stryker-config.json
|   |__ CleanSolutionTemplate.Domain
|       |__ ...
|       |__ stryker-config.json
|   |__ CleanSolutionTemplate.Infrastructure
|       |__ ...
|       |__ stryker-config.json
|
|__ tests
|   |__ CleanSolutionTemplate.Api.Tests.Unit
|   |__ CleanSolutionTemplate.Application.Tests.Unit
|   |__ CleanSolutionTemplate.Domain.Tests.Unit
|   |__ CleanSolutionTemplate.Infrastructure.Tests.Unit
|   |__ CleanSolutionTemplate.Tests.EndToEnd
|   |__ CleanSolutionTemplate.Tests.Integration
|
|__ LICENSE
|
|__ README.md
```

The `scripts` directory is where all scripts should be placed. By default four come with the template to run the tests and generate a coverage and mutation report locally.

The `src` directory is where all the source code should be placed. By default four projects are included here. The Api project is the default Presentation Layer, then we have the Application, Domain and Infrastructure layers.

The `tests` directory is where all the code for the tests should be placed. By default four Unit Test projects corresponding to a `src` project, an Integration Test and an End to End Test are placed here.

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

Our definition of Unit Test is a test that takes a layer in isolation and mocks every external dependency or those that belongs to a different layer. We rely on DI to obtain the SUTs and the configuration for it can be found in the TestBase class. These tests will run isolated from one another, making xUnit a great tool for the job. No test should depend on the execution or state of a previous test. Everything that relates to the database will be done using an InMemory Database.

### Integration Tests

Our definition of Integration Test is a test that takes all the layers and works only mocking external dependencies. We rely on DI to obtain the SUTs and the configuration for it can be found in the TestBase class. These tests will run sequentially, each class is allowed to keep state and rely on the state of the previous execution, making NUnit a great tool for the job. Tests within a class shouldn't depend on the execution or state of tests in different classes. Everything that relates to the database will be run within a Docker container using a real database.

### End to End Tests

Our definition of End to End Test is a test that takes all the layers and external dependencies. We do not mock. We rely on a Test Server to run our API. These tests will run sequentially, each class is allowed to keep state and rely on the state of the previous execution, making NUnit a great tool for the job. Tests within a class shouldn't depend on the execution or state of tests in different classes. Everything that relates to the database will be run against a database environment for development/testing, separated from production but as close as possible to it.

## Problems or Suggestions

If you run into any issue or have an idea to improve the template, simply create an issue on this repo explaining what the problem/suggestion is.

## License

This project is licensed with the [MIT license](https://github.com/fedeantuna/clean-solution-template/blob/main/LICENSE).
