# Lab-04 (Creating / Connecting to PKS K8s cluster)

As per the official Kubernetes website [kubernetes.io](https://kubernetes.io/)

``` 
Kubernetes (K8s) is an open-source system for automating deployment, scaling, and management 
of containerized applications.
```

Several people / organizations are contributing constantly to the open-source Kubernetes. In addition to this there are several vendors who offer their own **Distribution of K8s** or **Hosted service of K8s**.

Cloud Native Computing Foundation (**CNCF**)'s *Software Conformance Certification* ensures that every vendor’s version of Kubernetes supports the required APIs, as do open source community versions. For organizations using Kubernetes, conformance enables interoperability from one Kubernetes installation to the next. It allows them the flexibility to choose between vendors.

For more details refer to CNCF's [Software Conformance](https://www.cncf.io/certification/software-conformance/) page.

In this lab we will provide information about using Pivotal Container Service (PKS).

### Pre-requisites

1. This lab uses PKS, which requires pks cli . Follow the instructions to install PKS CLI from [here](https://docs.pivotal.io/pks/1-4/installing-pks-cli.html).
2. Follow the instructions to install kubectl CLI  from [here](https://kubernetes.io/docs/tasks/tools/install-kubectl/)

## PKS

### PKS Admin Operations (Presenter only access)

#### Log in to PKS

<!--
Source: https://docs.pivotal.io/runtimes/pks/1-4/login.html
-->

PKS is a secured environment and you will need to log in to get access
to the cluster via the API endpoint.

Please find the PKS API details for this lab below.

```
PKS-API-ENDPOINT = api.pksone.io
```

Use the `pks login` command to log in with your username, password and
PKS API endpoint.
You will skip SSL validation using the `-k` flag because our platform
has self signed SSL certificates.

``` bash
pks login -a https://<PKS-API-ENDPOINT> -u <YOUR-USERNAME> -k
```

`pks` is the CLI used to create, manage and delete kubernetes clusters.

#### Create a cluster

Execute the following command to create a pks cluster.

```pks create-cluster <<username>>pkscluster --external-hostname example.hostname --plan training```

#### View cluster status

Discover the clusters you are authorized to use by running:

```pks clusters```

#### Target a cluster

1.  `kubectl` is the CLI used to manage workloads within a K8s cluster.
    Use the `pks` CLI to provide credentials to `kubectl`:

    ``` bash
    pks get-credentials training
    ```

    The `pks get-credentials` command will update the kubeconfig file with
    the appropriate credentials so that you can access your K8s cluster
    using `kubectl`.

#### Viewing nodes

Nodes are the containers that run our workloads.
Being able to get information about the state of your nodes will help
you understand the health of your cluster.
This can come in handy during troubleshooting or capacity planning.
Use the following command to get a list of all of your nodes:
``` bash
kubectl get nodes -o wide
```

Answer the following questions about each of your nodes:
- What is the node name?
- How long has the node been running?
- What is the status?
- Is there an external IP address?
- What OS image is used?
- What Kernel version?

#### View more details of a node

`kubectl get nodes` did not give any detailed information about the
nodes.
To fetch more detailed information for a specific node run:
```no-highlight
kubectl describe node NODE-NAME
```
Answer the following questions about one of the nodes in your cluster:
- Is the node under Memory Pressure, Disk Pressure or PID Pressure?
- How many CPUs?
- How much storage and memory capacity does the node have?
- How much of the resources are allocated?
- Are there any events for the node?

There is lots of information here but the short list above will aid
in troubleshooting or capacity planning.


### PKS Cluster User Operations (All training attendees can access)

#### Log in to PKS

You may login to the PKS K8s training cluster by executing the below command.

``` bash
pks get-kubeconfig training -u <username> -p <password> -a api.pksone.io -k
```

#### Switch context and set target namespace

Ensure that your kubectl config is using the training context, by executing the below command.

``` bash
kubectl config use-context training
```

Each user has a corresponding kubernetes namespace to operate upon in the training environment. The namespaces are named after the username, with a "-ns" appended.

Target your name space using

``` bash
kubectl config set-context --current --namespace=<username>-ns
```

## Resources

* [PKS docs](https://docs.pivotal.io/runtimes/pks/)
* [Managing PKS](https://docs.pivotal.io/runtimes/pks/1-4/managing-clusters.html)
* [Using PKS](https://docs.pivotal.io/runtimes/pks/1-4/using.html)
* [Kubernetes Components](https://kubernetes.io/docs/concepts/overview/components/)
* [kubectl docs](https://kubernetes.io/docs/reference/kubectl/overview/)
* [kubectl cheatsheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)