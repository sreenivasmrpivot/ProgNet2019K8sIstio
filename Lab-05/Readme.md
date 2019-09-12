# Lab-05 (Install Istio on PKS K8s cluster)

## Pre-requisites

1. [Installing the HELM client](https://helm.sh/docs/using_helm/#installing-helm)
2. [Chocolatey](https://chocolatey.org/install) if you are working on a Windows machine

## Istio

Write briefly about Istio and point to the Istio url here
talk about various components and manual vs automatic sidecar injection

## Istio Setup

Istio is setup at the K8s cluster level. However it is enabled at the individual namespace level. Though most of these steps are documented in the official istio website, it could be daunting for some and may not work in a single attempt. I have replicated just the basic necesary steps in the correct order here to get istio working for you without much road blocks. These steps were derived from my personal experience of installing istio on PKS and AKS several times. These steps are valid at the time of this documentation and may not hold good if there is any changes from the istio team.

1. Clone the istio source code from official istio git repository using the command

``` bash
git clone https://github.com/istio/istio.git
```

2. Navigate inside the istio folder using 

``` bash
cd istio
```

3. Create a separate namespace in your K8s cluster, where all the istio components will be hosted. You may create a namespace using the below command.

``` bash
kubectl create namespace istio-system
```

4. CRDs are Custom Resource Definitions and istio leverages K8s CRDs for itself to work. You may install the CRDs required for teh proper functioning of istio usng the below command. 

``` bash
helm template install/kubernetes/helm/istio-init --name istio-init --namespace istio-system --set certmanager.enabled=true | kubectl apply -f -
```

As you did enable the certmanager in the above istio CRD installation, you should see a count of 28 returned when you execute the below command. A response of 28 represents successful installation of istio for the chosen arguments.

``` bash
kubectl get crds | grep 'istio.io\|certmanager.k8s.io' | wc -l

28
```

Incase you intend to delete the istio CRDs either for re-installation purpose or permanant removal, you may execute the below commds. 

``` bash
helm template install/kubernetes/helm/istio-init --name istio-init --namespace istio-system --set certmanager.enabled=true | kubectl delete -f -
kubectl delete -f install/kubernetes/helm/istio-init/files
```

5. The istio installation comprises of the istio CRDs and the actual istio components. In the previous step we have instaled the istio CRDs. However the istio is not fully functional until the remaining components of istio are installed. The istio components are bundled as a helm chart. To install the actual istio components, you execute the below command.

``` bash
# Redirect the output of helm command execution to a kubernetes manifest file called istio.yaml
helm template install/kubernetes/helm/istio --name istio --namespace istio-system --set ingress.enabled=true --set gateways.istio-egressgateway.enabled=true --set certmanager.enabled=true --set grafana.enabled=true --set galley.enabled=true --set servicegraph.enabled=true --set tracing.enabled=true --set kiali.enabled=true --set istiocoredns.enabled=true --set istio_cni.enabled=true > istio.yaml

# Apply the generated kubernetes manifest from istio.yaml file 
kubectl apply -f istio.yaml
```

## Resources

* [Istio](https://istio.io/docs/concepts/what-is-istio/)
* [Istio Architecture](https://istio.io/docs/concepts/what-is-istio/arch.svg)

