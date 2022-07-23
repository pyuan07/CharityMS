using CharityMS.Areas.Identity.Data;
using CharityMS.Data;
using CharityMS.Models;
using CharityMS.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3.Model; //define the s3 object structure
using Amazon.S3; //s3 bucket
using System.IO; //file reading
using Amazon; //S3 account location
using Microsoft.Extensions.Configuration; //link to the appsettings.json - get info key
using Microsoft.AspNetCore.Http; //upload file from user pc to the network
using Amazon.S3.Transfer;
using System.Net.Mime;

namespace CharityMS.Controllers
{
    public class PickUpController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly CharityMSdbContext _indentityContext;
        private readonly CharityMSApplicationDbContext _context;

        private readonly string staffId = "07df0a49-2de5-4ce0-b9f8-010d3230caaf";
        private readonly string donorId = "05e94752-51ac-4ad3-acef-5a7eac1ddea8";
        private readonly string bucketname = "charitymstp057978";

        public PickUpController(
                CharityMSApplicationDbContext context,
                CharityMSdbContext indentityContext,
                UserManager<User> userManager
            )
        {
            _context = context;
            _indentityContext = indentityContext;
            _userManager = userManager;
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

        public async Task<IActionResult> Index()
        {
            List<PickUpVM> vm = new List<PickUpVM>();
            var pu = _context.PickUp.ToList();

            foreach(var i in pu)
            {
                User tempDonor = _indentityContext.Users.FirstOrDefault(m=>m.Id.Equals(i.DonorId.ToString()));
                PickUpVM temp = new PickUpVM
                {
                    Id=i.Id,
                    Location = i.Location,
                    Donations = i.Donations,
                    Status = i.Status,
                    EstimatiedPickUpDate=i.EstimatedPickUpDate
                };

                temp.Donor = tempDonor;
                vm.Add(temp);
            }
                return View(vm);
        }

        // GET: UserController/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pu =  await _context.PickUp.Include(i=>i.Donations).FirstOrDefaultAsync(i=>i.Id.Equals(id));
            if (pu == null)
            {
                return NotFound();
            }

            PickUpDetailVM vm = new PickUpDetailVM
            {
                Id = pu.Id,
                Location = pu.Location,
                Status = pu.Status,
                EstimatiedPickUpDate = pu.EstimatedPickUpDate,
                Donations=pu.Donations
            };

            if (pu.Status.Equals("Picked-Up"))
            {
                vm.PickUpDate = pu.PickUpDate;
                List<string> KeyList = getCredentialInfo();
                var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

                List<S3Object> s3imageList = new List<S3Object>(); //use for storing the images objects to the frontend

                //2. start to collect the images 1 by 1 from the S3
                try
                {
                    //create token - next marker info
                    string token = null;
                    do
                    {
                        ListObjectsRequest request = new ListObjectsRequest() //read the items form the bucket now
                        {
                            BucketName = bucketname,
                            Prefix= "prove/" + id.ToString()+"/",
                            Delimiter="/"
                        };
                        ListObjectsResponse response = await s3clientobject.ListObjectsAsync(request).ConfigureAwait(false); //return response from s3
                        s3imageList.AddRange(response.S3Objects);
                        token = response.NextMarker; // to determine whether still have next item in s3 or not
                    } while (token != null);

                    ViewBag.PreSignedURLList = getPreSignedURL(s3imageList, s3clientobject);
                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to read the images from S3! Error as here: " + ex.Message);
                }
                vm.images = s3imageList;
                return View(vm);
            }

            return View(vm);
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            PickUpVM vm = new PickUpVM();
            vm.Donations = new List<Item>();
            return View(vm);
        }

        public IActionResult BlankItem()
        {
            return PartialView("_DonationItem", new DonationItemVM());
        }

        // POST: UserController/Create
        [HttpPost]
        public async Task<IActionResult> CreateAsync(PickUpVM vm)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return View(vm);
            }

            try
            {
                User donor = _indentityContext.Users.FirstOrDefault(m => m.Id.Equals(donorId));

                var pickUp = new PickUp
                {
                    DonorId = Guid.Parse(donor.Id),
                    Location = vm.Location,
                    EstimatedPickUpDate = vm.EstimatiedPickUpDate,
                    Status = "Registed",
                    Donations = vm.Donations
                };

                _context.PickUp.Add(pickUp);
                _context.SaveChanges();
            }catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            return RedirectToAction(nameof(Index));
        }

        //GET: UserController/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pu = await _context.PickUp.FindAsync(id);
            PickUpVM vm = new PickUpVM
            {
                Donor = _indentityContext.Users.FirstOrDefault(m=>m.Id==pu.DonorId.ToString()),

            };
            if (vm == null)
            {
                return NotFound();
            }
            return View(vm);
        }

        // POST: UserController/Edit/5
        [HttpPost]
        public async Task<IActionResult> EditAsync(Guid id, PickUpVM vm)
        {
            if (id != vm.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return View(vm);
            }

            try
            {
                PickUp pu = await _context.PickUp.FirstOrDefaultAsync(i=>i.Id==id);

                pu.Location = vm.Location;
                pu.EstimatedPickUpDate = vm.EstimatiedPickUpDate;
                pu.Donations = vm.Donations;
                pu.Status = vm.Status;

                _context.PickUp.Update(pu);
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Console.WriteLine(ex);
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: UserController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: UserController/Delete/5
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            try
            {
                PickUp pu = _context.PickUp.Find(id);
                _context.PickUp.Remove(pu);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult UpdateStatusProve(Guid id)
        {
            ViewBag.id = id;
            return View();
        }

        //update status with prove
        [HttpPost]
        public async Task<IActionResult> UpdateStatusProve(Guid id,List<IFormFile> images)
        {
            List<string> KeyList = getCredentialInfo();
            var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            string filename = ""; //collecting the filename for display in msg
            //2. upload images one by one
            foreach (var image in images)
            {
                //2.1 small input validation
                if (image.Length <= 0)
                {
                    return BadRequest(image.FileName + " is having empty content! So unable to upload to the S3");
                }
                else if (image.Length > 1048576)
                {
                    return BadRequest(image.FileName + " is exceeded 1 MB! So unable to upload to the S3");
                }
                else if (image.ContentType.ToLower() != "image/png" && image.ContentType.ToLower() != "image/jpeg"
                    && image.ContentType.ToLower() != "image/gif")
                {
                    return BadRequest(image.FileName + "is not a valid picture for uploading! Please change the file!");
                }

                //2.2 all passed then start sending to the S3 bucket
                try
                {
                    //a. create the upload request for the S3
                    PutObjectRequest uploadRequest = new PutObjectRequest
                    {
                        InputStream = image.OpenReadStream(), //what is the source file
                        BucketName = bucketname + "/prove/"+id.ToString(), //bucket path or bucket with folder path
                        Key = image.FileName, //object name
                        CannedACL = S3CannedACL.PublicRead //open to display in any browser
                    };

                    //b. execute your request command
                    await s3clientobject.PutObjectAsync(uploadRequest);
                    filename = filename + " " + image.FileName + ",";

                    PickUp pu = await _context.PickUp.FirstOrDefaultAsync(m => m.Id == id);

                    pu.Status = "Picked-Up";
                    pu.PickUpDate = DateTime.Now;
                    pu.StaffId = Guid.Parse(staffId);
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    return BadRequest("Unable to upload to S3. Error as here: " + ex.Message);
                }
            }

            return RedirectToAction("Index", "PickUp",
                new { msg = "Images of " + filename + " already uploaded to the S3" });
        }

        public List<string> getPreSignedURL(List<S3Object> s3imageList, AmazonS3Client s3clientobject)
        {
            List<string> PreSignedURLList = new List<string>();

            foreach (var image in s3imageList)
            {
                // Create a get the PreSigned URL request
                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
                {
                    BucketName = image.BucketName,
                    Key = image.Key,
                    Expires = DateTime.Now.AddMinutes(1)
                };
                PreSignedURLList.Add(s3clientobject.GetPreSignedURL(request));
            }
            return PreSignedURLList;
        }
    }
}
