using System;
using System.Collections.Generic;
using System.Linq;

namespace WB.Core.BoundedContexts.Designer.Comments
{
    public interface ICommentsFactory
    {
        List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId);
    }

    public class CommentsFactory : ICommentsFactory
    {
        public List<CommentView> LoadCommentsForEntity(Guid questionnaireId, Guid entityId)
        {
            var loadCommentsForEntity = new List<CommentView>
            {
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "test",
                    UserEmail = "a@a.com",
                    Date = DateTime.Now.AddDays(-1),
                    Comment = "Add two categorical fields to numeric question. The feature is implemented by adding “Define Missings” link to the numeric question layout. Clicking on Define Missings will open two categorical option fields where user can define a numeric value to be interpreted as missing and the message/option",
                    ResolveDate =  DateTime.Now.AddHours(-3)
                },
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "slava",
                    UserEmail = "slava.scherbak@gmail.com",
                    Date = DateTime.Now.AddHours(-1),
                    Comment = "Control should not allow entering values of our own System missings. " +
                              "On Interviewer, if any of these two options are defined, show numeric field and a single choice option for selecting an option. Only option is shown on the tablet/web. Values (999888) are not shown.",
                    ResolveDate = null
                },
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "test",
                    UserEmail = "a@a.com",
                    Date = DateTime.Now.AddDays(-1),
                    Comment = "Add two categorical fields to numeric question. The feature is implemented by adding “Define Missings” link to the numeric question layout. Clicking on Define Missings will open two categorical option fields where user can define a numeric value to be interpreted as missing and the message/option",
                    ResolveDate =  DateTime.Now.AddHours(-3)
                },
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "slava",
                    UserEmail = "slava.scherbak@gmail.com",
                    Date = DateTime.Now.AddHours(-1),
                    Comment = "Control should not allow entering values of our own System missings. " +
                              "On Interviewer, if any of these two options are defined, show numeric field and a single choice option for selecting an option. Only option is shown on the tablet/web. Values (999888) are not shown.",
                    ResolveDate = null
                },
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "test",
                    UserEmail = "a@a.com",
                    Date = DateTime.Now.AddDays(-1),
                    Comment = "Add two categorical fields to numeric question. The feature is implemented by adding “Define Missings” link to the numeric question layout. Clicking on Define Missings will open two categorical option fields where user can define a numeric value to be interpreted as missing and the message/option",
                    ResolveDate =  DateTime.Now.AddHours(-3)
                },
                new CommentView
                {
                    Id = Guid.NewGuid(),
                    UserName = "slava",
                    UserEmail = "slava.scherbak@gmail.com",
                    Date = DateTime.Now.AddHours(-1),
                    Comment = "Control should not allow entering values of our own System missings. " +
                              "On Interviewer, if any of these two options are defined, show numeric field and a single choice option for selecting an option. Only option is shown on the tablet/web. Values (999888) are not shown.",
                    ResolveDate = null
                }
            };

            return loadCommentsForEntity.Take(new Random(DateTime.Now.Millisecond).Next(0, loadCommentsForEntity.Count)).ToList();
        }
    }
}
