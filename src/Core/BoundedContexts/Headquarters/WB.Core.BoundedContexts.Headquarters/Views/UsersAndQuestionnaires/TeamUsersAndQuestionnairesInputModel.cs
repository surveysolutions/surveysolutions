﻿using System;

namespace WB.Core.BoundedContexts.Headquarters.Views.UsersAndQuestionnaires
{
    public class TeamUsersAndQuestionnairesInputModel
    {
        public TeamUsersAndQuestionnairesInputModel(Guid viewerId)
        {
            this.ViewerId = viewerId;
        }

        public Guid ViewerId { get; set; }
    }
}
