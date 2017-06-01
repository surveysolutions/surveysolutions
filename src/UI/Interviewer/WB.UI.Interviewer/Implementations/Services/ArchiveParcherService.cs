using System;
using Android.Runtime;
using Java.IO;
using WB.Core.BoundedContexts.Interviewer.Services;
using JavaFile = Java.IO.File;

namespace WB.UI.Interviewer.Implementations.Services
{
    public class ArchivePatcherService : IArchivePatcherService
    {
        public void ApplyPath(string oldFile, string pathIn, string newFileOut)
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
                new JValue(new FileInputStream(pathIn)), new JValue(new FileOutputStream(newFileOut)));
        }
    }
}