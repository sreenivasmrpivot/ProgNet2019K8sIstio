# Lab-02 (Containerize GiftShop Application using Docker locally)

## Prerequisites

1. Copy all the contents from ProgNet2019K8sIstio/Lab-01/End to ProgNet2019K8sIstio/Lab-02/Begin

## Dockerize GiftShopAPI

1. Create a Dockerfile.GiftShopAPI in the root of the Begin folder
2. Add the following contents to the Dockerfile.GiftShopAPI . The docker file has lot of comments to be self explanatory.

<sub><sup>*Dockerfile.GiftShopAPI --> ProgNet2019K8sIstio/Lab-02/Begin/Dockerfile.GiftShopAPI*</sup></sub>
``` dockerfile
# This is a multi-stage build docker file.
#  1. The last stage image only exists post building the docker. 
#     All stages prior to the last stage are cleaned up.
#  2. Different stages can access the folder / files from previous stage.

# Stage 1 - Build image which has all the SDK and tools to build the code
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
# ^^^ This the .net core build image 
WORKDIR /app
# ^^^ The WORKDIR defines the directory on the image which will be shared and accessible across different stages

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore GiftShopAPI/*.csproj

# Copy everything else and build
# COPY . ./
RUN dotnet publish GiftShopAPI/*.csproj -c Release -o out

# Stage 1 - Runtime image which 
#  1. Has just the .net core runtime
#  2. It does not have any SDK or build tools
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/GiftShopAPI/out .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "/app/GiftShopAPI.dll"]
```

3. Build the docker image for GiftShopAPI by executing the command 

``` bash
docker build -f Dockerfile.GiftShopAPI -t <username>-giftshopapi:1.0 .
```

4. Execute the below docker command to run the GiftShopAPI docker image locally

``` bash
docker run -d -p 5000:80 --name GiftShopAPISvc <username>-giftshopapi:1.0
```

5. Navigate to ```http://localhost:5000/swagger/index.html``` in your browser to view the GiftShopAPI running from docker


## Dockerize GiftShopUI

1. Create a Dockerfile.GiftShopUI in the root of the Begin folder
2. Add the following contents to the Dockerfile.GiftShopUI . The docker file has lot of comments to be self explanatory.

<sub><sup>*Dockerfile.GiftShopAPI --> ProgNet2019K8sIstio/Lab-02/Begin/Dockerfile.GiftShopAPI*</sup></sub>
``` dockerfile
# This is a multi-stage build docker file.
#  1. The last stage image only exists post building the docker. 
#     All stages prior to the last stage are cleaned up.
#  2. Different stages can access the folder / files from previous stage.

# Stage 1 - Build image which has all the SDK and tools to build the code
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
# ^^^ This the .net core build image 
WORKDIR /app
# ^^^ The WORKDIR defines the directory on the image which will be shared and accessible across different stages

# Copy csproj and restore as distinct layers
COPY . ./
RUN dotnet restore GiftShopUI/*.csproj

# Copy everything else and build
# COPY . ./
RUN dotnet publish GiftShopUI/*.csproj -c Release -o out

# Stage 1 - Runtime image which 
#  1. Has just the .net core runtime
#  2. It does not have any SDK or build tools
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/GiftShopUI/out .
EXPOSE 80 443
ENTRYPOINT ["dotnet", "/app/GiftShopUI.dll"]
```

3. Build the docker image for GiftShopUI by executing the command 

``` bash
docker build -f Dockerfile.GiftShopUI -t <username>-giftshopui:1.0 .
```

4. Execute the below docker command to run the GiftShopAPI docker image locally

``` bash
docker run -d -p 8000:80 --name GiftShopUISvc <username>-giftshopui:1.0
```

If you need to see exception stack in output from you docker container, you may have to use the below command to run your docker container.

``` bash
docker run -d -p 8000:80 -e ASPNETCORE_ENVIRONMENT="Development" --name GiftShopUISvc <username>-giftshopui
```

5. Navigate to ```http://localhost:8000/Home/``` in your browser to view the GiftShopAPI running from docker





**Note: The UI does not talk to API here as container to container networking is not enabled above.**


**If you have successfully completed this Lab, contents in your Begin and End folders would match.**