# Lab-10 (Enable Https Ingress)

## Pre-requisites

[Installing the HELM client](https://helm.sh/docs/using_helm/#installing-helm)

## Installing the cert tools

1. Versions of Helm, prior to 3.0 has a client side component and a server side component. Enable tiller by executing the below command

``` bash
kubectl apply -f helm-rbac.yaml
```

If you carefully observe, you have not passed in the namespace using -n parameter here. It is because the helm-rbac.yaml file has the namespace defined in it.

<sub><sup>*helm-rbac.yaml --> ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests/helm-rbac.yaml*</sup></sub>
``` yaml
apiVersion: v1
kind: ServiceAccount
metadata:
  name: tiller
  namespace: kube-system
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: tiller
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: cluster-admin
subjects:
  - kind: ServiceAccount
    name: tiller
    namespace: kube-system
```

2. Execute ```helm init --service-account tiller --upgrade``` to get the helm and tiller working

3. Execute helm version and ensure you have both the server and client versions displayed in the output as shown below.

``` bash
helm version
Client: &version.Version{SemVer:"v2.14.2", GitCommit:"a8b13cc5ab6a7dbef0a58f5061bcc7c0c61598e7", GitTreeState:"clean"}
Server: &version.Version{SemVer:"v2.14.2", GitCommit:"a8b13cc5ab6a7dbef0a58f5061bcc7c0c61598e7", GitTreeState:"clean"}
```

4. Install cert manager using https://hub.helm.sh/charts/jetstack/cert-manager

Supply additional parameters to the last command in the above link as shown below.

```helm install --name prognet2019-cert-manager --namespace cert-manager jetstack/cert-manager --set ingressShim.defaultIssuerName=letsencrypt-staging --set ingressShim.defaultIssuerKind=ClusterIssuer```

5. Now you should be able to see the cert-manager chart installed, if you execute 

``` bash
helm ls
NAME                    	REVISION	UPDATED                 	STATUS  	CHART              	APP VERSION	NAMESPACE
prognet2019-cert-manager	1       	Thu Aug 29 22:32:47 2019	DEPLOYED	cert-manager-v0.9.1	v0.9.1     	cert-manager
```

6. If you wish to delete the helm chart for any reason, use 

``` bash
helm del --purge prognet2019-cert-manager
```

7. Create CA Cluster Issuer using the command

``` bash
kubectl apply -f CreateCAClusterIssuer.yaml -n cert-manager
```

ClusterIssuer provides certs across all namespaces in a Cluster. It is a cluster wide operation. So if it is already existing in the cluster, you need create it again. Find the ClusterIssuer definition below.

<sub><sup>*CreateCAClusterIssuer.yaml --> ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests/CreateCAClusterIssuer.yaml*</sup></sub>
``` yaml
apiVersion: certmanager.k8s.io/v1alpha1
kind: ClusterIssuer
metadata:
  name: demo-staging
spec:
  acme:
    server: https://acme-staging-v02.api.letsencrypt.org/directory
    email: srajaram@pivotal.io
    privateKeySecretRef:
      name: demo-staging
    http01: {}
```

8. Create Certificate Object using the below command 

``` bash
kubectl apply -f CreateCertificateObject.yaml -n cert-manager
```

The CertificateObject definition maybe found here

<sub><sup>*CreateCertificateObject.yaml --> ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests/CreateCertificateObject.yaml*</sup></sub>
``` yaml
apiVersion: certmanager.k8s.io/v1alpha1
kind: Certificate
metadata:
  name: tls-secret
spec:
  secretName: tls-secret
  dnsNames:
  - <username>-giftshopapi.<user's domain>
  - <username>-giftshopui.<user's domain>
  acme:
    config:
    - http01:
        ingressClass: none
      domains:
      - <username>-giftshopapi.<user's domain>
      - <username>-giftshopui.<user's domain>
  issuerRef:
    name: demo-staging
    kind: ClusterIssuer
```

## Deploy the GiftShop K8s manifests

1. In the previous Lab, we changed the ConfigMaps from container environment based to volume based and left it at a troubleshooting point. You may complete it as an exercise later. For now lets copy the K8s manifests from ProgNet2019K8sIstio/Lab-09/GiftShop/K8s-Manifests to ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests
2. Rollback the version of image and deployment to 4.0 / v4, comment out volumes based ConfigMap and uncomment container env based ConfigMap. Find the updated GiftShopUI.yaml below.

<sub><sup>*CreateCertificateObject.yaml --> ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests/CreateCertificateObject.yaml*</sup></sub>
``` yaml
apiVersion: apps/v1beta1
kind: Deployment
metadata:
  name: giftshopui-v4
spec:
  replicas: 1
  template:
    metadata:
      labels:
        app: giftshopui
        tier: frontend
        version: v4
    spec:
      containers:
      - name: giftshopui
        image: <YOUR_HARBOR_FQDN>/<YOUR_HARBOR_PROJECT>/<username>-giftshopui:4.0
        imagePullPolicy: Always
        resources:
          limits:
            memory: "128Mi"
            cpu: "500m"
        ports:
        - containerPort: 80
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Development"
        - name: ExternalDependencies_GiftShopAPI_BaseUrl
          valueFrom:
            configMapKeyRef:
              name: giftshopui-config
              key: GiftShopAPIBaseUrl
        # volumeMounts:
        # - name: giftshopui-config
        #   mountPath: /app/GiftShopUI/out/AppConfig.json
        #   subPath: AppConfig.json
      imagePullSecrets:
        - name:  <username>harborsecret
      # volumes:
      # - name: giftshopui-config
      #   configMap:
      #       name: giftshopui-config
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

3. Deploy the updated GiftShopUI using

``` bash
kubectl apply -f GiftShopUI.yaml -n <username>-ns
```

## Deploy the GiftShop Istio manifests

1. Copy ProgNet2019K8sIstio/Lab-09/GiftShop/Istio-Manifests/GiftShop-Ingress-Http.yaml to ProgNet2019K8sIstio/Lab-10/GiftShop/Istio-Manifests/GiftShop-Ingress-Http.yaml
2. Rename ProgNet2019K8sIstio/Lab-10/GiftShop/Istio-Manifests/GiftShop-Ingress-Http.yaml to ProgNet2019K8sIstio/Lab-10/GiftShop/Istio-Manifests/GiftShop-Ingress-Https.yaml
3. Update the http port section with https as shown below.

Replace 

``` yaml
  - port:
      number: 80
      name: http
      protocol: HTTP
    hosts:
    - <username>-giftshopapi.<user's domain>
    - <username>-giftshopui.<user's domain>
```

with

``` yaml
  - port:
      number: 443
      name: https
      protocol: HTTPS
    tls:
      mode: SIMPLE
      serverCertificate: /etc/istio/ingressgateway-certs/tls.crt
      privateKey: /etc/istio/ingressgateway-certs/tls.key
    hosts:
    - <username>-giftshopapi.<user's domain>
    - <username>-giftshopui.<user's domain>
```

4. Deploy the GiftShop-Ingress-Https.yaml to <username>-ns by executing the below command from ProgNet2019K8sIstio/Lab-10/Istio-Manifests folder.

``` bash
kubectl apply -f GiftShop-Ingress-Http.yaml -n <username>-ns
```

5. Now you should be able to access both GiftShopAPI and GiftShopUI over https

6. If you wish to expose the GiftShopAPI and GiftShopUI over http and https, you may have both http and https ports in the Ingress manifest as shown below 

<sub><sup>*GiftShop-Ingress.yaml --> ProgNet2019K8sIstio/Lab-10/GiftShop/K8s-Manifests/GiftShop-Ingress.yaml*</sup></sub>
``` yaml
  - port:
      number: 443
      name: https
      protocol: HTTPS
    tls:
      mode: SIMPLE
      serverCertificate: /etc/istio/ingressgateway-certs/tls.crt
      privateKey: /etc/istio/ingressgateway-certs/tls.key
    hosts:
    - <username>-giftshopapi.<user's domain>
    - <username>-giftshopui.<user's domain>
  - port:
      number: 80
      name: http
      protocol: HTTP
    hosts:
    - <username>-giftshopapi.<user's domain>
    - <username>-giftshopui.<user's domain>
```
