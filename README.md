#Supertronics Repair System

##Repair Tracking and Electronic Salaes Management Platform
The Supertronics Repair System is a multi-user web application designed to improve the business operations for electronic repairs.It provides feautere to manage repair jobs, tracking status updates, generating quotes and providing diffrent user interfaces for each type of user, Customers, Technicians, Owners.

##Key Features
The system uses role based access to provide each user with functions that only they can use:
- **Owner:** Is able to manage technicians by adding them to the system, updating their profiles and being able to delete them from the system. The owner is also able to add new products to the system for customers to browse. They also have a overview of how the business is doing like seeing the progress on repair jobs and quotes that have been sent to the system.
- **Technician:** Is able to create new repair jobs, update the status of repairs, generate quotes and track devices serial numbers
- **Customer:** Is able to login and signup to the system, view products, track their repairs and request for quotes.

###Core Technical Features
- Secure Authentication by using firebase authentication which is also used for user identity management
- Storage, the system uses firebase firestore, a NoSQL cloud database to store all the information from the system.
- Role Security is implemented with AuthorizeRoleAttribute to enforce access controll over all the controllers and actions

## Technology Used
- **Backend:** ASP.NET Core MVC, using C# with models view controllers and services
- **Database:** Firebase Firestore, used for all persistent data like repairs, products and users
- **Authentication:** Firebase Authentication, used for login and sign-up
- **Admin:** Firebase Admin SDK, used for secure server-side operations like token verification and user deletion
- **NuGet Packagets:** Google.Cloud.Firestore, FirebaseAdmin, Microsoft.AspNetCore.Mvc

## Installation and Setup
1. Prerequisites
- Visual Studio 2022
- .NET 8
- Ensure service account key is in project root file
- Ensure appsettings.json has the following
  {
  "Firebase": {
    "ProjectId": "YOUR_FIREBASE_PROJECT_ID_HERE", 
    "ApiKey": "YOUR_FIREBASE_WEB_API_KEY_HERE"
  },
  "Logging": { /* ... */ }
  }

### Build and run
- Uses the link from the project open visual studio go to clone project and paste the link and click clone
- Wait for project to finish loading
- Click on build and build the solution
- Open the NuGet packet manager and ensure that all packages are installed
- Click run and the application should launch

Published to https://supertronics.azurewebsites.net/ 
