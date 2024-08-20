

### 1. **PaymentGateway.Api**
   - **Purpose**: This is the main API project. It exposes endpoints to handle payment processing requests.
   - **Controllers**:
     - `AuthController.cs`: Handles authentication and token management.
     - `PaymentController.cs`: Handles payment processing requests.
     - `HealthController.cs`: Provides a health check endpoint to monitor the API’s status.
   - **Middleware**:
     - `ExceptionHandlingMiddleware.cs`: Global exception handling middleware to capture and log errors.
   - **Configuration**:
     - `appsettings.json`: Contains the default configuration settings for the API.
     - `launchSettings.json`: Used to configure the environment settings during development.
   - **Dependencies**:
     - **NuGet Packages**: Various packages to support ASP.NET Core, logging, validation, etc.
     - **External Services**: Cosmos DB (via Azure SDK), Azurite (Azure Storage Emulator), and Seq (logging).

### 2. **PaymentGateway.Api.Application**
   - **Purpose**: Contains the core application logic from the API and offloads messages to the headless service.
   - **Commands**
     - **PaymentRequestCommand.cs**:
       - **Purpose**: Represents a command class for the data required to initiate a payment request.
     - **PaymentRequestHandler.cs**:
       - **Purpose**: Handles the execution of the `PaymentRequestCommand`.
       - **Details**:
         - Implements the logic required to process the payment request contained within the `PaymentRequestCommand`.
         - Responsible for sending a create transaction message to the queue
     - **PaymentRequestValidator.cs**:
       - **Purpose**: Validates the data within the `PaymentRequestCommand`.
       - **Details**:
         - Ensures that all necessary fields in the payment request are correctly filled out and adhere to business rules (e.g., the amount should be positive, the currency should be valid).

   - **Queries**
     - **RetrievePaymentQuery.cs**:
       - **Purpose**: Represents a query that for the data required to retrieve a specific payment's details.
       - **Details**:
         - This class contains properties necessary to identify and fetch the details of a payment, such as a payment ID.
     - **RetrievePaymentHandler.cs**:
       - **Purpose**: Handles the execution of the `RetrievePaymentQuery`.
       - **Details**:
         - Implements the logic required to retrieve the payment details from the cosmos database.

   - **Behaviors**:
     - `LoggingBehavior.cs`: Implements logging behavior for all requests.
     - `ValidationBehavior.cs`: Implements validation behavior for all requests.
   - **Dependency Injection**:
     - `DependencyInjection.cs`: Handles the registration of services and dependencies in the IoC container.

### 3. **Headless Service**
   - **Queues**:
     - **PaymentGateway.PaymentService**:
       - **EventProcessor**:
         - `EventHubSimulatorProcessor.cs`: Handles events from the simulated Event Hub and logs out message that would have been sent through http.
         - `PaymentEvent.cs`: Represents the event structure used in payment processing with events.
       - **PaymentProcessor**:
         - `CreatePaymentProcessor.cs`: Creates a new payment request, updates the status to `PROCESSING` and sends to next queue and event queue.
         - `PaymentGatewayHandlerService.cs`: Main handler to receives all incoming queue messages that relates to payments.
         - `ProcessPaymentsHandler.cs`: Manages the payment processing from `PROCESSING` to `DECLINED` or `AUTHORIZED`.
       - **Services**:
         - `PaymentQueueService.cs`: Sends queue messages for payments.
         - `EventHubSimulatorService.cs`: Send queue messages for events.

### 4. **PaymentGateway.Bank.Simulator**
   - **Purpose**: A simulator for bank interactions, which helps in testing the payment processing pipeline without needing access to real banking APIs.
   - **Processors**:
     - `BankSimulatorProcessor.cs`: Handles simulated bank interactions, such as authorizing payments and checking account balances.

### 5. **PaymentGateway.Common**
   - **Purpose**: Contains common utilities, extensions, and shared logic used across different projects in the solution.

### 6. **PaymentGateway.Contracts**
   - **Purpose**: Defines the data contracts (models and DTOs) used for communication between different components and services within the Payment Gateway solution.
   - **Exceptions**:
     - `ValidationException.cs`: Handles validation exceptions throughout the application.

### 7. **PaymentGateway.Infrastructure**
   - **Purpose**: Handles connection to Cosmos database.
   - **Repositories**:
     - `CosmosDbSettings.cs`: Configures the Cosmos DB connection settings.
     - `IPaymentRepository.cs`: Interface for the payment repository.
     - `PaymentRepository.cs`: Implements the payment repository logic.

### 8. **PaymentGateway.Api.Tests**
   - **Purpose**: Contains integration tests to ensure that the Payment Gateway API works as expected.
   - **Test Files**:
     - `AuthenticationApiShould.cs`: Tests the authentication flows.
     - `PaymentGatewayApiShould.cs`: Tests the payment processing flows, including validation and edge cases.
     - `IntegrationStartup.cs`: Configures the test environment to use in-memory databases or mock services as needed.

### 9. **PaymentGateway.PaymentService.Tests**
   - **Purpose**: Contains unit tests for the `PaymentGateway.PaymentService` project.