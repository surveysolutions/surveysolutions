using System.ComponentModel;
using FluentMigrator;

namespace WB.Persistence.Headquarters.Migrations.Users;

// Legacy users migrated from the old document store (see M002_MigrateUsersFromUserDocumentStore)
// were inserted with "LockoutEnabled" hardcoded to FALSE. ASP.NET Core Identity's own
// UserManager.IsLockedOutAsync/CheckPasswordSignInAsync short-circuit to "not locked out"
// whenever LockoutEnabled is false, regardless of LockoutEnd/LockoutEndDateUtc. As a result:
//  - The automatic (failed-login) lockout mechanism never actually blocks sign-in for these
//    legacy accounts, even if LockoutEndDateUtc gets set in the future by AccessFailedAsync.
//  - The Manage/{id} page used to show IsLockedOut = false for such accounts even though the
//    account may have looked "locked" from the data perspective.
// New users created through UserManager.CreateAsync already get LockoutEnabled = true
// automatically (Options.Lockout.AllowedForNewUsers defaults to true). This migration backfills
// the same value for existing (legacy) accounts so automatic lockout behaves consistently for
// every account, matching what happens for newly created users.
[Localizable(false)]
[Migration(202609171200)]
public class M202609171200_FixLockoutEnabledForLegacyUsers : ForwardOnlyMigration
{
    public override void Up()
    {
        this.Execute.Sql(@"
            UPDATE users.users
            SET ""LockoutEnabled"" = TRUE
            WHERE ""LockoutEnabled"" = FALSE;
        ");
    }
}

