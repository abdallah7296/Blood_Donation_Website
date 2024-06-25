using BLL.Interfaces;
using DAL;
using DAL.Dtos.HospitalsDTO;
using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PL.Models;

namespace PL.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext context;
        private readonly IUnitOfWork unitOfWork;
        private readonly ILogger<AccountController> logger;

        public AccountController(UserManager<ApplicationUser> userManager,
                                 SignInManager<ApplicationUser> signInManager, ApplicationDbContext context,
                                 IUnitOfWork unitOfWork,
                                 ILogger<AccountController> logger)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.context = context;
            this.unitOfWork = unitOfWork;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> SignUp()
        {
            return View(new SignUpViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SignUp(SignUpViewModel input)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = input.UserName,
                    Email = input.Email,
                    PhoneNumber = input.PhoneNumber,
                };

                var result = await userManager.CreateAsync(user, input.Password);

                if (result.Succeeded)
                {
                    if (input.Role == "donor")
                    {
                        var donor = new Donor
                        {
                            UserName = input.UserName,
                            Email = input.Email,
                            PhoneNumber = input.PhoneNumber,
                            BloodType = input.BloodType,
                            Gender = input.Gender,
                            DateOfBirth = input.DateOfBirth,
                            LastDonationDate = input.LastDonationDate,
                            Governorate = input.Governorate,
                            Province = input.Province,
                            User = user,
                            UserId = user.Id
                        };
                        unitOfWork.DonorRepository.Add(donor);
                    }
                    else if (input.Role == "patient")
                    {
                        var patient = new Patient
                        {
                            UserName = input.UserName,
                            Email = input.Email,
                            PhoneNumber = input.PhoneNumber,
                            BloodType = input.BloodType,
                            Gender = input.Gender,
                            DateOfBirth = input.DateOfBirth,
                            Governorate = input.Governorate,
                            Province = input.Province,
                            User = user,
                            UserId = user.Id
                        };
                        unitOfWork.PatientRepository.Add(patient);
                    }
                    unitOfWork.Complete();
                    return RedirectToAction("Login");
                }

                foreach (var error in result.Errors)
                    //ModelState.AddModelError(string.Empty, error.Description); 
                    logger.LogError(error.Description);
            }
            return View(input);
        }

        [HttpGet]
        public IActionResult RegisterHospitals()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RegisterHospitals(HospitalDTO hospitalDTO)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = hospitalDTO.HospitalName, Email = hospitalDTO.Email   };
                var result = await userManager.CreateAsync(user,hospitalDTO.Password);

                if (result.Succeeded)
                {
                    var hospital = new Hospital
                    {
                        HospitalName = hospitalDTO.HospitalName,
                        Email = hospitalDTO.Email,
                        PhoneNumber = hospitalDTO.PhoneNumber,
                        Governorate = hospitalDTO.Governorate,
                        Province = hospitalDTO.Province,
                        Address = hospitalDTO.Address,
                        UserId = user.Id,
               
                    };
                    await context.Hospitals.AddAsync(hospital);
                    await context.SaveChangesAsync();

                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

            }
            return View(hospitalDTO);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(SignIn signIn)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = await userManager.FindByNameAsync(signIn.UserName);
                if (user != null)
                {
                    bool result = await userManager.CheckPasswordAsync(user, signIn.Password);
                    if (result == true)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                   
                }
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(signIn);
        }

    }
}