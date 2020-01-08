# Interactive C# Documentation

Interactive documenation for the C# API is provided using [Try .NET](https://dotnet.microsoft.com/platform/try-dotnet) 
a tool which allows C# code to be run in a browser using Markdown files.

The interactive Markdown files need to be setup and run locally using the steps in the sections below.

#### Setup

* Follow the setup requirements of Try .NET found in the [Setup section of project GitHub page](https://github.com/dotnet/try/blob/master/DotNetTryLocal.md).
* Clone the [cmdty/time-period-value-types](https://github.com/cmdty/time-period-value-types) repository to your local machine.


#### Running

* Open a CLI locally and set the current directory to the /samples directory within the cloned repository.
* In the CLI run the following command:
```
> dotnet try
```

This Markdown file should now be automatically opened in a browser. From this newly opened page, use the following links to view and run the interactive docs.
* [Time Period Value Types](TimePeriodValueTypes.md).
