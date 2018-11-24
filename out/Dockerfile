# Base image
FROM microsoft/dotnet:2.1-sdk

# Change working directory
WORKDIR /app

# Get all the files!
COPY . .

# Let Heroku define the port
CMD ASPNETCORE_URLS=http://*:$PORT dotnet comet-climate-server.dll