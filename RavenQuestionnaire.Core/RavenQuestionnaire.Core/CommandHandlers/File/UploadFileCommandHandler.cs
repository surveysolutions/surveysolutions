#region

using System;
using RavenQuestionnaire.Core.Commands.File;
using RavenQuestionnaire.Core.Repositories;
using RavenQuestionnaire.Core.Services;

#endregion

namespace RavenQuestionnaire.Core.CommandHandlers.File
{
    public class UploadFileCommandHandler : ICommandHandler<UploadFileCommand>
    {
        private readonly IFileRepository _fileRepository;
        private readonly IFileStorageService _fileStorageService;

        public UploadFileCommandHandler(IFileStorageService fileStorageService, IFileRepository fileRepository)
        {
            _fileStorageService = fileStorageService;
            _fileRepository = fileRepository;
        }

        #region ICommandHandler<UploadImageFileCommand> Members

        public void Handle(UploadFileCommand command)
        {
            var id = Guid.NewGuid();
            var filename = String.Format("images/{0}.png", id);
            var thumbname = String.Format("images/{0}_thumb.png", id);

            _fileStorageService.StoreFile(filename, command.OriginalImage);
            _fileStorageService.StoreFile(thumbname, command.ThumbnailImage);

            var newImage = new Entities.File(command.Title, command.Description, filename, command.OriginalWidth,
                                    command.OriginalHeight, thumbname, command.ThumbHeight, command.ThumbWidth, command.Executor.Id);

            _fileRepository.Add(newImage);
        }

        #endregion
    }
}