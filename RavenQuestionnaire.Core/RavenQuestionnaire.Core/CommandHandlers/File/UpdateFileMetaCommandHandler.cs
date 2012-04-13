using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RavenQuestionnaire.Core.Commands.File;
using RavenQuestionnaire.Core.Repositories;

namespace RavenQuestionnaire.Core.CommandHandlers.File
{
    public class UpdateFileMetaCommandHandler: ICommandHandler<UpdateFileMetaCommand>
    {
        private IFileRepository _fileRepository;
        public UpdateFileMetaCommandHandler(IFileRepository fileRepository)
        {
            _fileRepository = fileRepository;
        }
        public void Handle(UpdateFileMetaCommand command)
        {
            var file = _fileRepository.Load(command.Id);

            file.UpdateMeta(command.Title, command.Description);

        }
    }
}
