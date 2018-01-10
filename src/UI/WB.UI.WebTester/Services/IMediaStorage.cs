using System;

namespace WB.UI.WebTester.Services
{
    public interface IMediaStorage
    {
        void Store(MultimediaFile file, Guid interviewId);
        MultimediaFile Get(Guid interviewId, string filename);
    }
}