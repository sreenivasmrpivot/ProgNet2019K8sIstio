# Lab-03 (Tag and Push GiftShop Application to Harbor Container Registry)

In the previous Lab, we built docker images and ran the containers in the local development environment. However if you intend to provision your contianer in multiple environments such Dev, QA, Prod clusters, you would need a secure way of storing and retrieving the images you have created. This introduces the need for a repository for this purpose.

A Container Registry is a repository which helps store and retrive container images. Container images are stored in a registry by a push and retrieved using a pull. There are several container registries available in the market such as Dockerhub, Harbor, Azure Container Registry, Google Container Registry etc. Depending on their availability (External / Internet vs Internal / Intranet), they can be broadly classified into Public and Private registries. Many enterprises may need to store their container images internally due to 

1. Intellectual Property constraints
2. Presence of Sensitive information in the images
3. Need to prevent malware getting in 
4. Need to scan images for vulnerablities 
5. Need to regulate access using RBAC
6. Need to integrate with LDAP / AD / UAA etc
7. Need to sign and verify images using signature and Notary
8. Volume and frequency of image creation in Agile org can save resource by using local registries
9. Image replication across registries / regions to reduce latency

Harbor is an open source cloud native registry that stores, signs, and scans container images for vulnerabilities. Harbor solves common challenges by delivering trust, compliance, performance, and interoperability. It fills a gap for organizations and applications that cannot use a public or cloud-based registry, or want a consistent experience across clouds.

## Harbor Registry Organization

Harbor lets you group container images by *Project* and apply Role Based Access Control too at Project level. Enterprises may need to provide Harbor as service to several departments or teams and this is facilitated by the Project construct.

## Push an image to Harbor

Assuming you have built images for GiftShopAPI and GiftShopUI in the previous lab, you should be able to view those images by executing the below command.

``` bash
docker images
```

If you do not see your images, please execute the corresponding steps in the Lab-02.

Please find the Harbor project details for this lab below.

```
YOUR_HARBOR_FQDN = harbor.pksone.io
YOUR_HARBOR_PROJECT = prognet2019
```

Tag the images built by executing.

``` bash
# Tag the GiftShopAPI image
docker tag <username>-giftshopapi:1.0 <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopapi:1.0

# Tag the GiftShopUI image
docker tag <username>-giftshopui:1.0 <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopui:1.0
```

You may verify the existence of your tagged images locally by executing 

``` bash
docker images
```

**Note:** If you do not specify *1.0* above for version_tag, it defaults to the tag *latest*.

Until this point, the commands executed were local to your development machine. You will need to login to Harbor registry, to be able to access the Projects and operate. Execute the below command in your terminal to login to the harbor registry.

```
docker login https://<YOUR_HARBOR_FQDN>
```

You will be challenged with a username and password to login. Once you login, you may push your images to the Harbor registry using

``` bash
# Push the GiftShopAPI image
docker push <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopapi:1.0

# Push the GiftShopUI image
docker push <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopui:1.0
```

You may login to the Harbor regitry using the url https://<YOUR_HARBOR_FQDN> from your browser, navigate to <YOUR_HARBOR_PROJECT> and search for your images using your <username> in the search for "Filter Repositories".

## Pull an image from Harbor

You may pull an image from Harbor registry by executing the below command while logged-in

``` bash
# Pull the GiftShopAPI image
docker pull <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/giftshopapi:1.0

# Pull the GiftShopUI image
docker pull <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/giftshopui:1.0
```

However you may not need to pull images individually as shown above. We will see how the kubernetes clusters pull the image in the upcoming labs.

## Logout of Harbor

Use this if you need to clear docker credentials 

``` bash
docker logout https://harbor.pksone.io
```

## Resources

* [Harbor](https://goharbor.io/)

