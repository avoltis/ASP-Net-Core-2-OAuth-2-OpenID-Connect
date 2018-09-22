using System;
using System.Threading.Tasks;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Mvc;
using Voltis.IDP.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Voltis.IDP.Controllers.UserRegistration;

namespace Voltis.IDP.Controllers.UserRegistration
{
    public class UserRegistrationController : Controller
    {
        private readonly IVoltisUserRepository _voltisUserRepository;
        private readonly IIdentityServerInteractionService _interaction;

        public UserRegistrationController(IVoltisUserRepository voltisUserRepository,
            IIdentityServerInteractionService interaction)
        {
            _voltisUserRepository = voltisUserRepository;
            _interaction = interaction;
        }

        [HttpGet]
        public IActionResult RegisterUser(RegistrationInputModel registrationInputModel)
        {
            var vm = new RegisterUserViewModel()
            {
                ReturnUrl = registrationInputModel.ReturnUrl,
                Provider = registrationInputModel.Provider,
                ProviderUserId = registrationInputModel.ProviderUserId
            };
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                //create user + claims
                var userToCreate = new Entities.User();
                userToCreate.Password = model.Password;
                userToCreate.Username = model.Username;
                userToCreate.IsActive = true;
                userToCreate.Claims.Add(new Entities.UserClaim("country", model.Country));
                userToCreate.Claims.Add(new Entities.UserClaim("address", model.Address));
                userToCreate.Claims.Add(new Entities.UserClaim("given_name", model.Firstname));
                userToCreate.Claims.Add(new Entities.UserClaim("family_name", model.Lastname)); userToCreate.Claims.Add(new Entities.UserClaim("country", model.Country));
                userToCreate.Claims.Add(new Entities.UserClaim("email", model.Email));
                userToCreate.Claims.Add(new Entities.UserClaim("subscriptionlevel", "FreeUser"));

                //if we are provisioning a user via external login, we must add the provider 
                // and user id at the provider to this user's login

                if (model.IsProvisioningFromExternal)
                {
                    userToCreate.Logins.Add(new Entities.UserLogin()
                    {
                        LoginProvider = model.Provider,
                        ProviderKey = model.ProviderUserId
                    });
                }

                // add it through the repository
                _voltisUserRepository.AddUser(userToCreate);

                if (!_voltisUserRepository.Save())
                {
                    throw new Exception($"Creating a user failed");
                }

                if (!model.IsProvisioningFromExternal)
                {
                    await HttpContext.SignInAsync(userToCreate.SubjectId, userToCreate.Username);
                }
                
                //continue with the flow
                if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                Redirect("~/");
            }

            //ModelState invalid, retrn the view with the passed-in model
            //so changes can be made
            return View(model);
        }
    }
}