# Lab-06 (Deploying GiftShop Application to Pivotal Kubernetes Service)

## Create Harbor secret in your Namespace

As previously mentioned in the Lab-03, Harbor is a private registry and requires credentials to push or pull images from the registry. For kubernetes to be able to pull the GiftShop images from Harbor, we need to create a Kubernetes secret which can hold the Harbor credentials. Execute the below command to create a harbor secret for your namespace. 

``` bash
kubectl create secret docker-registry <username>harborsecret --docker-server=https://harbor.pksone.io --docker-username=<HARBOR_USER_NAME> --docker-password=<HARBOR_PASSWORD>
```

*Note: Create the above secret in the same namespace as your microservices*

## Deploy GiftShopAPI to PKS

The GiftShopAPI requires a service and a deployment definition to be deployed to the Kubernetes cluster. You may find these definitions for the GiftShopAPI in the GiftShopAPI.yaml file.

<sub><sup>*GiftShopAPI.yaml --> Pivotal/ProgNet2019K8sIstio/Lab-06/K8s-Manifests/GiftShopAPI.yaml*</sup></sub>

``` yaml
apiVersion: apps/v1beta1
kind: Deployment
metadata:
  name: giftshopapi-v1
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: giftshopapi
        tier: backend
        version: v1
    spec:
      containers:
      - name: giftshopapi
        image: <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopapi:1.0
        imagePullPolicy: Always
        resources:
          limits:
            memory: "128Mi"
            cpu: "500m"
        ports:
        - containerPort: 80
      imagePullSecrets:
        - name:  <username>harborsecret
---
apiVersion: v1
kind: Service
metadata:
  name: giftshopapi
spec:
  #type: LoadBalancer #uncomment this to directly expose with regular Kubernetes without Istio
  selector:
    app: giftshopapi
  ports:
  - port: 80
    name: http-giftshopapi  # Should start with http- for Istio, Jager and Prometheus to trace requests
```

To create the service and deployment defined in the GiftShopAPI.yaml, execute 

``` bash 
kubectl apply -f GiftShopAPI.yaml -n <username>-ns
```

Verify if the service was created successfully by executing the below command.

``` bash
kubectl get svc -n <username>-ns

NAME          TYPE        CLUSTER-IP       EXTERNAL-IP   PORT(S)   AGE
giftshopapi   ClusterIP   10.100.200.155   <none>        80/TCP    4h9m

```

You do not see an External-IP as ```type: LoadBalancer``` is commented out in the service definition. If you uncomment the ```type: LoadBalancer``` and redeploy the service, you would see an External-IP address assigned to your service.

``` 
Note: However you may be able to access the IP address only if you are on the Pivotal VPN, if your platform is installed on PEZ.
But if you have your Pivotal Platform installed on a public cloud or environment completely controlled by you, you will be able to access the External-IP without VPN.
```

The GiftShopAPI code would actually be deployed as a docker container within a Pod backing the services. You may verify if the GiftShopAPI pods are working by executing the below command.

``` bash
kubectl get pods -n <username>-ns | grep 'giftshopapi'

giftshopapi-v1-6dd4854b7d-htmmz   1/1     Running   0          4h36m
```

### Scaling GiftShopAPI to 2 instances

In case you need any modifications to be made to the service or deployment defined in GiftShopAPI.yaml, you may make changes and re-execute

``` bash 
kubectl apply -f GiftShopAPI.yaml -n <username>-ns
```

as kubernetes works are a state machine, it ensure the current state is always matching the desired state.

Update the GiftShopAPI.yaml with 

``` yaml
  replicas: 2
```

and re-execute

``` bash 
kubectl apply -f GiftShopAPI.yaml -n <username>-ns
```

Verify if the number of instances is change to 2 by executing the below

``` bash
kubectl get pods -n <username>-ns | grep 'giftshopapi'

giftshopapi-v1-6dd4854b7d-htmmz   1/1     Running   0          26h
giftshopapi-v1-6dd4854b7d-v5wbp   1/1     Running   0          24s
```

### Deleting GiftShopAPI 

If you wish to delete the GiftShopAPI service and deployment, you may do so by executing 

``` bash 
kubectl delete -f GiftShopAPI.yaml -n <username>-ns
```

## Deploy GiftShopAPI to PKS

Similar to GiftShopAPI, GiftShopUI also requires a service and a deployment definition to be deployed to the Kubernetes cluster. You may find these definitions for the GiftShopUI in the GiftShopUI.yaml file.

<sub><sup>*GiftShopUI.yaml --> Pivotal/ProgNet2019K8sIstio/Lab-06/K8s-Manifests/GiftShopUI.yaml*</sup></sub>

``` yaml
apiVersion: apps/v1beta1
kind: Deployment
metadata:
  name: giftshopui-v1
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: giftshopui
        tier: frontend
        version: v1
    spec:
      containers:
      - name: giftshopui
        image: <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopui:1.0
        imagePullPolicy: Always
        resources:
          limits:
            memory: "128Mi"
            cpu: "500m"
        ports:
        - containerPort: 80
      imagePullSecrets:
        - name:  <username>harborsecret
---
apiVersion: v1
kind: Service
metadata:
  name: giftshopui
spec:
  #type: LoadBalancer #uncomment this to directly expose with regular Kubernetes without Istio
  selector:
    app: giftshopui
  ports:
  - port: 80
    name: http-giftshopui  # Should start with http- for Istio, Jager and Prometheus to trace requests
```

To create the service and deployment defined in the GiftShopUI.yaml, execute 

``` bash 
kubectl apply -f GiftShopUI.yaml -n <username>-ns
```

Updating and deleting follows the similar pattern as described in the GiftShopAPI section, with file name changed to GiftShopUI.yaml. 