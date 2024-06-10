 # SND SMP

## Introduction

This SMI portal is created to replace the V1 version for Signature Mail. It is a revamped version, which consists of the frontend (Angular 17), backend (Dotnet 8 Core C#), a background worker to process heavy tasks, a MySQL as database, and a dockerized FileServer called ChibiSafe. All these are running on their own respective images using Docker (Except for the DB). The recommended server for this project is a Linux Server due to its flexibility, ease of use, and affordability.

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
-	Uses Azure PostgreSQL for each service
-	1 DB per service

Backend Cluster:
-	Uses Azure Kubernetes Cluster Manager
-	Web Gateway API is a Gateway to access all Services

Frontend:
-	KMS Angular is containerized as an instance
-	Will be accessed using public URL

## Guide
Below are the steps to guide you to run this project in local environment.

### Installing MySQL 8.0 

This SMD will be using MySQL as their database storage. Here are the steps to install MySQL on your local development machine.
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
