using BitMiracle.LibTiff.Classic;
using NUnit.Framework;

namespace WB.Tests.Unit
{
    /// <summary>
    /// Assembly-wide NUnit setup. Several tests intentionally feed malformed/edge-case TIFF
    /// content into LibTiff.NET (e.g. <c>GeoTiffInfoReaderTests</c>, <c>MapFileStorageServiceTests</c>)
    /// to verify that the reader gracefully rejects or tolerates them. By default LibTiff.NET writes
    /// every warning/error ("unknown field with tag ...", "tags are not sorted ...", "bad magic number ...")
    /// straight to stdout/stderr, which floods the CI log for what are expected, already-asserted-on
    /// conditions and can trip up log processors on hosted runners.
    ///
    /// Installing a silent <see cref="TiffErrorHandler"/> once for the whole test run keeps that noise
    /// out of the CI logs without changing any test behavior (the return values/exceptions used by the
    /// tests are unaffected by this handler).
    /// </summary>
    [SetUpFixture]
    public class GlobalTestSetup
    {
        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            Tiff.SetErrorHandler(new SilentTiffErrorHandler());
        }

        private sealed class SilentTiffErrorHandler : TiffErrorHandler
        {
            public override void WarningHandler(Tiff tif, string method, string format, params object[] args)
            {
                // Intentionally swallow LibTiff warnings (e.g. unknown/unsorted tags) to keep CI logs clean.
            }

            public override void WarningHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
            {
            }

            public override void ErrorHandler(Tiff tif, string method, string format, params object[] args)
            {
                // Intentionally swallow LibTiff errors (e.g. "not a TIFF" / "bad magic number") - the
                // corresponding tests assert on the resulting bool/exception, not on console output.
            }

            public override void ErrorHandlerExt(Tiff tif, object clientData, string method, string format, params object[] args)
            {
            }
        }
    }
}

