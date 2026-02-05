# OlieJournal Install
Please first complete all the steps in the /infrastructure/README.md file before continuing here.

With the infrastructure in place, we can now install oliejournal to the server. Log into the Linode instance and perform these steps.

## Create symbolic links
Create the oliejournal folder. This will contain useful scripts. These scripts are linked to the scripts inside the repo. Also installs a font.

    # ~/source/repos/oliejournal/install/create_links.sh

## Create install/log directories

    # sudo ~/source/repos/oliejournal/install/create_directories.sh

## Google Service Account Key
We now need to create a Google Service Account key so we can call the text-to-speech API. Log into the Google Cloud, select your project, and navigate to APIs & Services -> Credentials. Click on the Serice Account you want to use. To generate a new key:

- Click on the Keys tab
- Click on "Add key" -> "Create new key"
- Select Key type "JSON"
- Click "Create"
- Your browser should automatically download the json file.

Using WinSCP, make a connection to your Linode.

- Copy the json file to ~/oliejournal

When you update the sourcing file in the next step, set the GOOGLE_APPLICATION_CREDENTIALS to the path of this file.

Secure the file and prevent alterations.

    # chmod go-r virtualstormchasing-*.json
    # chmod u-w virtualstormchasing-*.json

## Create and parameterize the environment sourcing script
The source_template.sh script is copied into the oliejournal folder. This script is called by other scripts to load the proper environment variables.

    # cp ~/source/repos/oliejournal/install/source_template.sh ~/oliejournal
    # cat ~/oliejournal/source_template.sh

Replace the placeholder values within the file and then rename it from **source_template.sh** to **source.sh**. There are instructions within the file on what to do.

    # mv source_template.sh source.sh

Secure the file to prevent inadvertant revelations or alterations.

    # chmod go-rx source.sh
    # chmod u-w source.sh

## Install oliejournal
    # ~/oliejournal/deploy.sh
    # dotnet dev-certs https --trust

### Validate oliejournal.api
The deploy script should have started an instance of oliejournal.api. Send a request to the local server and confirm it returns a valid JSON string without any errors.

    # curl http://localhost:7021/api/weatherforecast

### SELinux Settings
Out of the box Apache isn't allowed to relay network traffic. Nor can it follow a symbolic link. SELinux needs to be configured to allow this.

    # sudo semanage port --add --proto tcp --type http_port_t 7021
    # sudo semanage port --add --proto tcp --type http_port_t 7022
    # sudo setsebool -P httpd_can_network_relay 1

### Apache SSL Site
Configure the SSL site to proxy requests to the dotnet applications, and to the videos folder. Copy the relevant lines.

    # cat ~/source/repos/oliejournal/infrastructure/3_AkamaiLinode/000-oliejournal-default-le-ssl.conf
    # sudo vi /etc/httpd/conf.d/000-oliejournal-default-le-ssl.conf
    # sudo systemctl restart httpd

### Validate startup
Confirm that Apache and the dotnet applications automatically start at boot.

    # sudo reboot

### Validate API URL
Confirm you can navigate to a video with your browser: https://oliejournal.olievortex.com/api/weatherforecast.

Confirm the result contains a valid JSON response without errors.
