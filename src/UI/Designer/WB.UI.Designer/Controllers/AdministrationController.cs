using Designer.Web.Extensions;
using Designer.Web.Models;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace Designer.Web.Controllers
{   
    [Authorize(Roles = "Administrator")]
    public class AdministrationController : Controller
    {   
        //
        // GET: /Administration/

        public ViewResult Index()
        {
            int totalRecords;
            return View(Membership.GetAllUsers(0, 20, out totalRecords));
        }

        //
        // GET: /Administration/Details/john

        public ViewResult Details(string id)
        {
            return View(GetUser(id));
        }

        //
        // GET: /Administration/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Administration/Create

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    string confirmationToken =
                        WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { Email = model.Email }, false);

                    Roles.Provider.AddUsersToRoles(new[] { model.UserName }, new[] { UserHelper.USERROLENAME });

                    return RedirectToAction("Index");
                }
                catch (MembershipCreateUserException e)
                {
                     ModelState.AddModelError("", e.StatusCode.ToErrorCode());
                }
            }
            return View();
        }
        
        //
        // GET: /Administration/Edit/john

        public ActionResult Edit(string id)
        {
            var intUser = GetUser(id);
            return View(new UpdateAccountModel()
                {
                    Comment = intUser.Comment,
                    Email = intUser.Email,
                    IsApproved = intUser.IsApproved,
                    IsLockedOut = intUser.IsLockedOut,
                    UserName = intUser.UserName
                });
        }

        //
        // POST: /Administration/Edit/john

        [HttpPost]
        public ActionResult Edit(UpdateAccountModel user)
        {
            if (ModelState.IsValid)
            {
                var intUser = Membership.GetUser(user.UserName);
                if (intUser != null)
                {
                    Membership.UpdateUser(new MembershipUser(providerName: intUser.ProviderName,
                                                             name: user.UserName,
                                                             providerUserKey: intUser.ProviderUserKey,
                                                             email: user.Email,
                                                             passwordQuestion: intUser.PasswordQuestion,
                                                             comment: user.Comment,
                                                             isApproved: user.IsApproved,
                                                             isLockedOut: user.IsLockedOut,
                                                             creationDate: intUser.CreationDate,
                                                             lastLoginDate: intUser.LastLoginDate,
                                                             lastActivityDate: intUser.LastActivityDate,
                                                             lastPasswordChangedDate: intUser.LastPasswordChangedDate,
                                                             lastLockoutDate: intUser.LastLockoutDate));
                }
                return RedirectToAction("Index");
            } else {
                return View();
            }
        }

        //
        // GET: /Administration/Delete/john

        public ActionResult Delete(string id)
        {
            return View(GetUser(id));
        }

        //
        // POST: /Administration/Delete/john

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(string id)
        {
            Membership.DeleteUser(id);

            return RedirectToAction("Index");
        }

        private MembershipUser GetUser(string id)
        {
            return Membership.GetUser(id);
        }
    }
}

