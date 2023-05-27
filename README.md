# azure-paas-functions-service-bus
Saga Pattern implementation with azure functions, azure service bus and azure sql

Azure Service Bus trigger for Azure Functions
=============================================

-   Article
-   04/04/2023
-   18 contributors

Feedback

Choose a programming language

C#JavaJavaScriptPowerShellPython

In this article
---------------

1.  [Example](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#example)
2.  [Attributes](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#attributes)
3.  [Usage](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#usage)
4.  [Connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#connections)

Show 4 more

Use the Service Bus trigger to respond to messages from a Service Bus queue or topic. Starting with extension version 3.1.0, you can trigger on a session-enabled queue or topic.

For information on setup and configuration details, see the [overview](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus).

Service Bus scaling decisions for the Consumption and Premium plans are made based on target-based scaling. For more information, see [Target-based scaling](https://learn.microsoft.com/en-us/azure/azure-functions/functions-target-based-scaling).

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#example)

Example
-------

A C# function can be created using one of the following C# modes:

-   [In-process class library](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library): compiled C# function that runs in the same process as the Functions runtime.
-   [Isolated worker process class library](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide): compiled C# function that runs in a worker process that is isolated from the runtime. Isolated worker process is required to support C# functions running on non-LTS versions .NET and the .NET Framework.
-   [C# script](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference-csharp): used primarily when creating C# functions in the Azure portal.

-   [In-process](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_2_in-process)
-   [Isolated process](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_2_isolated-process)
-   [C# Script](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_2_csharp-script)

The following example shows a [C# function](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library) that reads [message metadata](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#message-metadata) and logs a Service Bus queue message:

C#Copy

```
[FunctionName("ServiceBusQueueTriggerCSharp")]
public static void Run(
    [ServiceBusTrigger("myqueue", Connection = "ServiceBusConnection")]
    string myQueueItem,
    Int32 deliveryCount,
    DateTime enqueuedTimeUtc,
    string messageId,
    ILogger log)
{
    log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");
    log.LogInformation($"EnqueuedTimeUtc={enqueuedTimeUtc}");
    log.LogInformation($"DeliveryCount={deliveryCount}");
    log.LogInformation($"MessageId={messageId}");
}

```

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#attributes)

Attributes
----------

Both [in-process](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library) and [isolated worker process](https://learn.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide) C# libraries use the [ServiceBusTriggerAttribute](https://github.com/Azure/azure-functions-servicebus-extension/blob/master/src/Microsoft.Azure.WebJobs.Extensions.ServiceBus/ServiceBusTriggerAttribute.cs) attribute to define the function trigger. C# script instead uses a function.json configuration file.

-   [In-process](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_5_in-process)
-   [Isolated process](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_5_isolated-process)
-   [C# script](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_5_csharp-script)

The following table explains the properties you can set using this trigger attribute:

| Property | Description |
| --- | --- |
| QueueName | Name of the queue to monitor. Set only if monitoring a queue, not for a topic. |
| TopicName | Name of the topic to monitor. Set only if monitoring a topic, not for a queue. |
| SubscriptionName | Name of the subscription to monitor. Set only if monitoring a topic, not for a queue. |
| Connection | The name of an app setting or setting collection that specifies how to connect to Service Bus. See [Connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#connections). |
| Access | Access rights for the connection string. Available values are `manage` and `listen`. The default is `manage`, which indicates that the `connection` has the Manage permission. If you use a connection string that does not have the Manage permission, set `accessRights` to "listen". Otherwise, the Functions runtime might fail trying to do operations that require manage rights. In Azure Functions version 2.x and higher, this property is not available because the latest version of the Service Bus SDK doesn't support manage operations. |
| IsBatched | Messages are delivered in batches. Requires an array or collection type. |
| IsSessionsEnabled | `true` if connecting to a [session-aware](https://learn.microsoft.com/en-us/azure/service-bus-messaging/message-sessions) queue or subscription. `false` otherwise, which is the default value. |
| AutoComplete | `true` Whether the trigger should automatically call complete after processing, or if the function code will manually call complete.

If set to `true`, the trigger completes the message automatically if the function execution completes successfully, and abandons the message otherwise.

When set to `false`, you are responsible for calling [MessageReceiver](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.core.messagereceiver) methods to complete, abandon, or deadletter the message. If an exception is thrown (and none of the `MessageReceiver` methods are called), then the lock remains. Once the lock expires, the message is re-queued with the `DeliveryCount` incremented and the lock is automatically renewed. |

When you're developing locally, add your application settings in the [local.settings.json file](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-local#local-settings-file) in the `Values` collection.

See the [Example section](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#example) for complete examples.

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#usage)

Usage
-----

The following parameter types are supported by all C# modalities and extension versions:

| Type | Description |
| --- | --- |
| [System.String](https://learn.microsoft.com/en-us/dotnet/api/system.string) | Use when the message is simple text. |
| byte[] | Use for binary data messages. |
| Object | When a message contains JSON, Functions tries to deserialize the JSON data into known plain-old CLR object type. |

Messaging-specific parameter types contain additional message metadata. The specific types supported by the Service Bus trigger depend on the Functions runtime version, the extension package version, and the C# modality used.

-   [Extension v5.x](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_6_extensionv5_in-process)
-   [Functions 2.x and higher](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_6_functionsv2_in-process)
-   [Functions 1.x](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_6_functionsv1_in-process)

Use the [ServiceBusReceivedMessage](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusreceivedmessage) type to receive message metadata from Service Bus Queues and Subscriptions. To learn more, see [Messages, payloads, and serialization](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messages-payloads).

In [C# class libraries](https://learn.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library), the attribute's constructor takes the name of the queue or the topic and subscription.

You can also use the [ServiceBusAccountAttribute](https://github.com/Azure/azure-functions-servicebus-extension/blob/master/src/Microsoft.Azure.WebJobs.Extensions.ServiceBus/ServiceBusAccountAttribute.cs) to specify the Service Bus account to use. The constructor takes the name of an app setting that contains a Service Bus connection string. The attribute can be applied at the parameter, method, or class level. The following example shows class level and method level:

C#Copy

```
[ServiceBusAccount("ClassLevelServiceBusAppSetting")]
public static class AzureFunctions
{
    [ServiceBusAccount("MethodLevelServiceBusAppSetting")]
    [FunctionName("ServiceBusQueueTriggerCSharp")]
    public static void Run(
        [ServiceBusTrigger("myqueue", AccessRights.Manage)]
        string myQueueItem, ILogger log)
{
    ...
}

```

The Service Bus account to use is determined in the following order:

-   The `ServiceBusTrigger` attribute's `Connection` property.
-   The `ServiceBusAccount` attribute applied to the same parameter as the `ServiceBusTrigger` attribute.
-   The `ServiceBusAccount` attribute applied to the function.
-   The `ServiceBusAccount` attribute applied to the class.
-   The `AzureWebJobsServiceBus` app setting.

When the `Connection` property isn't defined, Functions looks for an app setting named `AzureWebJobsServiceBus`, which is the default name for the Service Bus connection string. You can also set the `Connection` property to specify the name of an application setting that contains the Service Bus connection string to use.

For a complete example, see [the examples section](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#example).

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#connections)

Connections
-----------

The `connection` property is a reference to environment configuration which specifies how the app should connect to Service Bus. It may specify:

-   The name of an application setting containing a [connection string](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#connection-string)
-   The name of a shared prefix for multiple application settings, together defining an [identity-based connection](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#identity-based-connections).

If the configured value is both an exact match for a single setting and a prefix match for other settings, the exact match is used.

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#connection-string)

### Connection string

To obtain a connection string, follow the steps shown at [Get the management credentials](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues#get-the-connection-string). The connection string must be for a Service Bus namespace, not limited to a specific queue or topic.

This connection string should be stored in an application setting with a name matching the value specified by the `connection` property of the binding configuration.

If the app setting name begins with "AzureWebJobs", you can specify only the remainder of the name. For example, if you set `connection` to "MyServiceBus", the Functions runtime looks for an app setting that is named "AzureWebJobsMyServiceBus". If you leave `connection` empty, the Functions runtime uses the default Service Bus connection string in the app setting that is named "AzureWebJobsServiceBus".

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#identity-based-connections)

### Identity-based connections

If you are using version 5.x or higher of the extension, instead of using a connection string with a secret, you can have the app use an [Azure Active Directory identity](https://learn.microsoft.com/en-us/azure/active-directory/fundamentals/active-directory-whatis). To do this, you would define settings under a common prefix which maps to the `connection` property in the trigger and binding configuration.

In this mode, the extension requires the following properties:

| Property | Environment variable template | Description | Example value |
| --- | --- | --- | --- |
| Fully Qualified Namespace | `<CONNECTION_NAME_PREFIX>__fullyQualifiedNamespace` | The fully qualified Service Bus namespace. | <service_bus_namespace>.servicebus.windows.net |

Additional properties may be set to customize the connection. See [Common properties for identity-based connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference#common-properties-for-identity-based-connections).

 Note

When using [Azure App Configuration](https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-azure-functions-csharp) or [Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview) to provide settings for Managed Identity connections, setting names should use a valid key separator such as `:` or `/` in place of the `__` to ensure names are resolved correctly.

For example, `<CONNECTION_NAME_PREFIX>:fullyQualifiedNamespace`.

When hosted in the Azure Functions service, identity-based connections use a [managed identity](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity?toc=/azure/azure-functions/toc.json). The system-assigned identity is used by default, although a user-assigned identity can be specified with the `credential` and `clientID` properties. Note that configuring a user-assigned identity with a resource ID is not supported. When run in other contexts, such as local development, your developer identity is used instead, although this can be customized. See [Local development with identity-based connections](https://learn.microsoft.com/en-us/azure/azure-functions/functions-reference#local-development-with-identity-based-connections).

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#grant-permission-to-the-identity)

#### Grant permission to the identity

Whatever identity is being used must have permissions to perform the intended actions. For most Azure services, this means you need to [assign a role in Azure RBAC](https://learn.microsoft.com/en-us/azure/role-based-access-control/role-assignments-steps), using either built-in or custom roles which provide those permissions.

 Important

Some permissions might be exposed by the target service that are not necessary for all contexts. Where possible, adhere to the principle of least privilege, granting the identity only required privileges. For example, if the app only needs to be able to read from a data source, use a role that only has permission to read. It would be inappropriate to assign a role that also allows writing to that service, as this would be excessive permission for a read operation. Similarly, you would want to ensure the role assignment is scoped only over the resources that need to be read.

You'll need to create a role assignment that provides access to your topics and queues at runtime. Management roles like [Owner](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#owner) aren't sufficient. The following table shows built-in roles that are recommended when using the Service Bus extension in normal operation. Your application may require additional permissions based on the code you write.

| Binding type | Example built-in roles |
| --- | --- |
| Trigger1 | [Azure Service Bus Data Receiver](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-service-bus-data-receiver), [Azure Service Bus Data Owner](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-service-bus-data-owner) |
| Output binding | [Azure Service Bus Data Sender](https://learn.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-service-bus-data-sender) |

1 For triggering from Service Bus topics, the role assignment needs to have effective scope over the Service Bus subscription resource. If only the topic is included, an error will occur. Some clients, such as the Azure portal, don't expose the Service Bus subscription resource as a scope for role assignment. In such cases, the Azure CLI may be used instead. To learn more, see [Azure built-in roles for Azure Service Bus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-managed-service-identity#resource-scope).

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#poison-messages)

Poison messages
---------------

Poison message handling can't be controlled or configured in Azure Functions. Service Bus handles poison messages itself.

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#peeklock-behavior)

PeekLock behavior
-----------------

The Functions runtime receives a message in [PeekLock mode](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-performance-improvements#receive-mode). It calls `Complete` on the message if the function finishes successfully, or calls `Abandon` if the function fails. If the function runs longer than the `PeekLock` timeout, the lock is automatically renewed as long as the function is running.

The `maxAutoRenewDuration` is configurable in *host.json*, which maps to [OnMessageOptions.MaxAutoRenewDuration](https://learn.microsoft.com/en-us/dotnet/api/microsoft.azure.servicebus.messagehandleroptions.maxautorenewduration). The default value of this setting is 5 minutes.

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#message-metadata)

Message metadata
----------------

Messaging-specific types let you easily retrieve [metadata as properties of the object](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-expressions-patterns#trigger-metadata). These properties depend on the Functions runtime version, the extension package version, and the C# modality used.

-   [Extension v5.x](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_7_extensionv5_in-process)
-   [Functions 2.x and higher](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_7_functionsv2_in-process)
-   [Functions 1.x](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#tabpanel_7_functionsv1_in-process)

These properties are members of the [ServiceBusReceivedMessage](https://learn.microsoft.com/en-us/dotnet/api/azure.messaging.servicebus.servicebusreceivedmessage) class.

| Property | Type | Description |
| --- | --- | --- |
| `ApplicationProperties` | `ApplicationProperties` | Properties set by the sender. |
| `ContentType` | `string` | A content type identifier utilized by the sender and receiver for application-specific logic. |
| `CorrelationId` | `string` | The correlation ID. |
| `DeliveryCount` | `Int32` | The number of deliveries. |
| `EnqueuedTime` | `DateTime` | The enqueued time in UTC. |
| `ScheduledEnqueueTimeUtc` | `DateTime` | The scheduled enqueued time in UTC. |
| `ExpiresAt` | `DateTime` | The expiration time in UTC. |
| `MessageId` | `string` | A user-defined value that Service Bus can use to identify duplicate messages, if enabled. |
| `ReplyTo` | `string` | The reply to queue address. |
| `Subject` | `string` | The application-specific label which can be used in place of the `Label` metadata property. |
| `To` | `string` | The send to address. |

[](https://learn.microsoft.com/en-us/azure/azure-functions/functions-bindings-service-bus-trigger?pivots=programming-language-csharp&tabs=python-v2%2Cin-process%2Cextensionv5#next-steps)