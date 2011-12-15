using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RavenQuestionnaire.Core.Documents;
using RavenQuestionnaire.Core.Entities;
using RavenQuestionnaire.Core.Entities.SubEntities;

namespace RavenQuestionnaire.Core.Tests.Entities
{
    [TestFixture]
    public class UserEntityTest
    {
         [Test]
         public void ChangeEmailToValid_ChangesEmailInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Email = "test@bank.com";
             User user = new User(innerDocument);
             user.ChangeEmail("mis@bank.com");

             Assert.AreEqual(innerDocument.Email, "mis@bank.com");
         }
         [Test]
         public void ChangeEmailToInvalidValid_ExceptionThrowed()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Email = "test@bank.com";
             User user = new User(innerDocument);
             Assert.Throws<ArgumentException>(() => user.ChangeEmail("misbank.com"));
         }
         [Test]
         public void ChangePasswordToValid_ChangedPasswordInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Password = "pass1";
             User user = new User(innerDocument);
             user.ChangePassword("pass2");

             Assert.AreEqual(innerDocument.Password, "pass2");
         }
         [Test]
         public void ChangeLockStatus_ChangeLockStatusInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.IsLocked = false;
             User user = new User(innerDocument);
             user.ChangeLockStatus(true);

             Assert.AreEqual(innerDocument.IsLocked, true);
         }
         [Test]
         public void AddRole_AddRoleInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.User);
             User user = new User(innerDocument);
             user.AddRole(UserRoles.Administrator);

             Assert.AreEqual(
                 innerDocument.Roles.Count == 2 && innerDocument.Roles.Contains(UserRoles.User) &&
                 innerDocument.Roles.Contains(UserRoles.Administrator), true);
         }
         [Test]
         public void RemoveExistingRole_RemoveRoleInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.User);
             User user = new User(innerDocument);
             var result = user.RemoveRole(UserRoles.User);

             Assert.AreEqual(
                 innerDocument.Roles.Count == 0 && result, true);
         }
         [Test]
         public void RemoveUnExistingRole_RemoveRoleInDocument()
         {
             UserDocument innerDocument = new UserDocument();
           
             User user = new User(innerDocument);
             var result = user.RemoveRole(UserRoles.User);

             Assert.AreEqual(
                 innerDocument.Roles.Count == 0 && !result, true);
         }
         [Test]
         public void IsUserInExistingRole_ReturnsTrue()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.User);
             User user = new User(innerDocument);
             var result  =user.IsUserInRole(UserRoles.User);

             Assert.AreEqual(result, true);
         }
         [Test]
         public void IsUserInUnExistingRole_ReturnsFalse()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             var result = user.IsUserInRole(UserRoles.User);

             Assert.AreEqual(result, false);
         }
         [Test]
         public void ChangeRoleList_RoleListIsChangedInDocument()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.User);
             User user = new User(innerDocument);
             user.ChangeRoleList(new[] {UserRoles.Manager, UserRoles.Administrator});

             Assert.AreEqual(
                 innerDocument.Roles.Count == 2 && innerDocument.Roles.Contains(UserRoles.Manager) &&
                 innerDocument.Roles.Contains(UserRoles.Administrator), true);
         }
         [Test]
         public void CreateSupervisorFromUserInRoleSupervisor_SupervisorInstanceIsCreatedFromUser()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.Supervisor);
             User user = new User(innerDocument);
             Supervisor supervisor = user.CreateSupervisor();
             Assert.AreEqual(
                 supervisor.SupervisorId == user.UserId && supervisor.SupervisorName == innerDocument.UserName, true);
         }
         [Test]
         public void CreateSupervisorFromUserNotInRoleSupervisor_SupervisorReturnedAsNull()
         {
             UserDocument innerDocument = new UserDocument();
             innerDocument.Roles.Add(UserRoles.User);
             User user = new User(innerDocument);
             Supervisor supervisor = user.CreateSupervisor();
             Assert.AreEqual(supervisor, null);
         }

         [Test]
         public void SetSupervisor_SupervisorIsSeted()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             Supervisor supervisor = new Supervisor() {SupervisorId = "som_id"};
             user.SetSupervisor(supervisor);

             Assert.AreEqual(innerDocument.Supervisor, supervisor);
         }
         [Test]
         public void SetSupervisorWithNullInstance_SupervisorIsREseted()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             user.SetSupervisor(null);

             Assert.AreEqual(innerDocument.Supervisor, null);
         }
         [Test]
         public void ClearSupervisor_SupervisorIsCleared()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             Supervisor supervisor = new Supervisor() { SupervisorId = "som_id" };
             innerDocument.Supervisor = supervisor;
             user.ClearSupervisor();

             Assert.AreEqual(innerDocument.Supervisor, null);
         }
         [Test]
         public void DeleteUser_UserIsMArkedAsDeleted()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             user.DeleteUser();

             Assert.AreEqual(innerDocument.IsDeleted, true);
         }
         [Test]
         public void SetLocaton_SetLocatonIsChanged()
         {
             UserDocument innerDocument = new UserDocument();
             User user = new User(innerDocument);
             user.SetLocaton(new Location("some location"));

             Assert.AreEqual(innerDocument.Location.Location, "some location");
         }
    }
}
