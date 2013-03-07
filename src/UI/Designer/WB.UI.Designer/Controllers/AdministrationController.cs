using WB.UI.Designer.Extensions;
using WB.UI.Designer.Models;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace WB.UI.Designer.Controllers
{
    public class AccountListViewItemModel
    {
        [Key]
        public string Id { get; set; }
        [Display(Name = "Name")]
        public string UserName { get; set; }
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Display(Name = "Created date")]
        public string CreationDate { get; set; }
        [Display(Name = "Last login")]
        public string LastLoginDate { get; set; }
        [Display(Name = "Approved?")]
        public bool IsApproved { get; set; }
        [Display(Name = "Locked?")]
        public bool IsLockedOut { get; set; }
    }

    [Authorize(Roles = "Administrator")]
    public class AdministrationController : BootstrapBaseController
    {   
        //
        // GET: /Administration/

        public ViewResult Index()
        {
            int totalRecords;
            var users = Membership.GetAllUsers(0, 20, out totalRecords).OfType<MembershipUser>().Select(x=> new AccountListViewItemModel()
                {
                    Id = x.UserName,
                    UserName = x.UserName,
                    Email = x.Email,
                    CreationDate = x.CreationDate.ToUIString(),
                    LastLoginDate = x.LastLoginDate.ToUIString(),
                    IsApproved = x.IsApproved,
                    IsLockedOut = x.IsLockedOut
                }).ToList();
            return View(users);
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
                    Error(e.StatusCode.ToErrorCode());
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

