using System;
using System.IO;
using Android.Runtime;
using Java.IO;
using Java.Util.Zip;
using WB.Core.SharedKernels.Enumerator.Services;
using JavaFile = Java.IO.File;

namespace WB.UI.Shared.Enumerator.Services
{
    public class ArchivePatcherService : IArchivePatcherService
    {
        public void ApplyPath(string oldFile, string patchFile, string newFileOut)
        {
            // to uncompress the patch
            using (Inflater uncompressor = new Inflater(true))
            {
                using (FileStream compressedPatchIn = new FileStream(patchFile, FileMode.Open))
                {
                    using (InflaterInputStream patchIn = new InflaterInputStream(compressedPatchIn, uncompressor, 32768))
                    {
                        //Get a pointer to the Java class.
                        IntPtr nativeJavaClass = JNIEnv.FindClass("com/google/archivepatcher/applier/FileByFileV1DeltaApplier");

                        // Find the parameterless constructor.
                        IntPtr defaultConstructor = JNIEnv.GetMethodID(nativeJavaClass, "<init>", "()V");
                        // Create a new instance of the class.
                        IntPtr instance = JNIEnv.NewObject(nativeJavaClass, defaultConstructor);

                        // Find java method.
                        IntPtr method = JNIEnv.GetMethodID(nativeJavaClass, "applyDelta", "(Ljava/io/File;Ljava/io/InputStream;Ljava/io/OutputStream;)V");

                        // Call the method with java parameters
                        JNIEnv.CallVoidMethod(instance, method, new JValue(new JavaFile(oldFile)),
                            new JValue(patchIn), new JValue(new FileOutputStream(newFileOut)));
                    }
                }
            }
        }
    }
}