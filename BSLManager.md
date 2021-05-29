# BSLManager

A CLI is provided in the Docker image so that admins can manage their instance. 

## Access to the CLI

Since the CLI is packaged into the docker image, you'll have to open a shell from the container. To do so, list first your running containers: 

```
docker ps
```

This should display you something equivalent to this:

```
CONTAINER ID   IMAGE                               COMMAND                  CREATED       STATUS       PORTS                           NAMES
3734c41af5a7   postgres:9.6                        "docker-entrypoint.s…"   2 weeks ago   Up 2 weeks   5432/tcp                        db_1
be6870fe103e   nicolasconstant/birdsitelive:latest "dotnet BirdsiteLive…"   6 weeks ago   Up 2 weeks   443/tcp, 0.0.0.0:5000->80/tcp   birdsitelive
```

Find the BSL container and keep the ID, here it's `be6870fe103e`. And you only need the three first char to identify it, so we'll be using `be6`.

Then open a shell inside the container (change `be6` with your own id):

```
docker exec -it be6 /bin/bash
```

And you should now be inside the container, and all you have to do is calling the CLI:

```
./BSLManager
```

## Setting up the CLI

The manager will ask you to provide information about the database and the instance. 
Those must be same than the ones in the `docker-compose.yml` file.
Provide the information, review it and validate it. Then the CLI UI should shows up. 

## Using the CLI

You can navigate between the sections with the arrows and tab keys. 

The **filter** permits to filter the list of users with a pattern. 

All users have their followings count provided next to them. 
You can select any user by using the up/down arrow keys and hitting the `Enter` key, this will display more information about the user.
You can also remove a user and all their followings by hitting the `Del` key. You will be prompted by a confirmation message, and you'll be able to remove this user.

Deleting users having a lots of followings can take some time: after the prompt has closed the process is still running and will update the list after that. Let the software do its thing and it will go through. 
