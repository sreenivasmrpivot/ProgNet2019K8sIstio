## Points to remember

1. Use istioctl dashboard commands to bring up Kiali, Jaeger, Prometheus, Grafana etc
  1. istioctl dashboard grafana
  2. istioctl dashboard prometheus
  3. istioctl dashboard jaeger
  4. istioctl dashboard kiali
  5. istioctl dashboard envoy <pod-name>.<namespace>
2. Ideally you will not see any logs in the Jaeger thats because PILOT_TRACE_SAMPLING is set to 1% you can increase it during install or you may change it after install using the instructions towards the end of this page https://preliminary.istio.io/docs/tasks/observability/distributed-tracing/overview/
3. Out of the box Kiali shows calls between all the virtual services which are marked as "VirtualService" or "ServiceEntry"





















# MTLS Tips and Tricks

## Check mtls policy between 2 pods

``` bash
istio-1.3.3 $istioctl authn tls-check giftshopui-v6-66fbd6b784-qcb6s.microservices giftshopapi.microservices.svc.cluster.local

HOST:PORT                                          STATUS     SERVER     CLIENT           AUTHN POLICY              DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         STRICT     ISTIO_MUTUAL     microservices/default     microservices/enable-mtls
```


## Ensure there are certs installed in the envoy container of the pod

``` bash

istio-1.3.3 $kubectl exec giftshopui-v6-66fbd6b784-qcb6s -c istio-proxy -- ls /etc/certs

cert-chain.pem
key.pem
root-cert.pem

istio-1.3.3 $kubectl get pods

NAME                              READY   STATUS    RESTARTS   AGE
giftshopapi-v2-6ddd8b6668-gst48   2/2     Running   0          3h
giftshopui-v6-66fbd6b784-qcb6s    2/2     Running   0          3h

istio-1.3.3 $kubectl exec giftshopapi-v2-6ddd8b6668-gst48 -c istio-proxy -- ls /etc/certs

cert-chain.pem
key.pem
root-cert.pem

```


## Ensure validity of certs

``` bash

istio-1.3.3 $kubectl exec giftshopui-v6-66fbd6b784-qcb6s -c istio-proxy -- cat /etc/certs/cert-chain.pem | openssl x509 -text -noout  | grep Validity -A 2
        Validity
            Not Before: Oct 13 15:29:15 2019 GMT
            Not After : Jan 11 15:29:15 2020 GMT
istio-1.3.3 $kubectl exec giftshopapi-v2-6ddd8b6668-gst48 -c istio-proxy -- cat /etc/certs/cert-chain.pem | openssl x509 -text -noout  | grep Validity -A 2
        Validity
            Not Before: Oct 13 15:29:15 2019 GMT
            Not After : Jan 11 15:29:15 2020 GMT

```


## Traffic sniffing

https://blog.getambassador.io/verifying-service-mesh-tls-in-kubernetes-using-ksniff-and-wireshark-454b1e3f4dc9
https://github.com/kubernetes-sigs/krew
https://www.wireshark.org/#download

kubectl sniff giftshopui-v1-ccb98fd97-rspgk -c istio-proxy -p -n microservices

kubectl sniff giftshopapi-v1-7554fc7cf9-p6jwn -c istio-proxy -p -n microservices


## Before enabling mtls

### This is the default state

``` bash

istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER         CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         PERMISSIVE     -          /default         -

istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER         CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         PERMISSIVE     -          /default         -

```

**UI works end to end with API, API swagger works**













## Mesh wide policy

### Just MeshPolicy alone

``` yaml

apiVersion: "authentication.istio.io/v1alpha1"
kind: "MeshPolicy"
metadata:
  name: "default"
spec:
  peers:
  - mtls: {}

```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER     CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         STRICT     -          /default         -
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER     CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         STRICT     -          /default         -

```

**UI works end to end with API, API swagger works  from browser**


### Just Mesh level DestinationRule alone


``` yaml

apiVersion: "networking.istio.io/v1alpha3"
kind: "DestinationRule"
metadata:
  name: "default"
  namespace: "istio-system"
spec:
  host: "*.local"
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL

```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS       SERVER      CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     CONFLICT     DISABLE     ISTIO_MUTUAL     -                istio-system/default
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS       SERVER      CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     CONFLICT     DISABLE     ISTIO_MUTUAL     -                istio-system/default

```

**UI broken, API swagger broken  from browser**



### Both MeshPolicy and Mesh level DestinationRule

``` yaml

apiVersion: "authentication.istio.io/v1alpha1"
kind: "MeshPolicy"
metadata:
  name: "default"
spec:
  peers:
  - mtls: {}
---
apiVersion: "networking.istio.io/v1alpha3"
kind: "DestinationRule"
metadata:
  name: "default"
  namespace: "istio-system"
spec:
  host: "*.local"
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL


```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER     CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         STRICT     ISTIO_MUTUAL     /default         istio-system/default
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER     CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         STRICT     ISTIO_MUTUAL     /default         istio-system/default

```



**UI broken, API swagger broken  from browser**






































### This is after applying DestinationRule


``` yaml

apiVersion: "networking.istio.io/v1alpha3"
kind: "DestinationRule"
metadata:
  name: "giftshopui-istio-client-mtls"
  namespace: microservices
spec:
  host: giftshopui.microservices.svc.cluster.local
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL

```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER         CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         PERMISSIVE     -          /default         -
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER         CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         PERMISSIVE     ISTIO_MUTUAL     /default         microservices/giftshopui-istio-client-mtls

```

**UI broken, API swagger works**


``` yaml

apiVersion: "networking.istio.io/v1alpha3"
kind: "DestinationRule"
metadata:
  name: "giftshopapi-istio-client-mtls"
  namespace: microservices
spec:
  host: giftshopapi.microservices.svc.cluster.local
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL

```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER         CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         PERMISSIVE     ISTIO_MUTUAL     /default         microservices/giftshopapi-istio-client-mtls
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER         CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         PERMISSIVE     -          /default         -

```

**UI works end to end with API, API swagger broken**





``` yaml

apiVersion: "networking.istio.io/v1alpha3"
kind: "DestinationRule"
metadata:
  name: "all-istio-client-mtls"
  namespace: microservices
spec:
  host: *.microservices.svc.cluster.local
  trafficPolicy:
    tls:
      mode: ISTIO_MUTUAL

```

``` bash

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-rspgk.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER         CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         PERMISSIVE     ISTIO_MUTUAL     /default         microservices/all-istio-client-mtls
~ $istioctl authn tls-check giftshopapi-v1-7554fc7cf9-p6jwn.microservices giftshopui.microservices.svc.cluster.local
HOST:PORT                                         STATUS     SERVER         CLIENT           AUTHN POLICY     DESTINATION RULE
giftshopui.microservices.svc.cluster.local:80     OK         PERMISSIVE     ISTIO_MUTUAL     /default         microservices/all-istio-client-mtls

```

**UI broken, API swagger broken**





Policy at namespace level or MeshPolicy at Mesh level apply to recieving side or Server only.
























~ $kubectl get svc -n microservices
NAME          TYPE        CLUSTER-IP    EXTERNAL-IP   PORT(S)   AGE
giftshopapi   ClusterIP   10.0.22.143   <none>        80/TCP    21h
giftshopui    ClusterIP   10.0.94.161   <none>        80/TCP    21h


giftshopui-v1-ccb98fd97-rspgk       pod ip      10.1.0.133
giftshopapi-v1-7554fc7cf9-p6jwn     pod ip      10.1.0.126






















# GKE

``` bash

~ $istioctl version
client version: 1.4.0-beta.0
control plane version: d3ed357dbb236f0f6188a7fcd17a47941442f9a8
data plane version:  (3 proxies)

~ $kubectl get pods -n microservices
NAME                              READY   STATUS    RESTARTS   AGE
giftshopapi-v1-55f9669bc6-xfghd   2/2     Running   0          132m
giftshopui-v1-7494c94f74-jzkvs    2/2     Running   0          132m

~ $istioctl authn tls-check giftshopui-v1-7494c94f74-jzkvs.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER        CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         HTTP/mTLS     HTTP       default/         -

~ $istioctl authn tls-check giftshopui-v1-7494c94f74-jzkvs.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER     CLIENT     AUTHN POLICY                              DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         mTLS       mTLS       giftshopapi-receive-tls/microservices     giftshopapi-clients-destinationrule/microservices

```


# AKS

``` bash

~ $istioctl version
client version: 1.4.0-beta.0
control plane version: 1.4.0-beta.0
data plane version: 1.4.0-beta.0 (3 proxies)

~ $kubectl get pods -n microservices
NAME                              READY   STATUS    RESTARTS   AGE
giftshopapi-v1-7554fc7cf9-572vx   2/2     Running   0          37s
giftshopui-v1-ccb98fd97-chmr4     2/2     Running   0          28s

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-chmr4.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER         CLIENT     AUTHN POLICY     DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         PERMISSIVE     -          /default         -

~ $istioctl authn tls-check giftshopui-v1-ccb98fd97-chmr4.microservices giftshopapi.microservices.svc.cluster.local
HOST:PORT                                          STATUS     SERVER     CLIENT           AUTHN POLICY                              DESTINATION RULE
giftshopapi.microservices.svc.cluster.local:80     OK         STRICT     ISTIO_MUTUAL     microservices/giftshopapi-receive-tls     microservices/giftshopapi-clients-destinationrule


```