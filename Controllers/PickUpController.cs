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
using System.Security.Claims;

namespace CharityMS.Controllers
{
    public class PickUpController : Controller
    {
        private readonly UserManager<User> _userManager;

        private readonly CharityMSdbContext _indentityContext;
        private readonly CharityMSApplicationDbContext _context;

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
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build(); 

            List<string> KeyList = new List<string>();
            KeyList.Add(configure["AWSCredential:key1"]); //access key
            KeyList.Add(configure["AWSCredential:key2"]); //secret key
            KeyList.Add(configure["AWSCredential:key3"]); //session token

            return KeyList;
        }

        public async Task<IActionResult> Index(string? status)
        {
            List<PickUpVM> vm = new List<PickUpVM>();
            var pu = _context.PickUp.ToList();

            if (User.IsInRole("User"))
            {
                pu = pu.Where(x => x.DonorId.Equals(Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)))).ToList();
            }

            foreach (var i in pu)
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

            if(status != null)
            {
                vm = vm.Where(x => x.Status.Equals(status)).ToList();
            }
            
            return View(vm);
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pu =  await _context.PickUp.Include(i=>i.Donations).FirstOrDefaultAsync(i=>i.Id.Equals(id));
            var donor = await _indentityContext.Users.FirstOrDefaultAsync(m => m.Id.Equals(pu.DonorId.ToString()));
            if (pu == null)
            {
                return NotFound();
            }

            PickUpDetailVM vm = new PickUpDetailVM
            {
                Donor = donor,
                Id = pu.Id,
                Location = pu.Location,
                Status = pu.Status,
                EstimatiedPickUpDate = pu.EstimatedPickUpDate,
                Donations=pu.Donations
            };

            ViewBag.items = pu.Donations;
            if (pu.Status.Equals("Picked-Up"))
            {
                vm.PickUpDate = pu.PickUpDate;
                List<string> KeyList = getCredentialInfo();
                var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

                List<S3Object> s3imageList = new List<S3Object>();

                try
                {
                    string token = null;
                    do
                    {
                        ListObjectsRequest request = new ListObjectsRequest()
                        {
                            BucketName = bucketname,
                            Prefix= "prove/" + id.ToString()+"/",
                            Delimiter="/"
                        };
                        ListObjectsResponse response = await s3clientobject.ListObjectsAsync(request).ConfigureAwait(false); 
                        s3imageList.AddRange(response.S3Objects);
                        token = response.NextMarker; 
                    } while (token != null);

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

        public ActionResult Create()
        {
            PickUpVM vm = new PickUpVM();
            vm.Donations = new List<Item>();
            vm.EstimatiedPickUpDate = DateTime.Now;
            vm.Location = _userManager.GetUserId(HttpContext.User) != null ? _userManager.GetUserAsync(HttpContext.User).Result.Address : "";
            return View(vm);
        }

        public IActionResult BlankItem()
        {
            return PartialView("_DonationItem", new DonationItemVM());
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(PickUpVM vm)
        {
            ResultMessageModel resultMessageModel = new ResultMessageModel();

            if (!ModelState.IsValid)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "Invalid Data";
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return Json(resultMessageModel);
            }

            if(vm.Donations == null || vm.Donations.Where(x=>x.ItemName != null && x.Quantity > 0).Count() < 1)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "At least one donation item is required!";
                return Json(resultMessageModel);
            }

            try
            {
                var pickUp = new PickUp
                {
                    DonorId = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty,
                    Location = vm.Location,
                    EstimatedPickUpDate = vm.EstimatiedPickUpDate,
                    Status = "Registed",
                    Donations = vm.Donations
                };

                _context.PickUp.Add(pickUp);
                _context.SaveChanges();
                resultMessageModel.Result = 0;
                resultMessageModel.Message = "Create Pick-up Successfully";

            }
            catch(Exception ex)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "Some error occured when creating ! "+ ex.Message;

                ex.StackTrace.ToString();
            }

            return Json(resultMessageModel);
        }

        private async Task<PickUpDetailVM> getPickUpDetailsAsync(Guid? id)
        {

            var pu = await _context.PickUp.Include(i => i.Donations).FirstOrDefaultAsync(i => i.Id.Equals(id));
            var donor = await _indentityContext.Users.FirstOrDefaultAsync(m => m.Id.Equals(pu.DonorId.ToString()));
            if (pu == null)
            {
                return new PickUpDetailVM();
            }

            PickUpDetailVM vm = new PickUpDetailVM
            {
                Donor = donor,
                Id = pu.Id,
                Location = pu.Location,
                Status = pu.Status,
                EstimatiedPickUpDate = pu.EstimatedPickUpDate,
                Donations = pu.Donations
            };

            if (pu.Status.Equals("Picked-Up"))
            {
                vm.PickUpDate = pu.PickUpDate;
                List<string> KeyList = getCredentialInfo();
                var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

                List<S3Object> s3imageList = new List<S3Object>();

                try
                {
                    string token = null;
                    do
                    {
                        ListObjectsRequest request = new ListObjectsRequest()
                        {
                            BucketName = bucketname,
                            Prefix = "prove/" + id.ToString() + "/",
                            Delimiter = "/"
                        };
                        ListObjectsResponse response = await s3clientobject.ListObjectsAsync(request).ConfigureAwait(false);
                        s3imageList.AddRange(response.S3Objects);   
                        token = response.NextMarker;
                    } while (token != null);
                        
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Unable to read the images from S3! Error as here: " + ex.Message);
                }
                vm.images = s3imageList;
            }

            return vm;
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            PickUpDetailVM vm = await getPickUpDetailsAsync(id);

            return View(vm);
        }

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
                PickUpDetailVM vmDetail = await getPickUpDetailsAsync(id);

                vmDetail.Location = vm.Location;
                vmDetail.EstimatiedPickUpDate = vm.EstimatiedPickUpDate;
                vmDetail.Status = vm.Status;

                return View(vmDetail);
            }

            try
            {
                PickUp pu = await _context.PickUp.FirstOrDefaultAsync(i=>i.Id==id);

                pu.Location = vm.Location;
                pu.EstimatedPickUpDate = vm.EstimatiedPickUpDate;
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

        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var pu = await _context.PickUp.Include(i => i.Donations).FirstOrDefaultAsync(i => i.Id.Equals(id));

                foreach(var i in pu.Donations)
                {
                    _context.Item.Remove(i);
                }

                _context.PickUp.Remove(pu);
                _context.SaveChanges();

                List<string> KeyList = getCredentialInfo();
                var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

                List<S3Object> s3imageList = new List<S3Object>();

                string token = null;
                do
                {
                    ListObjectsRequest request = new ListObjectsRequest()
                    {
                        BucketName = bucketname,
                        Prefix = "prove/" + id.ToString() + "/",
                        Delimiter = "/"
                    };
                    ListObjectsResponse response = await s3clientobject.ListObjectsAsync(request).ConfigureAwait(false);
                    s3imageList.AddRange(response.S3Objects);
                    token = response.NextMarker;
                } while (token != null);

                foreach(var i in s3imageList)
                {
                    var deleteObjectRequest = new DeleteObjectRequest
                    {
                        BucketName = bucketname,
                        Key = i.Key,
                    };

                    await s3clientobject.DeleteObjectAsync(deleteObjectRequest);
                }

                return RedirectToAction("Index");
            }
            catch(Exception ex)
            {
                Console.Write(ex);
                return RedirectToAction("Index");
            }
        }

        public ActionResult UpdateStatusProve(Guid id)
        {
            ViewBag.id = id;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatusProve(Guid id,List<IFormFile> images)
        {
            List<string> KeyList = getCredentialInfo();
            var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            string filename = "";
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

                try
                {
                    PutObjectRequest uploadRequest = new PutObjectRequest
                    {
                        InputStream = image.OpenReadStream(),
                        BucketName = bucketname + "/prove/"+id.ToString(),
                        Key = image.FileName,
                        CannedACL = S3CannedACL.PublicRead
                    };

                    //b. execute your request command
                    await s3clientobject.PutObjectAsync(uploadRequest);
                    filename = filename + " " + image.FileName + ",";

                    PickUp pu = await _context.PickUp.FirstOrDefaultAsync(m => m.Id == id);

                    pu.Status = "Picked-Up";
                    pu.PickUpDate = DateTime.Now;
                    pu.StaffId = User.FindFirstValue(ClaimTypes.NameIdentifier) != null ? Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)) : Guid.Empty;
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

        public async Task<IActionResult> deleteImage(string filename, string pid)
        {
            List<string> KeyList = getCredentialInfo();
            var s3clientobject = new AmazonS3Client(KeyList[0], KeyList[1], KeyList[2], RegionEndpoint.USEast1);

            ResultMessageModel resultMessageModel = new ResultMessageModel();

            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = bucketname,
                    Key = filename,
                };

                await s3clientobject.DeleteObjectAsync(deleteObjectRequest);
                resultMessageModel.Result = 0;
                resultMessageModel.Message = filename + " is successfully deleted from the S3 Bucket!";
            }
            catch (AmazonS3Exception ex)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "Error in deleting the " + filename + " : " + ex.Message;
            }
            catch (Exception ex)
            {
                resultMessageModel.Result = -1;
                resultMessageModel.Message = "Error in deleting the " + filename + " : " + ex.Message;
            }

            return Json(resultMessageModel);
            //return RedirectToAction("Edit", "PickUp", new { id = Guid.Parse(pid) });
        }
    }
}
