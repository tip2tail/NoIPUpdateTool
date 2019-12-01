# NoIPUpdateTool

A quick and dirty NoIP dynamic DNS update tool.

Provided with no warranties, expressed or implied!

_It works on my machine!_

~~~~
A tool to update the NoIP Dynamic DNS system via API calls

Usage: NoIPUpdateTool [options] [command]

Options:
  -?|-h|--help   Show help information

Commands:
  about          Displays the about informaition for this tool
  list-settings  Shows the settings file in JSON format
  set-creds      Updates the username and password settings
  set-hosts      Sets the list of hostnames for updating on each execution
  set-interval   Sets the interval of executions before a forced update
  update         Runs the NoIP API update process

Run 'NoIPUpdateTool [command] --help' for more information about a command.
~~~~

## Build Instructions

### Linux x64

`~/repos/NoIPUpdateTool$ dotnet publish -c Release -r linux-x64`

### Windows x64

`C:\repos\NoIPUpdateTool> dotnet publish -c Release -r win-x64`
