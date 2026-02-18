#!/bin/sh
# Get the PID of the process
pid=$(pgrep -f "dotnet oliejournal.cli.dll audioprocessqueue")

# Check if the process exists
if [ -n "$pid" ]; then
  # Terminate the process
  kill "$pid"
  echo "Process with PID $pid terminated."
else
  echo "Process 'dotnet oliejournal.cli.dll audioprocessqueue' not found."
fi
