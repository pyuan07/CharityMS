using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CharityMS.Data;
using CharityMS.Models;
using Microsoft.AspNetCore.Identity;
using CharityMS.Areas.Identity.Data;
using Microsoft.Extensions.Configuration;
using System.IO;
using Newtonsoft.Json;
using Amazon.SQS.Model;
using Amazon.SQS;
using Amazon;
using CharityMS.ViewModels;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using System.Security.Claims;

namespace CharityMS.Controllers
{
    public class DonationsController : Controller
    {
        private const string queueName = "DonationRequestSQS";

        private readonly UserManager<User> _userManager;
        private readonly CharityMSApplicationDbContext _context;
        private readonly CharityMSdbContext _identityContext;

        public DonationsController(
                CharityMSApplicationDbContext context,
                CharityMSdbContext identityContext,
                UserManager<User> userManager
            )
        {
            _context = context;
            _userManager = userManager;
            _identityContext = identityContext;
        }

        private List<string> getCredentialInfo()
        {
            //1. how to link to the appsettings.json
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); //build the json file

            //2. read the info from json using configure instance
            List<string> KeyList = new List<string>();
            KeyList.Add(configure["AWSCredential:key1"]); //access key
            KeyList.Add(configure["AWSCredential:key2"]); //secret key
            KeyList.Add(configure["AWSCredential:key3"]); //session token

            //3. return keys to function that needed
            return KeyList;
        }

        //public async Task<IActionResult> Index()
        //{

        //    //connection
        //    List<string> keys = getCredentialInfo();
        //    var sqsClient = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

        //    //get the queue URL 
        //    var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });

        //    #region Get Number Of Queue
        //    GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
        //    attReq.QueueUrl = response.QueueUrl;
        //    attReq.AttributeNames.Add("ApproximateNumberOfMessages");
        //    GetQueueAttributesResponse response1 = await sqsClient.GetQueueAttributesAsync(attReq);
        //    ViewBag.QueueCount = response1.ApproximateNumberOfMessages;
        //    #endregion


        //    #region Get Queue
        //    //create a list to store the returned messages
        //    //return message - customer information, receiptHandler (use for delete the message)
        //    List<KeyValuePair<DonationRequest, string>> messages = new List<KeyValuePair<DonationRequest, string>>();

        //    try
        //    {
        //        ReceiveMessageRequest receivedRequest = new ReceiveMessageRequest
        //        {
        //            QueueUrl = response.QueueUrl,
        //            WaitTimeSeconds = 10, //short pooling = 0(min) seconds, long pooling = 1-20(max) seconds
        //            MaxNumberOfMessages = 10,//how many msg you need per reading  
        //            VisibilityTimeout = 20 //decide how much time the item not viewable by other people while still reading 
        //        };

        //        ReceiveMessageResponse returnResponse = await sqsClient.ReceiveMessageAsync(receivedRequest);

        //        if (returnResponse.Messages.Count <= 0)
        //        {
        //            ViewBag.ErrorMsg = "No more message in the queue now";
        //            return View(messages);
        //        }

        //        for (int i = 0; i < returnResponse.Messages.Count; i++)
        //        {
        //            DonationRequest donation = JsonConvert.DeserializeObject<DonationRequest>(returnResponse.Messages[i].Body); //json back to object style
        //            messages.Add(new KeyValuePair<DonationRequest, string>(donation, returnResponse.Messages[i].ReceiptHandle));
        //        }
        //    }
        //    catch (AmazonSQSException ex)
        //    {
        //        ViewBag.ErrorMsg = ex.Message;
        //        return View(messages);
        //    }
        //    #endregion

        //    return View(messages);
        //}

        public async Task<IActionResult> Index(string? status)
        {
            List<DonationVM> donationVMList = new List<DonationVM>();
            IEnumerable<Donation> donationList = await _context.Donation.Include(x=>x.Donations).ToListAsync();

            if (status != null)
            {
                donationList = donationList.Where(x => x.Status.Equals(status)).ToList();
            }

            donationList.ToList().ForEach( d =>
            {
                donationVMList.Add(new DonationVM()
                {
                    StaffFullName = _identityContext.Users.FirstOrDefault(m => m.Id == d.StaffId.ToString()) != null ? _identityContext.Users.FirstOrDefault(m => m.Id == d.StaffId.ToString()).FullName : "Unknown",
                    StaffId = d.StaffId,
                    Status = d.Status,
                    Date = d.Date,
                    Donations = d.Donations,
                    Id = d.Id,
                    Reason = d.Reason,
                    ReceiverFullName = _identityContext.Users.FirstOrDefault(m => m.Id == d.ReceiverId.ToString()) != null ? _identityContext.Users.FirstOrDefault(m => m.Id == d.ReceiverId.ToString()).FullName: "Unknown",
                    ReceiverId = d.ReceiverId
                });
            });

            return View(donationVMList);
        }

        public async Task<IActionResult> CheckRequest()
        {

            #region Get Queue
            //connection
            List<string> keys = getCredentialInfo();
            var sqsClient = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //get the queue URL 
            var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });

            DonationRequest donation = new DonationRequest();
            try
            {
                //Define the details in ReceiveMessageRequest
                ReceiveMessageRequest receivedRequest = new ReceiveMessageRequest
                {
                    QueueUrl = response.QueueUrl,
                    WaitTimeSeconds = 10, //short pooling = 0(min) seconds, long pooling = 1-20(max) seconds
                    MaxNumberOfMessages = 10,//how many msg you need per reading  
                    VisibilityTimeout = 20 //decide how much time the item not viewable by other people while still reading 
                };

                //Send the Request and return the Response from AWS
                ReceiveMessageResponse returnResponse = await sqsClient.ReceiveMessageAsync(receivedRequest);


                if (returnResponse.Messages.Count <= 0)
                {
                    ViewBag.ErrorMsg = "No more message in the queue now";
                    ViewBag.QueueCount = "0";
                    return View();
                }

                //DeserializeObject the message from Json to Object,DonationRequest
                donation = JsonConvert.DeserializeObject<DonationRequest>(returnResponse.Messages[0].Body);
                ViewBag.DeleteToken = returnResponse.Messages[0].ReceiptHandle; // ReceiptHandle will be used when 
                ViewBag.Data = returnResponse.Messages[0].Body;
            }
            catch (AmazonSQSException ex)
            {
                ViewBag.ErrorMsg = ex.Message;
                return View();
            }
            #endregion

            #region Get Number Of Queue
            GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
            attReq.QueueUrl = response.QueueUrl;
            attReq.AttributeNames.Add("ApproximateNumberOfMessages");
            attReq.AttributeNames.Add("ApproximateNumberOfMessagesDelayed");
            attReq.AttributeNames.Add("ApproximateNumberOfMessagesNotVisible");
            GetQueueAttributesResponse response1 = await sqsClient.GetQueueAttributesAsync(attReq);
            ViewBag.QueueCount = response1.ApproximateNumberOfMessages + response1.ApproximateNumberOfMessagesDelayed + response1.ApproximateNumberOfMessagesNotVisible;
            #endregion

            return View(donation);
        }


        public async Task<IActionResult> deleteMessage(string deleteToken, string answer, string body)
        {
            DonationRequest donationRequest = JsonConvert.DeserializeObject<DonationRequest>(body);

            #region Delete the Message
            //Establish connection by using the keys.
            List<string> keys = getCredentialInfo();
            var sqsClient = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //Get the Queue URL by providing the Queue Name from connection
            var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });

            try
            {
                //Define the ReceiptHandle of the message that want to delete in DeleteMessageRequest
                DeleteMessageRequest deleteMessageRequest = new DeleteMessageRequest
                {
                    QueueUrl = response.QueueUrl,
                    ReceiptHandle = deleteToken
                };

                ////Send the Request to the AWS SQS
                await sqsClient.DeleteMessageAsync(deleteMessageRequest);

               
            }
            catch (AmazonSQSException ex)
            {
                TempData["errorMsg"] = "Unable to accept/reject the message from the queue! "+ex.Message;
                return RedirectToAction("Index");
            }
            #endregion

            Donation donation = new Donation()
            {
                Id = donationRequest.Id,
                Donations = donationRequest.Donations,
                Reason = donationRequest.Reason,
                ReceiverId = donationRequest.ReceiverId,
                Date = DateTime.Now,
                StaffId = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty,
                Status = answer
            };

            TempData["donation"] = JsonConvert.SerializeObject(donation);

            return RedirectToAction("Create");
        }


        // GET: Donations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var donation = await _context.Donation.Include(x=>x.Donations)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (donation == null)
            {
                return NotFound();
            }

            DonationVM donationVM = new DonationVM()
            {
                StaffFullName = _identityContext.Users.FirstOrDefault(m => m.Id == donation.StaffId.ToString()) != null ? _identityContext.Users.FirstOrDefault(m => m.Id == donation.StaffId.ToString()).FullName : "Unknown",
                StaffId = donation.StaffId,
                Status = donation.Status,
                Date = donation.Date,
                Donations = donation.Donations,
                Id = donation.Id,
                Reason = donation.Reason,
                ReceiverFullName = _identityContext.Users.FirstOrDefault(m => m.Id == donation.ReceiverId.ToString()) != null ? _identityContext.Users.FirstOrDefault(m => m.Id == donation.ReceiverId.ToString()).FullName : "Unknown",
                ReceiverId = donation.ReceiverId
            };

            return View(donationVM);
        }

        public async Task<IActionResult> CreateRequest()
        {

            DonationRequest donation = new DonationRequest()
            {
                Donations = new List<Item>()    
            };

            ViewBag.UserFullname = _userManager.GetUserId(HttpContext.User) != null ? _userManager.GetUserAsync(HttpContext.User).Result.FullName : "Unknown";

            #region Get Number Of Queue
            //Establish connection by using the keys. 
            List<string> keys = getCredentialInfo();
            var sqsClient = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //Get the Queue URL by providing the Queue Name from connection
            var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });

            //Define the GetQueueAttributesRequest to get the Total Approximate Number Of Messages
            GetQueueAttributesRequest attReq = new GetQueueAttributesRequest();
            attReq.QueueUrl = response.QueueUrl;
            attReq.AttributeNames.Add("ApproximateNumberOfMessages");
            attReq.AttributeNames.Add("ApproximateNumberOfMessagesDelayed");
            attReq.AttributeNames.Add("ApproximateNumberOfMessagesNotVisible");
            //Send the Request and return the Response from AWS
            GetQueueAttributesResponse response1 = await sqsClient.GetQueueAttributesAsync(attReq);

            ViewBag.QueueCount = response1.ApproximateNumberOfMessages + 
                                 response1.ApproximateNumberOfMessagesDelayed + 
                                 response1.ApproximateNumberOfMessagesNotVisible;
            #endregion

            return View(donation);      
        }

        public IActionResult BlankItem()
        {
            return PartialView("_DonationItem", new DonationItemVM());
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest(DonationRequest donation)
        {
            ResultMessageModel resultMessageModel = new ResultMessageModel();

            if (donation.Donations == null || donation.Donations.Where(x => x.ItemName != null && x.Quantity > 0).Count() < 1)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "At least one requested item is required!";
                return Json(resultMessageModel);
            }

            donation.Id = Guid.NewGuid();
            donation.ReceiverId = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty;

            #region Send Message Queue to SQS
            //Establish connection by using the keys.
            List<string> keys = getCredentialInfo();
            var sqsClient = new AmazonSQSClient(keys[0], keys[1], keys[2], RegionEndpoint.USEast1);

            //Get the Queue URL by providing the Queue Name from connection
            var response = await sqsClient.GetQueueUrlAsync(new GetQueueUrlRequest { QueueName = queueName });

            try
            {
                //Define the message want to send in SendMessageRequest
                SendMessageRequest queueMessage = new SendMessageRequest()
                {
                    QueueUrl = response.QueueUrl,
                    MessageBody = JsonConvert.SerializeObject(donation),
                };

                //Send the Request to the AWS SQS
                await sqsClient.SendMessageAsync(queueMessage);

                resultMessageModel.Result = 0;
                resultMessageModel.Message = "Your donations request already sent to the charity service!\n Your track number is: " + donation.Id;
            }
            //Catch the AmazonSQSException if there are any exception when sending the message to queue
            catch (AmazonSQSException ex)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "Unable to send your donation request. Please try again!\n" + ex.Message;
            }
            #endregion

            return Json(resultMessageModel);
        }

        public IActionResult Create()
        {
            if(TempData["donation"] == null)
            {
                return View(new Donation());
            }

            Donation donation = JsonConvert.DeserializeObject<Donation>(TempData["donation"].ToString());

            ViewBag.ApplicantFullname = _identityContext.Users.FirstOrDefault(m => m.Id == donation.ReceiverId.ToString()) != null ? _identityContext.Users.FirstOrDefault(m => m.Id == donation.ReceiverId.ToString()).FullName : "Unknown";
            ViewBag.StaffFullname = _userManager.GetUserId(HttpContext.User) != null ? _userManager.GetUserAsync(HttpContext.User).Result.FullName : "Unknown";

            return View(donation);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Donation donation)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return View(donation);
            }

            donation.StaffId = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty;

            _context.Add(donation);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> ReceivedDonation(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            try 
            {
                var donation = await _context.Donation.FindAsync(id);
                if (donation == null)
                {
                    return NotFound();
                }

                donation.Status = "received";

                _context.Donation.Update(donation);
                _context.SaveChanges();

                TempData["successMsg"] = "Received successfully";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["errorMsg"] = "Failed to receive";

                Console.WriteLine(ex);
            }


            return RedirectToAction("Index");
        }

        private bool DonationExists(Guid id)
        {
            return _context.Donation.Any(e => e.Id == id);
        }
    }
}
