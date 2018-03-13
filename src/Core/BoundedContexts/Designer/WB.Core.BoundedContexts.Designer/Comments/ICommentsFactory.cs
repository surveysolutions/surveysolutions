using System;
using System.Collections.Generic;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public interface ICommentsFactory
    {
        List<CommentView> LoadCommentsForEntity(string id);
    }

    public class CommentsFactory : ICommentsFactory
    {
        public List<CommentView> LoadCommentsForEntity(string id)
        {
            return new List<CommentView>
            {
                new CommentView
                {
                    UserId = "6d420c16-fcfb-4620-80d7-9d040c12dfbf",
                    UserEmail = "a@a.com",
                    Date = DateTime.Now.AddDays(-1),
                    Comment = "Add two categorical fields to numeric question. The feature is implemented by adding “Define Missings” link to the numeric question layout. Clicking on Define Missings will open two categorical option fields where user can define a numeric value to be interpreted as missing and the message/option",
                    ResolveDate =  DateTime.Now.AddHours(-3)
                },
                new CommentView
                {
                    UserId = "6d420c16-fcfb-4620-80d7-9d040c12dfbf",
                    UserEmail = "a@a.com",
                    Date = DateTime.Now.AddHours(-1),
                    Comment = "Control should not allow entering values of our own System missings. " +
                              "On Interviewer, if any of these two options are defined, show numeric field and a single choice option for selecting an option. Only option is shown on the tablet/web. Values (999888) are not shown.",
                    ResolveDate = null
                }
            };
        }
    }
}
