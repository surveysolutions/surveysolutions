using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Main.Core.Documents;
using Main.Core.Domain;
using Main.Core.View;
using Ncqrs.Commanding.CommandExecution;
using Ncqrs.Domain;
using WB.Core.Questionnaire.ImportService.Commands;

namespace WB.Core.Questionnaire.ImportService
{
    public class DefaultImportService : CommandExecutorBase<ImportQuestionnaireCommand>
    {
        protected override void ExecuteInContext(IUnitOfWorkContext context, ImportQuestionnaireCommand command)
        {
            var document = command.Source as QuestionnaireDocument;
            if (document == null)
                throw new ArgumentException("only QuestionnaireDocuments are supported for now");

           /* var creator = viewRepository.Load<AccountViewInputModel, AccountView>(new AccountViewInputModel(command.CreatedBy));
            if (creator == null)
                throw new ArgumentException("Creator is absent");*/

          /*  var questionnsire = viewRepository.Load<QuestionnaireViewInputModel, QuestionnaireView>(
                  new QuestionnaireViewInputModel(command.CommandIdentifier));
            if(questionnsire!=null)
                throw new ArgumentException("Questionnair with the same key present in system");*/
            /*var ar =*/ 
            new QuestionnaireAR(command.CommandIdentifier, document.Title, command.CreatedBy, document);
            context.Accept();
        }
    }
}
