
with Diagram("SuSo productionDeployment", show=False):
    tc = Teamcity("CI")
    portal = Csharp("portal")  # also registers
    packages = Github("packages")
    nomad = Nomad("deploy")
    tc >> Edge(label="docker image") >> packages
    portal >> Edge(label="job specs") >> nomad << Edge(
        label="pull") << packages
    with Cluster("web servers"):
        with Cluster("server 1"):
            web = [Docker("tenant 1"), Docker("tenant 2")]
        with Cluster("server 2"):
            web1 = [Docker("tenant 1"), Docker("tenant 2")]
    nomad >> web
    nomad >> web1
