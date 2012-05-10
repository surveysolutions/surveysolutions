#region

using System;
using RavenQuestionnaire.Core.Commands.File;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

#endregion

namespace RavenQuestionnaire.Core.CommandHandlers.File
{
    public class DeleteFileCommandHandler : ICommandHandler<DeleteFileCommand>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorageService _fileStorageService;

        public DeleteFileCommandHandler(IFileStorageService fileStorageService, IFileRepository fileRepository)
        {
            _fileStorageService = fileStorageService;
            _fileRepository = fileRepository;
        }

        #region ICommandHandler<UploadImageFileCommand> Members

        public void Handle(DeleteFileCommand command)
        {
            var entity = _fileRepository.Load(command.Id);

            var filename = entity.GetInnerDocument().Filename;
            var thumbname = entity.GetInnerDocument().Thumbnail;

            _fileStorageService.DeleteFile(filename);
            _fileStorageService.DeleteFile(thumbname);

            _fileRepository.Remove(entity);
        }

        #endregion
    }
}