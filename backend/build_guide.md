# Build Guide for ports of the backend

All commands are to be executed in the root of the project.

## MacOS (Silicon) build

### Step 1

public to application

```shell 
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```
  
### Step 2

copy /publish to root

Note: you may need to change paths, depending on your system

```shell
mkdir -p MyAppInstaller/MyApp
cp -r bin/Release/net9.0/osx-arm64/publish MyAppInstaller/MyApp
```