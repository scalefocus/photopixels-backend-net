
# Photo Pixels API

## Software Design Specification (SDS)

### Version 1.0 - January 2025

---

### Table of Contents
1. [Introduction](#introduction)
   - [Purpose](#purpose)
   - [Definitions, Acronyms, and Abbreviations](#definitions-acronyms-and-abbreviations)
   - [Intended Audience and Reading Suggestions](#intended-audience-and-reading-suggestions)
   - [System Overview](#system-overview)
   - [References](#references)
2. [Design Considerations](#design-considerations)
   - [Assumptions and Dependencies](#assumptions-and-dependencies)
   - [General Constraints](#general-constraints)
   - [Goals and Guidelines](#goals-and-guidelines)
3. [Architectural Strategies](#architectural-strategies)
   - [Clean Architecture](#clean-architecture)
   - [CQRS](#cqrs)
   - [MediatR for Command/Query Handling](#mediatr-for-commandquery-handling)
   - [Database Choice and Persistence](#database-choice-and-persistence)
   - [TUS for Resumable Uploads](#tus-for-resumable-uploads)
   - [Logging and Monitoring](#logging-and-monitoring)
4. [System Architecture](#system-architecture)
   - [System Decomposition](#system-decomposition)
5. [Policies and Tactics](#policies-and-tactics)

---

### Introduction

#### Purpose
The purpose of this document is to specify the design of the Photo Pixels API, which offers the user the chance to sync photos to a server and access, download, delete, and modify them. This SDS focuses on the technical design of the API, version 1.0 or MVP phase, which provides the initial functionalities developed thus far. The design described in this document covers the back-end structure, omitting any user interface (UI) considerations, as the system will primarily interact with client-side applications through a RESTful API  

Contained here is the full architecture of the API, including key design patterns like Clean Architecture, CQRS, MediatR alongside SOLID principles, database considerations and protocols. It provides a comprehensive view of how the API is structured. 

#### Definitions, Acronyms, and Abbreviations
- **API**: Application Programming Interface
- **ORM**: Object Relational Mapper
- **IIS**: Internet Information Services
- **JWT**: JSON Web Token
- **TUS**: Resumable Upload Protocol
- **CQRS**: Command Query Responsibility Segregation
- **SOLID**: Software Design Principles
- **KISS**: Keep It Simple, Stupid 
- **MVP**: Minimum Viable Product


#### Intended Audience and Reading Suggestions
- **Developers**: SDS will serve as a blueprint for implementing and extending the system. It provides key architectural decisions and design patterns. By reading the architecture and system design sections developers will understand how the system is organized before moving to detailed implementation. It will also serve onboarding purposes for new team members. 
- **Testers**: Can use this document to understand how the system is built and to identify key areas for integration and unit testing.
- **Integrators**: Since this API will be integrated into client-side application, mobile and web developers will find important information about the API endpoints and service interface design

#### System Overview
The Photo Pixels API is an open-source RESTful API developed using the .NET 8 stack. The API is built following SOLID principles and Clean Architecture, separating concerns and enforcing maintainability, testability and scalability. With clean architecture it's ensured that the system can scale as additional features or integrations are required. The architecture makes use of CQRS to handle commands (for actions like managing users and uploading photos) separately from queries (for retrieving data), enhancing performance and enabling better separation of read and write concerns.  

At the core of the system is MediatR, a package used to decouple the command/query requests from their handlers, improving modularity and ensuring that business logic remains independent of the infrastructure. 

#### References
- [Clean Architecture](https://positiwise.com/blog/clean-architecture-net-core)
- [CQRS with MediatR](https://medium.com/@1nderj1t/implement-cqrs-design-pattern-with-mediatr-in-asp-net-core-6-c-dc192811694e)
- Business Specification Document: **Open Source Scalefocus Photos.docx**

---

### Design Considerations

#### Assumptions and Dependencies
- **Operating Systems**: Runs as a Docker container on Windows/Linux servers.
- **Stack**: The API uses the .NET 8 framework with ASP.NET Core as the web development environment. Marten is used as a .NET Transactional Document DB and Event Store with PostgreSQL as the database system. The photos are saved directly to the server in a file system. 
- **Authentication**: JWT is used for user authentication and authorization, handled by Microsoft.Identity and integrated within the API to secure endpoints.
- **End-User Characteristics**: The primary end-users will not interact directly with the API but rather through a web or mobile UI developed separately. The API must therefore be flexible, providing endpoints that can be easily integrated into diverse front-end applications.
- **Potential Changes in Functionality**: The API is designed with Clean Architecture principles and CQRS which will allow for easier modifications or expansions. New features or changes in functionality are expected, especially as new user requirements are identified. 
- **Mail and Image Processing**: MailKit is used for all features concerned with mailing the user. ImageSharp is used for all image processing done on the back end. And SolidTUS is used as an implementation of the TUS protocol for dotnet to achieve resumable file uploads
- **Uploads**: SolidTUS for resumable uploads.

#### General Constraints
- **Software**: The system must be compatible with .NET 8 and as this is a new release, some libraries may not yet be fully compatible. This may result in adjustments in the software stack if unforeseen issues arise. 
- **Performance**: The API response times must be under 2 seconds for all user-related operations. For all photo related operations response time might be longer depending on the amount of data that is being uploaded. Here the use of TUS protocol is to handle resumable uploads in order not to start all over if there is a network issue.  
- **Security**: All user data must be secured, particularly authentication data. Passwords will be encrypted in the database. JWT will be used to handle secure authorization. 
- **Testing**: Integration tests have been created for most of the features done thus far. The choice for integration over unit tests is due to integration tests providing greater value when it comes to testing the upload of photos, since it does not make sense to test just bits and not the whole flow. Given the Clean Architecture design, testing will focus on business logic separated from infrastructure concerns. 
- **Interoperability**: The API is designed in a RESTful manner with standardized endpoints to facilitate easy integration with web and mobile applications. Swagger documentation will be generated to ensure developers integrating the API can do so with minimal friction.

#### Goals and Guidelines
- **SOLID Principles**: The system is designed using the SOLID design principles to ensure maintainability, extensibility and testability. Each class and function have a single responsibility, interfaces are used where necessary to decouple components and dependencies are injected to promote flexibility.
- **CQRS with MediatR**: The separation of command and query responsibilities ensures a clear distinction between operations that alter system state and those that retrieve data. MediatR is used to handle communication between layers, ensuring loose coupling and adherence to the Single Responsibility Principle.

---

### Architectural Strategies

#### Clean Architecture
With this architecture it is intended to separate the system into layers: API, application, domain and infrastructure. This ensures that the business logic is independent of external dependencies, such as the database or mail client, allowing for greater flexibility, testability and scalability 

- **Reasoning**: It enforces the SOLID principles and allows the system to remain flexible for future enhancements. It ensures that changes to the layer do not affect others facilitating easier maintenance and testing. Due to the application being smaller, it could have been done otherwise and evade the complexity of setting up clean architecture, but the team wanted to extend their knowledge and try something new.  

- **Alternatives**: A traditional Layered Architecture was considered, but this would tightly couple the business logic with the infrastructure and database layers, making future scalability and testing more difficult. Clean Architecture was chosen because it supports the long-term maintainability of the system. 

#### CQRS
The use of CQRS in combination with MediatR allows for separation of commands (for write operations) and queries (for read operations). This improves performance by allowing commands and queries to be optimized independently.  

- **Reasoning**: By separating commands and queries, the API can better handle scalability and future performance enhancements, especially when dealing with complex operations such as photo directory upload. CQRS also allows for future optimizations like event sourcing if needed. With queries being separated into cases like returning object data lists which come from the database and loading photos which are grabbed from the file system of the server. 

- **Alternatives**: A simple CRUD (Create, Read, Update, Delete) approach was considered, but due to the nature of the app having relatively small database and most of the persistence being photos that are uploaded along with CQRS already being a good addition to Clear Architecture this seemed better. Although CQRS brings more complexity initially, having the possibility to tackle photo upload and database changes separately making the codebase more modular and easier to maintain seems like a good tradeoff. 

#### MediatR for Command/Query Handling
The MediatR library is used to handle communication between the layers of the application, specifically for dispatching commands and queries in the CQRS pattern. MediatR ensures loose coupling between the different parts of the application by centralizing how commands and queries are processed.  

- **Reasoning**: Using MediatR allows for better organization of code and helps enforce the separation of concerns that Clean Architecture promotes. It also provides an easy way to introduce cross-cutting concerns such as logging, validation and error handling through pipeline behaviors. It goes well with CQRS and makes the codebase more structured. 

- **Alternatives**: Without MediatR, a more traditional approach using service classes for handling business logic could have been used. However, this would lead to more tightly coupled components, reducing flexibility in managing the command and query flow. MediatR was chosen because of its simplicity and ability to improve code modularity and maintainability. 

#### Database Choice and Persistence
The API uses PostgreSQL as the primary database system saving the user data and photo metadata as JSON strings in columns of the tables. This is because Marten is providing a document DB that works with PostgreSQL. The original photos are saved in a directory on the server created for that user  

- **Reasoning**: Marten was chosen because it needs less mechanical work for mapping and configuration. This way we need less effort for schema management and database migrations. Additionally, Marten is a community OSS project which goes great with our aim for this application. Having Marten’s event store also makes it possible to have tracking of historical data, such as modifications to photos.  

- **Alternatives**: Alternatives like using Dapper (a micro-ORM) were considered and tried, but later it was decided to switch to Marten and store the data as JSON taking advantage of PostgreSQL advanced JSON features.  

#### TUS for Resumable Uploads
The application uses the TUS protocol as the primary mechanism for handling the upload of photos. TUS is an open protocol specifically designed for resumable uploads, ensuring that large files, interrupted transfers or unreliable network connections do not disrupt the user experience. There is an additional flow diagram for TUS protocol that can be found in the references of this document.  

- **Reasoning**: It provides robust support for interrupted transfers, ensuring that no data is lost due to network issues. This is especially good for future implementation of video uploads since the protocol is designed to handle large files and high-volume upload scenarios. TUS is also an open-source protocol with strong community backing, aligning well with the application’s open-source ethos.  

- **Alternatives**: Direct File Uploads with Retries is also possible and implemented in the application, this can be used for uploading photos although it can lead to poor user experience during connection failures. Multipart Form Data with Chunked uploads was considered but not used due to complexity it adds to the backend for handling chunk assembly and validation. 

#### Logging and Monitoring
Serilog is used for logging errors, warnings and information. Logs are written in files for starters but can be also added to external monitoring tools (Application Insights or Logstash) to track API performance and diagnose issues in production. For now, there is also an endpoint /logs that gets the server logs for the past day, which can be used by developers that don’t have access to the server.  

- **Reasoning**: Serilog provides powerful logging capabilities with easy integration into .NET applications. It supports structured logging, making it easier to query and analyze logs in production environments. 

- **Alternatives**: Nlog and log4net were considered but lacked the flexibility and integration with modern tools like Serilog, which offers better support for structured logging.   

---

### System Architecture

#### System Decomposition
1. **Presentation Layer**: Outermost layer that handles incoming HTTP requests and routes them to the appropriate handlers. It exposes the API endpoints, which can be consumed to client applications (such as web or mobile apps). The key role of this layer is to map requests to the corresponding command or query and return the appropriate response to the client. The technologies used here are Ardalis Endpoints, MediatR, Swagger for API documentation
2. **Application Layer**: Serves as the mediator between the presentation and domain layers. It contains the business logic that directs commands and queries to their respective handlers, which are responsible for carrying out the appropriate actions. The layer ensures that the correct flow of control happens while maintaining separation from the infrastructure. 
3. **Domain Layer**: Contains the core business logic and the domain models. It also handles domain-specific rules and validations. This layer is isolated from the infrastructure and application layers to ensure that the business logic remains independent and reusable. 
4. **Infrastructure Layer**: The infrastructure layer handles all external interactions such as database access, file storage and third-party integrations. In this case it handles data persistance and retrieval using the document sesion from Marten and implements repositories to abstract data access logic. The technoligies used here are Marten, MailKit and ImageSharp  

---

### Policies and Tactics
- **IDE**: The project is developed using Visual Studio 2022. This IDE offers tight integration with the .NET ecosystem, comprehensive debugging tools and powerful features for code management.
- **Compiler**: The API is compiled using .NET SDK 8 which includes the latest C# compiler optimized for the .NET 8 runtime.
- **C# Coding Standards**: The project follows standard C# coding conventions, including naming conventions, best practices and avoiding deep nesting for readability. 
- **Version Control**: Git is used for version control, allowing the team to track changes, collaborate effectively and manage branching strategies for feature development. The repository will be hosted on GitHub which provides additional tools for collaboration, issue tracking and automated CI/CD workflows. 
- **Coding Standards**: The code follows SOLID principles (Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation, Dependency Inversion) to ensure maintainable and extensible code. 
- **Code Reviews**: All code is subject to code reviews to ensure that it meets quality standards, follows the coding conventions and adheres to the architectural guidelines. 

---

© 1999 Karl E. Wiegers. Permission granted to use, modify, and distribute this document.
