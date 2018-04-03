using System.Linq;
using Main.Core.Entities.Composite;
using Main.Core.Entities.SubEntities;
using WB.Core.BoundedContexts.Designer.Aggregates;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Group;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Question;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.StaticText;
using WB.Core.BoundedContexts.Designer.Commands.Questionnaire.Variable;
using WB.Core.BoundedContexts.Designer.Comments;
using WB.Core.GenericSubdomains.Portable;
using WB.Core.GenericSubdomains.Portable.ServiceLocation;
using WB.Core.Infrastructure.CommandBus;


namespace WB.Core.BoundedContexts.Designer.Implementation.Services.QuestionnairePostProcessors
{
    internal class ResourcesPreProcessor : 
        ICommandPreProcessor<Questionnaire, DeleteQuestion>,
        ICommandPreProcessor<Questionnaire, DeleteGroup>,
        ICommandPreProcessor<Questionnaire, DeleteVariable>,
        ICommandPreProcessor<Questionnaire, DeleteStaticText>
    {
        private ICommentsService commentsService => ServiceLocator.Current.GetInstance<ICommentsService>();

        public void Process(Questionnaire aggregate, DeleteQuestion command)
        {
            this.commentsService.RemoveAllCommentsByEntity(command.QuestionnaireId, command.QuestionId);
        }

        public void Process(Questionnaire aggregate, DeleteVariable command)
        {
            this.commentsService.RemoveAllCommentsByEntity(command.QuestionnaireId, command.EntityId);
        }

        public void Process(Questionnaire aggregate, DeleteStaticText command)
        {
            this.commentsService.RemoveAllCommentsByEntity(command.QuestionnaireId, command.EntityId);
        }

        public void Process(Questionnaire aggregate, DeleteGroup command)
        {
            var @group = aggregate.QuestionnaireDocument.Find<IComposite>(command.GroupId);
            var allChildrens = @group.TreeToEnumerable(g => g.Children).Select(e => e.PublicKey).ToList();
            this.commentsService.RemoveAllCommentsByEntity(command.QuestionnaireId, command.GroupId);

            foreach (var childrenId in allChildrens)
            {
                this.commentsService.RemoveAllCommentsByEntity(command.QuestionnaireId, childrenId);
            }
        }
    }
}
