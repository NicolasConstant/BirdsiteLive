# Installation

## Prerequisites 

You will need a Twitter API key to make BirdsiteLIVE working. First create an **Standalone App** in the [Twitter developer portal](https://developer.twitter.com/en/portal/projects-and-apps) and retrieve the API Key and API Secret Key. 

## Server prerequisites

Your instance will need [docker](https://docs.docker.com/engine/install/) and [docker-compose](https://docs.docker.com/compose/install/) installed and working. 

## Setup

Download the [docker-compose file](https://github.com/NicolasConstant/BirdsiteLive/blob/master/docker-compose.yml): 

```
sudo curl -L https://raw.githubusercontent.com/NicolasConstant/BirdsiteLive/master/docker-compose.yml -o docker-compose.yml
```

Then edit file: 

```
sudo nano docker-compose.yml
```

### Attributes to change in the docker-compose file

#### Personal info

* `Instance:Domain` the domain name you'll be using, for example use `birdsite.live` for the URL `https://birdsite.live`
* `Instance:AdminEmail` the admin's email, will be displayed in the instance /.well-known/nodeinfo endpoint
* `Twitter:ConsumerKey` the Twitter API key
* `Twitter:ConsumerSecret` the Twitter API secret key

#### Database credentials

The database credentials must be changed the same way in the **server** and **db** section.

* database name:
  * `Db:Name`
  * `POSTGRES_DB`
* database user name:
  * `Db:User`
  * `POSTGRES_USER`
* database user password:
  * `Db:Password`
  * `POSTGRES_PASSWORD`

## Startup

Launch the app with:

```
docker-compose up -d
```

By default the app will be available on the port 5000

## Nginx 

On a Debian based distrib:

```
sudo apt update
sudo apt install nginx
```

Check nginx status: 

```
sudo systemctl status nginx
```

### Create nginx configuration

Create your nginx configuration

```
sudo nano /etc/nginx/sites-enabled/{your-domain-name.com}
```

And fill your service block as follow:

```
server {
    listen        80;
    server_name   {your-domain-name.com};
    location / {
        proxy_pass         http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

Save and start/restart your Nginx service 

```
sudo service nginx start
# or restart it if its already started
sudo service nginx restart
```

### Secure your hosted application with SSL

After having a domain name pointing to your instance, install and setup certbot:

```
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d {your-domain-name.com}
```

Make sure you're redirecting all traffic to https when asked.

Finally check that the auto-revewal will work as espected:

```
sudo certbot renew --dry-run
```

### Set the firewall 

Make sure you're securing your firewall correctly:

```
sudo ufw app list
sudo ufw allow 'Nginx Full'
sudo ufw allow 22/tcp
sudo ufw allow 80/tcp
sudo ufw allow 443/tcp
sudo ufw enable
sudo ufw status
```

You should now have an up and running BirdsiteLIVE instance!

## Upgrading 

Make sure your data belong outside the containers before migrating (set by default). 

To upgrade your installation to the latest release:

```
# Edit `docker-compose.yml` to update the version, if you have one specified
# Pull new images
docker-compose pull
# Start a new container, automatically removes old one
docker-compose up -d
```
