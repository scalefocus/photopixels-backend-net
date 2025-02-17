# Business Specification - Open Source Scalefocus Photos




Contents

I.   	Purpose of this document

II.  	Project background

III.     	Application

1\.  	MVP – Phase I

1.1.   	Front-end Requirements.

1.2.   	Back-end Requirements.

2\.  	Phase II

3\.  	Design

 

 

 

 

 

Revision History

 

| Date | Author | Version | Changes |
| ----- | ----- | ----- | :---- |
| 06.2023 | Gorjan Jakovleski | 1.0 | *Document created* |
| 06.2023 | Gorjan Jakovleski | 1.1 | *Requirements revised, Design added* |
| 07.2023 | Gorjan Jakovleski | 1.2 | *Back-end Requirements added* |

 

 

 

 

 

 

 

 

 

 

 

# **I.**                **Purpose of this document**

The purpose of this document is to describe the scope of the internal project initiative, to set clear app development and implementation guidelines as well as to facilitate the smooth onboarding of new team members.

This document gives insights into the purpose and functioning of the open-source application and the anticipated implementation approach by Scalefocus team.

 

# **II.**              **Project background**

There is a self-hosting community that is not using any of the conventional cloud services such as Google’s cloud services. Most of the services required for self-hosting have an alternative which is in parallel to the quality of the Google services, except for the Photos services, that is where this project will step in and provide such a service in order to cover that gap. The project will provide a backend (server) and a  frontend (a mobile application) for both Android and iOS.

# **III.**            **Application**

The project will offer the user to sync photos to a server and access, download, delete, modify them through a UI in the form of a mobile application for both Android and iOS. To describe the application requirements the following format will be used

| ID | User story | Description | Acceptance criteria | Type |
| ----- | ----- | ----- | ----- | ----- |
| Unique ID | Name | User story description | Further details | Functional/Non-functional |

 

 

## **1\.**     **MVP – Phase I**

The first phase of this project will be the MVP version on which the application will be based and later developed further with additional features.

### **1.1.**          **Front-end Requirements**

| ID | User story | Description | Acceptance criteria | Type |
| ----- | ----- | ----- | ----- | ----- |
| P-I-100 | Set-up wizard | As a user, I want to have a set-up wizard, so that I can easily connect to my server | ·        On the top of the screen is the logo of the app. <br> ·        There is an input field to enter the server HTTP address. <br> ·        There is a text below the field describing what’s the idea of the app and at the bottom is a link to the page of the project on GitHub. <br> ·        There is a button “Next” that takes the user to the next step of the set-up process, which is a screen showing the status the connection to the server. <br> If the connection is successful it shows a message that the connection is successful and leads the user to an account creation view with a username and password.<br>  If the connection is not successful there is an error explaining the reason for the unsuccessful connection e.g. “Server is not accessible, please check the address of the server”. <br> ·        While the app is checking the connection to the server, the “Next” button is disabled. | Functional |
| P-I-110 | Account creation   | As a user, I want to be able to create an account | ·        There is an input field for: o   Username (e-mail address) o   Password ·        The login session is saved in the application. ·        The user can register an account with an e-mail address ·        There is an option to reset the password by entering the email address of the account ·        To reset the password the user will receive an e-mail with a link that will redirect the user to the reset password screen of the app. This screen has a new password and confirm password input field. ·        If the user registration is disabled from the BE then an error message is shown “Account registration is disabled”. | Functional |
| P-I-120 | Syncing | As a user, I want the application to sync with the server and the device after I login, so that I can view any existing media | ·        If the user is new then the device’s media is synced to the app. ·        There is an indicator that the device is syncing ·        There is an indicator (green dot) on the top-right corner of every thumbnail to indicate that the media has just synced. ·        If there is no access to the photos on the device, there is a message \- “Please allow access to the photos on your device”. ·        If there are no photos on the server and there are no photos on the device, a blank page is displayed and a message is shown “No media on”. ·        If the user is an already existing user, then all of the metadata (thumbnails) should be downloaded. ·        If a media is deleted to the server, the application deletes that photo from the database and from all devices. ·        If a media is added to the server, that media is added to the device. ·        If a media is added to the device, the media file is added to the server. ·        If a media file is deleted on the device, it is also deleted from the server. | Functional |
| P-I-130 | Thumbnails | As a user, I want to have thumbnails for all of my photos, so that I can easily select the one I want for viewing | ·        There is a thumbnail for all the photos. ·        There are two types of thumbnails: o   Very small version for the tyle (while scrolling). o   On opening the photo a very low resolution of the photo before it loads. ·        The thumbnails are downloaded on the initial sync of the device and are constantly synced. | Functional |
| P-I-140 | Downloading the media | As a user, I want to be able to download the media onto the device once I open it, so that I can view it | ·        When a photo or video is opened, the full photo or video is downloaded to the device. | Functional   |
| P-I-150 | Swiping through media | As a user, I want to be able to swipe between media, so that I can easily switch between them | ·        By swiping left or right the user can switch between media. | Functional |
| P-I-160 | Live photos | As a user, I want to be able to view live photos | ·        Google and Apple live photos are also supported. | Functional |
| P-I-170 | Uploading media | As a user, I want to upload media to the server | ·        When a user inserts a media into the device, then that media is automatically updated to the server. | Functional |
| P-I-180 | Syncing settings | As a user, I want to be able to set up syncing settings, so that I can control when the sync is happening | ·        There is an option to toggle between two syncing modes based on the network connection: o   Only through Wi-fi o   Always (even through cellular data) ·        The default setting is to sync only through Wi-fi. ·        There is an informational message about the sync setting below it: o   When only on Wi-fi is enabled – “The app will sync only on Wi-fi.”. o   When only on Wi-fi is disabled – “The app will sync on Wi-fi and cellular.”. ·        The user can manually initiate the Sync through a button. ·        If the current sync setting is not compatible with the current network settings of the device, a toast message is displayed – “Change your syncing settings to proceed”. | Functional |
| P-I-190 | Trash | As a user, I want to have a Trash folder, so that all the media I delete are not deleted immediately | ·        There is a Trash folder where all the deleted media are moved to. ·        The trash folder holds the deleted media for 30 days, after which they are permanently deleted. ·        There is a message below the heading – “Photos older than 30 days will be automatically deleted.” | Functional |
| P-I-200 | Sorting | As a user, I want the media to be sorted by date, so that I can have better orientation when scrolling | ·        The media are sorted by date of creation. ·        The photos are divided by months. | Non-functional |
| P-I-210 | Divider | As a user, I want to see a divider in the overview of the media I am viewing, so that I know in which period are all the photos I’m viewing | ·        There is a divider of the photos which indicates what month the photos are below it are in e.g. May 2023\. | Non-Functional |
| P-I-211 | Importing from Apple photos and Google photos | As a user, I want to be able to import photos from Apple photos and Google photos, so that I am able to view them inside the application   | ·        The files from Google photos and Apple photos are imported into the application. ·        If a media file is deleted inside Google and Apple photos, it is deleted from the application as well. ·        If a media file is added to Google and Apple photos, it will be added to the application as well. | Non-Functional |
| P-I-212 | Unique media files can be imported only once | As a user, I want to be notified when I try to upload an existing media file, so that I don’t have any duplicated files | ·        If the app tries to upload the same media file to the server, the server will return an error for a duplicated file – “Media already exists”. | Non-Functional |

 

### **1.2.**          **Back-end Requirements**

| ID | User story | Description | Acceptance criteria | Type |
| ----- | ----- | ----- | ----- | ----- |
| P-I-220 | Admin panel (web only) | As an admin, I want to have a panel, so that I can edit user settings | ·        There is a list of every user with their email, password and quota. ·        There is a quota for the data used for each user, the default amount is 50 GB.   ·        There is a number showing the used amount of quota for each user | Non-functional |
| P-I-230 | User management | As an admin, I want to have user’s data and be able to manage it | ·        The admin can add a user. ·        The admin can adjust the quota of a user. ·        The admin can delete a user’s account, when this is done all the files from that user are deleted as well. There is a double confirmation for this – pop-up “All media files for this user will be deleted.” with two buttons “Cancel” on the left and “Continue” on the right side. | Non-functional |
| P-I-240 | Login API | As a product owner, I want the login interface to communicate with the BE | ·        The API checks if the access is authorized and authenticated based on the username and password provided, if approved then an access token is returned, if not the server returns an error. | Non-functional |
| P-I-250 | Register API | As a product owner, I want the register interface to communicate with the BE | ·        The API registers new users with an email and a password to the BE. ·        The admin should be able to disable new user registration and when this is enabled the server should return an error when a new user registration is detected. | Non-functional |
| P-I-260 | Syncing | As a product owner, I want the application to be able to upload, download, update and delete a file, so that the BE and FE are always synced | ·        The backend can: o   Upload a new file. o   Download an existing file. o   Download the two versions of thumbnails (small version for scrolling and bigger version to show when file is opened but not downloaded). o   Delete a file. o   Update a file (if a photo is edited the new version overwrites the existing version). ·        If a user exceeds the quota, no new files are added to the cloud. Existing files can be deleted to make room for new files to be added. | Non-functional |
| P-I-270 | Revisions | As a product owner, I want the BE to reflect the FE and vice versa, so that every change is synced | ·        The BE communicates with the FE on any changes made to the files i.e. the number of files added or deleted and the FE is updated accordingly. ·        The FE tells the server that a change has been made to the files and the server is updated accordingly. | Functional |
| P-I-280 | Docker | As a product owner, I want the application to be run on a docker | ·        The BE is in a docker and is compatible with a Linux operating system | Non-functional |
| P-I-290 | File format | As a product owner, I want the files to be stored in their original format | ·        All the files are stored in their original format. | Non-functional |
| P-I-300 | BE exposes an API endpoint | As a product owner, I want the BE to expose an API endpoint, so that it provides information about the server | ·        The server exposes an endpoint address with “/status” ·        The following information is returned: o   Version of the BE server in format XX.XX.XX e.g.“1.5.3” o   Whether the user registrations are enabled or disabled. | Functional |

 

 

## **2\.** 	**Phase II**

Some of the additional features that would be part of the second phase are:

·        Grouping by date – week, days divider

·        Zooming in and out of the multiple media view by pinching

 

## **3\.** 	**Design**

The design is based on wireframes which will be further refined.

