# Black.Beard.MockServices
Provide a service for hosting mock without specifically writing the service

[![Build status](https://ci.appveyor.com/api/projects/status/li2if1n9224v9p3n?svg=true)](https://ci.appveyor.com/project/gaelgael5/black-beard-mockservices)

## Quick start to launch the service

the service runs under linux docker.

For launch the docker container
```batch    
    sudo docker run -p 80:8080 blackbeardteam/mock
```

Or in interactive mode
```batch
-- enter in the container
    sudo docker run -p 80:8080 -it --entrypoint /bin/bash blackbeardteam/mock

-- launch the server
    dotnet Black.Beard.ParrotServices.dll
```

Launch a navigator and open the swagger page

``` batch
   explorer.exe "https://localhost:80/swagger/index.html"
```


## Quick start to use the service by code with curl.

Arguments
- **url** address of the docker container
- **contract name** : custom name you want to give to yours service
- **contract file** openApi v3.\* contract
for this test the file to upload is named swagger.json.

### Upload the contract on the service
```batch
   curl -X POST "https://localhost:80/Manager/mock/{contract name}/upload" -H "accept: */*" -H "Content-Type: multipart/form-data" -F "upfile=@swagger.json;type=application/json"
```

### uses the service mock
```batch
    The available url to consume your service is
    "https://localhost:80/Manager/proxy/{contract name}/{your path in the contract}"
```

### Fetch the list of mock template uploaded.
```batch
   explorer.exe "https://localhost:80/www/index.html"
```

# How to test on windows

Source : https://learn.microsoft.com/en-us/windows/wsl/install

for install a linux virtual machine
```batch
    wsl --install -d Ubuntu
```

Now we need to install Docker
source : https://docs.docker.com/engine/install/ubuntu/


## Install using the apt repository
Before you install Docker Engine for the first time on a new host machine, you need to set up the Docker repository. Afterward, you can install and update Docker from the repository.

Set up the repository
Update the apt package index and install packages to allow apt to use a repository over HTTPS:

```bash
 sudo apt-get update
 sudo apt-get install ca-certificates curl gnupg
```

Add Docker’s official GPG key:

```bash
 sudo install -m 0755 -d /etc/apt/keyrings
 curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
 sudo chmod a+r /etc/apt/keyrings/docker.gpg
```

Use the following command to set up the repository:

```bash
 echo \
  "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  "$(. /etc/os-release && echo "$VERSION_CODENAME")" stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null
```


## Install Docker Engine
Update the apt package index:

```bash
sudo apt-get update
```

Install Docker Engine, containerd, and Docker Compose.

Latest
Specific version

To install the latest version, run:

```bash
sudo apt-get install docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
```
Verify that the Docker Engine installation is successful by running the hello-world image.


```bash
sudo docker run hello-world
```

## If you have troubleshoot.

You have a message like this.
```html
Cannot connect to the Docker daemon at unix:/var/run/docker.sock
```

first try to launch the service.
```bash
sudo dockerd
```

Check the service run.

```bash
# check if your system is using `systemd` or `sysvinit`
ps -p 1 -o comm=
```

If the command doesn't return systemd, and in my case, Ubuntu-20.04 on WSL, the command returned init, then use the command pattern

```bash
# start services using sysvinit
sudo service docker start
```

If the command return systemd/
```bash
# start services using systemd
sudo systemctl start docker
```

## Push variables in the docker.
all environment variables started by "parrot_log_" are injected in configuration of the log.

* parrot_log_directory : can be used for customize the targer folder of the logs.