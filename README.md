# SND SMP

## Introduction

This SMI Portal is created to replace the V1 version for Signature Mail. It is a revamped version, which consists of the frontend (Angular 17), backend (Dotnet 8 Core C#), a background worker to process heavy tasks, a MySQL as database, and a dockerized FileServer called ChibiSafe. All these are running on their respective images using Docker (Except for the DB). The recommended server for this project is a Linux Server due to its flexibility, ease of use, and affordability.

## SMP Information

-	Architecture: Monolithic
-	Database: MySQL
-	Framework: 
    - Backend: C# DotNet8
    - Frontend: Angular Framework 17
-	Source Control: GitHub Enterprise
-	Targeted Deployment Hub: Linux Server | Ubuntu 22.04 Jammy
-	Recommended IDEs: 
    - Backend: Visual Studio 2022 (DotNet8 Support) / Visual Studio Code
    - Frontend: Visual Studio Code
-	Requirements:
    - Node.js
    - MySQL
    - DotNet8
    - Docker


## Architecture

![image]()

Database: 
-	Uses MySQL

Backend and Background Worker:
-	Uses Docker to containerize the image.

Frontend:
-	SMI Portal is containerized as an instance
-	Will be accessed using a public URL

ChibiSafe File Server:
-	Docker to containerize the image.


## Guide
Below are the steps to guide you to run this project in the local environment.

### Installing MySQL 8.0 

This SMD Portal will be using MySQL as its database storage. Here are the steps to install MySQL on your local development machine.
1.	Head to [Community DL Page (dev.mysql.com)](https://dev.mysql.com/downloads/installer/) to download the installer. We are using the current latest version 8.0.36. Select the Windows x86-64bit installer.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL01.png?raw=true)

2.	Click on ``No thanks, just start my download`` to start your download.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL02.png?raw=true)

3.	Run the installer and accept and allow changes, then select **Custom** and click **Next**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL03.png?raw=true)

4.	Select these product to be installed and click the arrow to the right.
    - MySQL Server 8.0.36 - X64
    - MySQL Workbench 8.0.36 - X64
    - MySQL Shell 8.0.36 - X64
    - MySQL Router 8.0.36 - X64
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL04.png?raw=true)

5.	Confirm the list of product to be installed and then click **Execute**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL05.png?raw=true)

6.	Wait for the installation to complete then click **Next**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL06.png?raw=true)

7.	Then click **Next** to proceed to configure MySQL Server.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL07.png?raw=true)

8.	Use default settings then **Next**.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL08.png?raw=true)

9.	Proceed **Next** again.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL09.png?raw=true)

10.	Input your password. In this case my password is `abcd1234`.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL10.png?raw=true)

11.	Rename the services name if you wish or else just use default then **Next**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL11.png?raw=true)

12.	Grant full access and proceed.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL12.png?raw=true)

13.	Click on **Execute** to apply all those configuration you just did.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL13.png?raw=true)

14.	After configuration successfully applied, click on **Finish**.
	
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL14.png?raw=true)

15.	Then click on **Next** and proceed to use default configuration for MySQL Router setup.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL15.png?raw=true)

16.	You may check ``Start MySQL Workbench after setup`` to run the application after click on **Finish** to finalize MySQL installation.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL16.png?raw=true)

17.	Or you use `Windows Key + S` to open search on Windows and type in **MySQL** to open it.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL17.png?raw=true)

18.	Click on `Local instance MySQL80`. Additionally you may rename the connection name by right click **Edit Connection** to suit your project naming criteria.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL18.png?raw=true)

19.	Enter your password that we just configured just now. You may tick `Save password in vault` and the password will auto saved so you do not need to enter everytime you access MySQL.
> In our case, the username is `root` and the password is `abcd1234`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL19.png?raw=true)

20.	Click on `Schemas` and your database list will be listed on the left side.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MySQL/MYSQL20.png?raw=true)

### Installing GitHub Desktop

The source control for this project is using GitHub Enterprise. If you do not have an account, go to [GitHub Signup Page](https://github.com/signup?ref_cta=Sign+up&ref_loc=header+logged+out&ref_page=%2F&source=header-home) to create an account. Remember to use your company account. After you have created an account, request access to GitHub enterprise from your manager and provide your account name.

The guide below will be to install GitHub Desktop on your local machine and pull the project from the Hub.

1.	Head over to this [link](https://desktop.github.com/) to download the installer for GitHub Desktop.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB01.png?raw=true)

2.	Click on **Skip this step** to proceed.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB02.png?raw=true)

3.	Configure your information.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB03.png?raw=true)

4.	Once installed, you will see the screen below.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB04.png?raw=true)

5.	Go to **File > Options** or `Ctrl + ,` to open GitHub Desktop Settings.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB04.5.png?raw=true)

6.	Head to Accounts and Sign-in to GitHub.com.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB05.png?raw=true)

7.	It will prompt you sign in using your browser. Click **Continue with browser**.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB06.png?raw=true)

8.	Key in the `Username`/`Email` address and `Password` of the account linked to the company’s GitHub Enterprise and click **Sign in**
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB07.png?raw=true)

9.	After you sign in, it will redirect back to GitHub Desktop. If you have accepted to join GitHub enterprise from your company email, you will see `coldarse` with the available repos under it.  Select **"coldarse/SND.SMP"** and click **Clone**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB08.png?raw=true)

10.	Create a local path for the repo to clone to and click **Clone**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB09.png?raw=true)

11.	After the cloning process is done, you will see the screen as below.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB10.png?raw=true)

12.	To open the project, got to Repository and select `Show in Explorer` or `Ctrl + Shift + F`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB11.png?raw=true)

13.	This will open up the project folder in File Explorer. Double click the `.sln` file to open the solution in Visual Studio 2022.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB12.png?raw=true)

14.	Your project solution’s Solution Explorer should look something like that. 

![image](https://github.com/jackywoo1991/images/blob/main/SMP/GITHUB/GITHUB13.png?raw=true)

### Installing Docker Desktop

In this project, we will utilize Docker to deploy our applications.

Below are the steps to install Docker Desktop on your local machine.

1.	Head over to [https://www.docker.com/products/docker-desktop/](https://www.docker.com/products/docker-desktop/) and download Docker Desktop for windows

![image](https://user-images.githubusercontent.com/124862597/228181794-ab2341a7-c0c5-421f-9eba-ad9ed3ae7f09.png)

2.	Click on the `.exe` file to open the installer.

![image](https://user-images.githubusercontent.com/124862597/230108626-0be0f167-699c-480a-b5ae-3668b5e8814a.png)

3.	You will see the Configuration page, you may uncheck **Add shortcut to desktop** if you do not wish for it to, but make sure to check **Use WSL 2**, this will enable you to run both Windows and Linux containers on your Windows machine. Click **Ok**.

![image](https://user-images.githubusercontent.com/124862597/228181855-7ea3a791-e150-4a47-9e5d-9d5513c4c771.png)

4.	The installer will now start installing Docker Desktop. The installation will take a while.

![image](https://user-images.githubusercontent.com/124862597/228181887-a85f2e40-b222-42c0-85c4-c6f3b68ce07e.png)

5.	After the installation is complete, it will prompt you to restart your machine. Click **Close and restart**.
 
![image](https://user-images.githubusercontent.com/124862597/228181908-6b8e07b9-0c90-4d36-a0ec-36432bbcfbcd.png)

6.	After your machine has restarted, Docker Desktop will automatically pop up its Service Agreement. You may read through the agreement if you wish and select **Accept**.

![image](https://user-images.githubusercontent.com/124862597/228181935-15f17cd3-6e39-4088-a789-68585293b948.png)

7.	If you have not previously configured Docker on your machine or have not run any linux related VMs before, you will be prompted to update/install WSL kernel. Follow the link and you will be brought the Step 4 in the link.

![image](https://user-images.githubusercontent.com/124862597/228181955-e1dc35f5-86cc-42d2-bdb8-0f9e26495146.png)

8.	Click on the link to download the installer for the latest WSL kernel.

![image](https://user-images.githubusercontent.com/124862597/228181989-b0720a25-c4f2-440f-90c5-3f30167d1877.png)

9.	Click on the `.msi` file to open the installer.

![image](https://user-images.githubusercontent.com/124862597/228182003-cc271769-36c2-4e42-840d-c82b0b27e9c3.png)

10.	You will be prompted a pop-up to install the WSL. Click **Next**.

![image](https://user-images.githubusercontent.com/124862597/228182026-bc683fd7-bdae-4652-a15a-0fa4bc33f1f6.png)

11.	It will be a quick installation. Click **Finish**.
 
![image](https://user-images.githubusercontent.com/124862597/228182047-4287dd12-103e-446a-8665-345124bf2ccc.png)

12.	Use `Windows Key + S` to open search on Windows and type in **Windows PowerShell** and **Run As Administrator**.

![image](https://user-images.githubusercontent.com/124862597/228182060-90c91299-5279-4bbf-83ae-cc112e51b79c.png)
 
13.	Type in the command `wsl --set-default-version 2` to set the default version of WSL.

![image](https://user-images.githubusercontent.com/124862597/228182086-de779275-ee6e-4b7c-a27f-5014703abbd1.png)

14.	Open Docker Desktop if it’s not already open and you shall see **Docker Desktop Starting** and you shall see a green bar at the bottom left when it has started.

![image](https://user-images.githubusercontent.com/124862597/228182122-a155a45d-786b-4092-be4f-95d93ffa5ce4.png)
![image](https://user-images.githubusercontent.com/124862597/228182136-b1767c1d-af17-4ee9-a46c-d1cb79319448.png)
![image](https://user-images.githubusercontent.com/124862597/228182157-22e15876-4bd0-4787-a39a-bbb7e54fe13d.png)

## Setting up ChibiSafe File Server
This project uses a file server called ChibiSafe, which provides its own authorization, APIs, and documentation to store any files uploaded to the server for easy retrieval and keepsake.

It uses Docker to run so in this setup, a `docker-compose.yml` file will be used to create a container instance in your own local machine to host this file server.

1. Navigate to `SND.SMP > chibisafe`, and within it will contain the `docker-compose.yml` file.

2. The file should contain the following.

```
version: "3.7"

services:
  chibisafe:
    image: chibisafe/chibisafe:v.5.6.10
    container_name: chibisafe
    volumes:
      - ./database:/home/node/chibisafe/database:rw
      - ./uploads:/home/node/chibisafe/uploads:rw
      - ./logs:/home/node/chibisafe/logs:rw
    ports:
      - 24424:8000
    restart: always
```
3. To explain this YAML file simply, it is the requirements for making the container run properly.

4. As you can see under the `services` tab, we are creating a service called `chibisafe` with the image that will be pull from the ChibiSafe's DockerHub Repository.

5. We are naming the container name to be `chibisafe`.

6. In the `volumes` section, we are creating 3 essential directories:
-	`database`: This is the DB for the file server.
-	`uploads`: This is the place where it will store the files uploaded to the server.
-	`logs`: This directory contains the logs generated by the file server.

7. This file server runs on port 24424 by default, which will route to the port 8000 internally.

8. We are also setting the `restart` to `always` to auto-restart whenever it crashes. This will make sure the file server is always up.

9. To run this on your local device, assuming you have already installed and configured Docker to your usage, we are going to open up PowerShell and head to this directory.

10. To run this, simply run the command `docker compose up -d`.

11. The parameter `-d` is to make sure the container is running in the background.

12. You shall see it begin to pull the image from the ChibiSafe Repository (if you don't have the image on your local machine)

13. And when its done pulling, it will start on its own.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/e40b5b65-3693-4a27-8351-aad30ccfc453)

14. If you see the container has been started like the image above, you can now head over to [link](http://localhost:24424) and you will see the page below.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/7d9820ff-3b12-4392-bb8a-c4b87526217d)

15. On the top right corner, click `Login / Register`.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/f500e13a-0ea0-4649-a779-a487f83dbca2)

16. In the `Username` and `Password` key in `admin` on both and click `Sign In`.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/2360ead9-ab17-4ada-899f-c75111a6bda0)

17. You are now logged in to the file server as an admin.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/6eb7638a-5d8f-4b1e-af51-e64e2ecb5f03)

18. You can select the `Credentials` menu under `Account` to change your admin password.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/a4492535-251d-49bd-a5e4-5c6777e26214)

19. This project uses the admin account, so to use this we will need the key for the APIs to verify. Click on `Request new API key` and you will be prompted that this will overwrite the old key, select `Confirm` to generate.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/1912ca29-91d4-4069-a4ab-cd7d09fcb501)

![image](https://github.com/coldarse/SND.SMP/assets/37390180/1c7f800e-e0f4-47f8-bb70-7db904bd7cff)

20. Hover over the textbox to show your generated key and copy it.

21. If you have done the database migration, you will need to insert this into the `applicationsettings` table.

22. Head over to MySQL and locate the `applicationsettings` table. Right click and `Select Rows`

![image](https://github.com/coldarse/SND.SMP/assets/37390180/98eac89f-bb69-4b30-a2c8-8f396b433724)

23. Insert/Update the below 2 records in this table if you do not already have them.

-	`ChibiURL` : `http://localhost:24424/api/`
-	`ChibiKey` : {_YourGeneratedToken_}

![image](https://github.com/coldarse/SND.SMP/assets/37390180/1fddd239-7b1c-4c84-84e7-2515dc85a837)

24. On the bottom right corner, click `Apply` and `Apply` in the prompt to execute this query for change.

![image](https://github.com/coldarse/SND.SMP/assets/37390180/864169a1-8d7d-417e-84f7-49f80c2b96a8)

![image](https://github.com/coldarse/SND.SMP/assets/37390180/194809a8-1140-4b83-9502-d747a968ab83)


## Installing node version manager
Node Version Manager (NVM), as the name implies, is a tool for managing Node versions on your device.

NVM allows you to install different versions of Node, and switch between these versions depending on the project that you're working on via the command line.

To download, go to [nvm-windows](https://github.com/coreybutler/nvm-windows?tab=readme-ov-file#readme). The current latest version as of today (April 2024) is 1.1.12.

1. Scroll down and click on the **Download Now!** button. 

![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS01.png?raw=true)

2.	Then scroll down and click on `nvm-setup.exe` under **Assets**.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS02.png?raw=true)

3.	Complete the installation wizard. When done, you can confirm that nvm has been installed by running `nvm -v` on **Command Prompt (CMD)**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS03.png?raw=true)

4.	Run this command `nvm install 20.9.0` to install nodejs version **20.9.0** which compatible with our current project version.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS04.png?raw=true)

5.	After installation succeeded, run this command `nvm use 20.0.0` to use the version you just installed.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS05.png?raw=true)

6.	To check current version, run `nvm current`.
 
![image](https://github.com/jackywoo1991/images/blob/main/SMP/Nodejs/NODEJS06.png?raw=true)

## EF Core Migration to MySQL

This project already had a build in ef core migration script pre-written. All you need to do is just run the projects and everything will be done automatically.

1. Expand the `src` folder then expand `SND.SMP.Migrator` and finally click on ``appsettings.json``

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG01.png?raw=true)

2. Comment the **live server** by adding `//` on the start of the code then copy the code below at a new line and save it.
```
 "Default": "server=localhost;port=3306;database=SMPDb;uid=root;pwd=abcd1234;"
```
![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG02.png?raw=true)

3. On middle top of your visual studio, there is a drop down list for projects to run. Choose `SND.SMP.Migrator`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG03.png?raw=true)

4. Then click on the start button located at right of the drop down list.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG04.png?raw=true)

5. A command prompt will then pop out and ask do you want to perform this migration. Type `Y` then enter.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG05.png?raw=true)

6. After that wait for the migration to complete and then press **ENTER** to exit.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG06.png?raw=true)

7. Close the command prompt.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG07.png?raw=true)

8. Head to MySQL Workbench and connect to your database.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG08.png?raw=true)

9. You will notice that on your left, your database had created.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/MIGRATION/MIG09.png?raw=true)

## Setting up swagger

1. Expand the `src` folder then expand `SND.SMP.Web.Host` and finally click on ``appsettings.json``

![image](https://github.com/jackywoo1991/images/blob/main/SMP/SWAGGER/SWAG01.png?raw=true)

2. Comment the **live server** by adding `//` on the start of the code then copy the code below at a new line and save it.
```
 "Default": "server=localhost;port=3306;database=SMPDb;uid=root;pwd=abcd1234;"
```
![image](https://github.com/jackywoo1991/images/blob/main/SMP/SWAGGER/SWAG02.png?raw=true)

3. On middle top of your visual studio, there is a drop down list for projects to run. Choose `SND.SMP.Web.Host`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/SWAGGER/SWAG03.png?raw=true)

4. Then click on the start button located at right of the drop down list.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/SWAGGER/SWAG04.png?raw=true)

5. Wait for the swagger website to launch itself. This will serve as the prove that you had successfully setup swagger.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/SWAGGER/SWAG05.png?raw=true)

## Setting up Angular (UI)
To setup Frontend, we will be using Visual Studio Code. You can use Microsoft Visual Studio too to run angular too.

1. Launch Visual Studio Code.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG01.png?raw=true)

2. Go to **File > Open Folder** or `Ctrl+K + Ctrl+O` to open folder.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG02.png?raw=true)

3. Navigate to your project folder and search for **angular** folder then **Select Folder**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG03.png?raw=true)

4. Visual Studio Code will load your angular project folder.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG04.png?raw=true)

5. Right click any of the file and then **Open in Integrated Terminal**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG05.png?raw=true)

6. After that enter `npm install` to install all the necessary packages.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG06.png?raw=true)

7. If you are having npm ERR! issue, this is due to dependency issue. You can just ignore the error with `npm install --force` command.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG07.png?raw=true)

8. Then, after downloaded relevant packages, start the project by typing `yarn start`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG08.png?raw=true)

9. **[Optional]** If you encounter error as shown from images below, it is due to your `yarn.lock` content does not match with your `package.json` else skip to **step 13**.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG09.png?raw=true)

10. **[Optional]** `Ctrl+C` to stop the terminal and then right click `yarn.lock` files and **delete** it.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG10.png?raw=true)

11. Next, type in `yarn` to generate a new `yarn.lock` file.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG11.png?raw=true)

12. After finish rebuilding `yarn.lock` type in `yarn start` to build and start your angular. Wait for it to finish compile. It may take a while.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG12.png?raw=true)

13. When Visual Studio Code has done building the project, click on [http://localhost:4200](http://localhost:4200) and it will open up in your default browser. 

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG13.png?raw=true)

14. This is the login page. The id is `admin` and password is `123qwe`.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG14.png?raw=true)

15. You have successfully setup the frontend. Test out the CRUD of your services to see if they are working properly.

![image](https://github.com/jackywoo1991/images/blob/main/SMP/ANGULAR/ANG15.png?raw=true)
