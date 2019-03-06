INSERT INTO plainstore."AspNetRoles" (id
, email
, emailconfirmed
, passwordhash
, securitystamp
, phonenumber
, phonenumberconfirmed
, twofactorenabled
, lockoutenddateutc
, lockoutenabled
, accessfailedcount
, username
, organisationid
, firstname
, lastname
, inactive)
    SELECT
        u.id,
        u.username Email,
        m.isconfirmed EmailConfirmed,
        m.password PasswordHash,
        --SignInManager.PasswordSignInAsync (used in Login method) 
        --throws an exception http://stackoverflow.com/a/23354148/150342
        NEWID() SecurityStamp, 
        u.telephone PhoneNumber,
        CASE
            WHEN u.telephone IS NULL THEN 0
            ELSE 1
        END PhoneNumberConfirmed,
        0 TwoFactorEnabled,
        NULL LockoutEndDateUtc,
        0 LockoutEnabled,
        m.passwordfailuressincelastsuccess AccessFailedCount,
        u.username,
        u.organisationid,
        u.firstname,
        u.lastname,
        u.inactive
    FROM dbo.userprofiles u
        INNER JOIN dbo.webpages_membership m
            ON m.userid = u.id
    WHERE NOT EXISTS (SELECT
        1
    FROM dbo.aspnetusers
    WHERE id = u.id);

INSERT INTO dbo.aspnetroles (id
, name)
    SELECT
        roleid,
        rolename
    FROM dbo.webpages_roles r
    WHERE NOT EXISTS (SELECT
        1
    FROM dbo.aspnetroles
    WHERE roleid = r.roleid);

 INSERT INTO dbo.aspnetuserroles (userid
    , roleid)
        SELECT
            userid,
            roleid
        FROM dbo.webpages_usersinroles ur
        WHERE NOT EXISTS (SELECT
            1
        FROM dbo.aspnetuserroles
        WHERE userid = ur.userid
        AND roleid = ur.roleid);
