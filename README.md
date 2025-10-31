# Supertronics Repair System

## Repair Tracking and Electronic Sales Management Platform
The Supertronics Repair System is a multi-user web application designed to improve the business operations for electronic repairs. It provides feature to manage repair jobs, tracking status updates, generating quotes and providing different user interfaces for each type of user, Customers, Technicians, Owners.

## Key Features
The system uses role based access to provide each user with functions that only they can use:
- **Owner:** Is able to manage technicians by adding them to the system, updating their profiles and being able to delete them from the system. The owner is also able to add new products to the system for customers to browse. They also have a overview of how the business is doing like seeing the progress on repair jobs and quotes that have been sent to the system.
- **Technician:** Is able to create new repair jobs, update the status of repairs, generate quotes and track devices serial numbers
- **Customer:** Is able to login and signup to the system, view products, track their repairs and request for quotes.

### Core Technical Features
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

Replace both serviceAccountKey.json and google-credentials.json files contents with the following:


--------START OF FILE--------

{
  "type": "service_account",
  "project_id": "supertronics-dc0f9",
  "private_key_id": "463782a7173735c2033d382772f0764bc28a0bca",
  "private_key": "-----BEGIN PRIVATE KEY-----\nMIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQC+vJkug4LA7nZs\nNFUd2XBAIggp5akj9AGv3sWHUwsT/1+JbTuZtfeLZw1NnO/utvpEZBiC0UwRViDU\nhuu4iiSH8cJYQrXIBsLNw44EdpMkttG/olaF1A7eVQj0EvjP9pZD1CI2qccdvS6u\nSOR7JMNaELXQyeUENZTRryR87HOtzjxoXJAI8qckrD05M9UcZurjtfRk2oHT/a/r\nXVjKZDBaVQbXZTtq7tySyZiCm9ax1TcR4SEdqwwbsorVwCxADMpt0GT2NHbJAkdL\nFYdnxCTGGj2oLDgOlYO4PZn5hjmw242jRNGCK3mfNw2Nay2ih9niinekxcRGmsvf\nFAEEoR2HAgMBAAECggEAEkjNHG7h+XHCq3PG1xIxlwxMIQwRj09EXCIsaGEXFkcs\nVuXASWduwA/UcCTDwWwMCMNZsCy1MJJDmmwGq82eW+PaoBBhEbhIM2Z0NkxRpbra\n15aN2c1u0XRwY8lvzavTUtRCpV/YJSsuDuoQ8RcgnQEuFMJN8H3ccs9p3YnjOv14\nLPac8V4i8kmXl6uGW65KApvBOW1NB0mgX3h1ZALYohQ/Q8plOTOwH9MelPzFTlxu\n+WQ1i6TS7cZPfgoM3mVcJa7RblxlIXGDc2nxgfONNYEt5LSfi3aeMuv1Vd4e2akV\nFkHHFd1ZcfWmV0gY6eYgH7NuNkm9G/ShK3b5gOrPiQKBgQDz/NPXFwCDOXpTCXAv\nUYVMjxKPv+AiwzIw8kfeeMhFX2pKN9RrvutNg4Y9u9kpbynCFvsNycp2TLTnx9H8\nOAnqHZ7NBKRgpW5kCYOSXDv8rh6z2IB/LTFcGbxpaJmELiMMQ9escCdY3ho0AZ+s\n7Cz6Be2H/rs1664+cfEtRpB6nwKBgQDIIJteV6uXr+zfPCrSGQKDyZMUkScs+4nY\nIDq09OCIJg2+7I19W1YUsUm8SEOSTNjWMRtLi6/oms2VgwS0Z1o9CKlt1/B5+noz\nSdNxT4m9P/YFZ6kapuk7QhTe6fs/SfYbPoHkW0isjGvoxK6LFTcJGx5QnsULQrW9\nurbtawxcGQKBgQCK9f55TKKAGqZu0LtSV+3BsuLxeGjho6bcdcE1FjmKOVPCPZYA\nX9aIaVZ1pp2CmcuAvbHzInDre7i1IfuY2Rnce05Mmk48tTvwQfLr0xhfS5Q0/iQB\nSu4H9Kh4qJh2zggsHh+iGKZwWN83q40T4dej9uhYQl7B0R+GdULVdJEpJQKBgGi5\nn3RMHnA/UI41FxdgnQ9H0Z7GoqCIdMbTfsUpC8JTX4gUk6oQvMgRSkur6ShK7IGR\nqv3qeEAZmhFuQW0CVJdxy+++O4opR/9E19AI1kRhjyWe7EAVLiGjX/aSrLaexpnV\nKPXNywb4aJOjMM95Z5ZtSXzYyLYuz5ocdhNhnFMpAoGBALTkx6FEpXgoLdviI3a3\neYNn5Ilb2hR2yGYJzgVhMHe24g01hcQIyMV3YLvllL7TFs6K4VO02nEZfbsXhuPc\n3fABS917bZJUxlBCPVmRnwJgjOel/8xq0bC5HoJhihnHvXxAoWv+T5LMdZeAjS6K\n5HQM+gGUTQkD+sBW7nGhptMn\n-----END PRIVATE KEY-----\n",
  "client_email": "firebase-adminsdk-fbsvc@supertronics-dc0f9.iam.gserviceaccount.com",
  "client_id": "117527695912256083449",
  "auth_uri": "https://accounts.google.com/o/oauth2/auth",
  "token_uri": "https://oauth2.googleapis.com/token",
  "auth_provider_x509_cert_url": "https://www.googleapis.com/oauth2/v1/certs",
  "client_x509_cert_url": "https://www.googleapis.com/robot/v1/metadata/x509/firebase-adminsdk-fbsvc%40supertronics-dc0f9.iam.gserviceaccount.com",
  "universe_domain": "googleapis.com"
}


-------- END OF FILE --------
  
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

Owner login: owner@gmail.com / owner123!
Tech login: tech1@gmail.com / tech1!
