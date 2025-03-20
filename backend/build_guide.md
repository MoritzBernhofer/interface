# Build Guide for ports of the backend

All commands are to be executed in the root of the project.

## MacOS (Silicon) build

### Step 1

public to application

```shell 
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

### Step 2

copy /publish to project root

Note: you may need to change paths, depending on your system

```shell
mkdir -p InterfaceInstaller/Interface
cp -r bin/Release/net9.0/osx-arm64/publish/ InterfaceInstaller/Interface/
```

### Step 3

copy plist into the publish folder

```shell
cp ./interface.plist InterfaceInstaller/Interface/
```

### Step 4

build the .pkg

```shell
pkgbuild --root ./InterfaceInstaller/Interface \
--identifier moritz.bernhofer.interface \
--version 1.0.0 \
--install-location /Applications/Interface \
--scripts ./InterfaceInstaller/scripts \
interface.pkg
```
