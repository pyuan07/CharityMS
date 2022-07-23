using Amazon.DynamoDBv2.Model;
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

namespace CharityMS.Controllers
{
    public class UsersController : Controller
    {

        private readonly UserManager<User> _userManager;

        private readonly CharityMSdbContext _indentityContext;
        private readonly CharityMSApplicationDbContext _context;

        public UsersController(
                CharityMSApplicationDbContext context,
                CharityMSdbContext indentityContext,
                UserManager<User> userManager
            )
        {
            _context = context;
            _indentityContext = indentityContext;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string? role)
        {
            ViewBag.Role = role;
            if(role != null)
            {
                IEnumerable<User> users = await _userManager.GetUsersInRoleAsync(role);
                return View(users);
            }
            else
            {
                return View(_indentityContext.Users.ToList());
            }
        }

        // GET: UserController/Details/5
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _indentityContext.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: UserController/Create
        public ActionResult Create()
        {
            return View(new UserVM());
        }

        // POST: UserController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAsync(ViewModels.UserVM userVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return View(userVM);
            }

            var user = new User
            {
                UserName = userVM.Email,
                Email = userVM.Email,
                Address = userVM.Address,
                FullName = userVM.FullName,
                NormalizedUserName = userVM.FullName,
                PhoneNumber = userVM.PhoneNumber,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, userVM.Password);
            if (result.Succeeded)
            {
                _userManager.AddToRoleAsync(user, userVM.Role).Wait();
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

            var user = await _indentityContext.Users.FindAsync(id.ToString());
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        // POST: UserController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, User user)
        {
            if (id.ToString() != user.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError(string.Empty, "Invalid Data");
                return View(user);
            }

            try
            {
                _indentityContext.Update(user);
                _indentityContext.SaveChanges();
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
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }


    }
}
