// using System;

// public class SMIQueue
//     {
//         public string QUEUE_NAME { get; set; } = "SPS";

//         public class QueueItem
//         {
//             public DateTime DateCreated { get; set; }
//             public string Key { get; set; }
//             public string Value { get; set; }
//             public string PostalCode { get; set; }
//             public string ServiceCode { get; set; }
//             public string ProductCode { get; set; }
//             public string AccNo { get; set; }
//         }

//         //**************************************************
//         // Provides an entry point into the application.
//         //		
//         // This example posts a notification that a message
//         // has arrived in a queue. It sends a message
//         // containing an other to a separate queue, and then
//         // peeks the first message in the queue.
//         //**************************************************

//         public static void Main()
//         {
//             // Create a new instance of the class.
//             //SMIQueue myNewQueue = new SMIQueue();

//             // Wait for a message to arrive in the queue.
//             //myNewQueue.NotifyArrived();

//             // Send a message to a queue.
//             //myNewQueue.SendMessage("keye", "valu");

//             // Peek the first message in the queue.
//             //myNewQueue.PeekFirstMessage();

//             return;
//         }

//         public SMIQueue(string privateQueueName = null, bool? alwaysResetPermission = false)
//         {
//             QUEUE_NAME = string.IsNullOrWhiteSpace(privateQueueName) ? QUEUE_NAME : privateQueueName;

//             var path = ".\\private$\\" + QUEUE_NAME;

//             MessageQueue q = null;

//             if (MessageQueue.Exists(path))
//             {
//                 q = new MessageQueue(path);

//                 if (alwaysResetPermission.GetValueOrDefault())
//                 {
//                     #region Set Permissions
//                     string everyone = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null).Translate(typeof(System.Security.Principal.NTAccount)).Value;

//                     q.SetPermissions(
//                             everyone,
//                             MessageQueueAccessRights.FullControl,
//                             AccessControlEntryType.Allow);
//                     #endregion
//                 }
//             }
//             else
//             {
//                 q = MessageQueue.Create(path);

//                 #region Set Permissions
//                 string everyone = new System.Security.Principal.SecurityIdentifier(System.Security.Principal.WellKnownSidType.WorldSid, null).Translate(typeof(System.Security.Principal.NTAccount)).Value;

//                 q.SetPermissions(
//                         everyone,
//                         MessageQueueAccessRights.FullControl,
//                         AccessControlEntryType.Allow);
//                 #endregion
//             }
//         }

//         //**************************************************
//         // Posts a notification when a message arrives in
//         // the queue "monitoredQueue". Does not retrieve any
//         // message information when peeking the message.
//         //**************************************************

//         public void NotifyArrived()
//         {

//             // Connect to a queue.
//             MessageQueue myQueue = new
//                 MessageQueue(".\\private$\\" + QUEUE_NAME);

//             // Specify to retrieve no message information.
//             myQueue.MessageReadPropertyFilter.ClearAll();

//             // Wait for a message to arrive.
//             Message emptyMessage = myQueue.Peek();

//             // Post a notification when a message arrives.
//             Console.WriteLine("A message has arrived in the queue.");

//             return;
//         }

//         //**************************************************
//         // Sends an Order to a queue.
//         //**************************************************

//         public void SendMessage(string key, string value, string postalCode = null, string serviceCode = null, string productCode = null, string accNo = null)
//         {

//             // Create a new order and set values.
//             QueueItem sentOrder = new QueueItem();
//             sentOrder.DateCreated = DateTime.Now;
//             sentOrder.Key = key;
//             sentOrder.Value = value;
//             sentOrder.PostalCode = postalCode;
//             sentOrder.ServiceCode = serviceCode;
//             sentOrder.ProductCode = productCode;
//             sentOrder.AccNo = accNo;

//             // Connect to a queue on the local computer.
//             MessageQueue myQueue = new MessageQueue(".\\private$\\" + QUEUE_NAME);

//             // Send the Order to the queue.
//             myQueue.Send(sentOrder);

//             return;
//         }

//         //**************************************************
//         // Peeks a message containing an Order.
//         //**************************************************

//         public QueueItem PeekFirstMessage()
//         {
//             QueueItem item = null;

//             // Connect to a queue.
//             MessageQueue myQueue = new MessageQueue(".\\private$\\" + QUEUE_NAME);

//             // Set the formatter to indicate the body contains an Order.
//             myQueue.Formatter = new XmlMessageFormatter(new Type[]
//                 {typeof(QueueItem)});

//             try
//             {
//                 // Peek and format the message.
//                 Message myMessage = myQueue.Peek();
//                 item = (QueueItem)myMessage.Body;

//                 // Display message information.
//                 Console.WriteLine("Key: " +
//                     item.Key.ToString());
//                 Console.WriteLine("Sent: " +
//                     item.DateCreated.ToString());
//             }

//             catch (MessageQueueException ex)
//             {
//                 // Handle Message Queuing exceptions.
//                 Log.Add(ex);
//             }

//             // Handle invalid serialization format.
//             catch (InvalidOperationException ex)
//             {
//                 Console.WriteLine(ex.Message);
//                 Log.Add(ex);
//             }

//             // Catch other exceptions as necessary.

//             return item;
//         }

//         public QueueItem ReceiveMessage()
//         {
//             QueueItem item = null;

//             // Connect to the a queue on the local computer.
//             MessageQueue myQueue = new MessageQueue(".\\private$\\" + QUEUE_NAME);

//             // Set the formatter to indicate body contains an Order.
//             myQueue.Formatter = new XmlMessageFormatter(new Type[]
//                 {typeof(QueueItem)});

//             try
//             {
//                 // Receive and format the message.
//                 Message myMessage = myQueue.Receive();
//                 item = (QueueItem)myMessage.Body;

//                 // Display message information.
//                 Console.WriteLine("Order ID: " +
//                     item.Key.ToString());
//                 Console.WriteLine("Sent: " +
//                     item.DateCreated.ToString());
//             }

//             catch (MessageQueueException ex)
//             {
//                 // Handle Message Queuing exceptions.
//                 Log.Add(ex);
//             }

//             // Handle invalid serialization format.
//             catch (InvalidOperationException ex)
//             {
//                 Console.WriteLine(ex.Message);
//                 Log.Add(ex);
//             }

//             // Catch other exceptions as necessary.

//             return item;
//         }
//     }