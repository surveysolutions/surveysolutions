using System;

namespace WB.UI.WebTester.Services
{
    public interface IMediaStorage
    {
        void Store(Guid interviewId, MultimediaFile file);
        MultimediaFile Get(Guid interviewId, string filename);
    }
}