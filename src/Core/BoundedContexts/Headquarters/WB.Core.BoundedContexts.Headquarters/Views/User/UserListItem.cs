﻿using System;
using System.Collections.Generic;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Headquarters.Views.Interviewer;

namespace WB.Core.BoundedContexts.Headquarters.Views.User
{
    public class UserListItem : InterviewersItem
    {
        public UserListItem(Guid id, string name, string email, DateTime creationDate, bool isLockedBySupervisor, bool isLockedByHQ, List<UserRoles> roles, string deviceId)
            : base(id, name, null, email, creationDate, isLockedBySupervisor, isLockedByHQ, deviceId)
        {
            this.Roles = roles;
        }

        public List<UserRoles> Roles { get; set; }
    }
}