// <copyright file="FilePickerFragment.cs" company="Compass Informatics Ltd.">
// Copyright (c) Compass Informatics 2014, All Right Reserved, http://compass.ie/
//
// This source is subject to the MIT License.
// Please see the License file for more information.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY 
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// </copyright>
// <author>Michele Scandura</author>
// <email>mscandura@compass.ie</email>
// <date>30-04-2014</date>
// <summary>Contains a simple Point with floating coordinates.</summary>

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Enum = System.Enum;
using Environment = Android.OS.Environment;
using Exception = System.Exception;
using File = Java.IO.File;
using Object = Java.Lang.Object;

namespace WB.UI.Interviewer.Activities
{
    /// <summary>
    /// A DialogFragment that will show the files and subdirectories of a given directory.
    /// </summary>
    public class FilePickerFragment : DialogFragment
    {
        private const string ArgInitialDir = "ArgInitialDirectory";
        private const string ArgNewDirectoryName = "ArgNewDirectoryName";
        private const string ArgMode = "ArgMode";
        private const string LogTag = "Compass.FilePicker.FileListFragment";
        private const string KeyCurrentDirectory = "KeyCurrentDirectory";

        private Button btnCancel;
        private Button btnConfirm;
        private DirectoryInfo currentDirectory;
        private File selectedFile;
        private FileListAdapter adapter;
        private FilePickerMode filePickerMode;
        private ImageButton btnCreateFolder;
        private ImageButton btnLevelUp;
        private ListView listFiles;
        private FilePickerFileObserver fileObserver;
        private string initialDirectory;
        private string newDirectoryName;

        /// <summary>
        /// Occurs when the user press the Confirm button.
        /// </summary>
        public event FileSelectedEventHandler FileSelected;

        /// <summary>
        /// Occurs when the user press the Cancel button.
        /// </summary>
        public event CancelEventHandler Cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePickerFragment"/> class.
        /// </summary>
        public FilePickerFragment()
            : this(null, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePickerFragment"/> class.
        /// </summary>
        /// <param name="initialDir">The initial dirrectory.</param>
        /// <param name="mode">The filepicker mode.</param>
        public FilePickerFragment(string initialDir, FilePickerMode mode = FilePickerMode.File)
            : this(initialDir, null, mode)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilePickerFragment"/> class.
        /// </summary>
        /// <param name="initialDir">The initial dirrectory.</param>
        /// <param name="newDirName">Default name for new folders.</param>
        /// <param name="mode">The filepicker mode.</param>
        public FilePickerFragment(string initialDir, string newDirName, FilePickerMode mode = FilePickerMode.File)
        {
            var args = new Bundle();
            args.PutString(ArgNewDirectoryName, newDirName ?? string.Empty);
            args.PutString(ArgInitialDir, initialDir ?? string.Empty);
            args.PutInt(ArgMode, (int)mode);
            this.Arguments = args;
        }

        /// <inheritdoc />
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (this.Arguments == null)
            {
                throw new IllegalArgumentException("You must create DirectoryChooserFragment via the FileListFragment(string, string, FilePickerMode) constructor.");
            }

            this.newDirectoryName = this.Arguments.GetString(ArgNewDirectoryName);
            this.initialDirectory = this.Arguments.GetString(ArgInitialDir);
            this.filePickerMode = (FilePickerMode)this.Arguments.GetInt(ArgMode);

            if (savedInstanceState != null)
            {
                this.initialDirectory = savedInstanceState.GetString(KeyCurrentDirectory);
            }

            if (this.ShowsDialog)
            {
                this.SetStyle(DialogFragmentStyle.NoTitle, 0);
            }
            else
            {
                this.SetHasOptionsMenu(true);
            }
        }

        /// <inheritdoc />
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.Assert(this.Activity != null, "Activity != null");
            var view = inflater.Inflate(Resource.Layout.filepicker_fragment, container, false);

            this.btnConfirm = view.FindViewById<Button>(Resource.Id.btnConfirm);
            this.btnCancel = view.FindViewById<Button>(Resource.Id.btnCancel);
            this.btnCreateFolder = view.FindViewById<ImageButton>(Resource.Id.btnCreateFolder2);
            this.btnLevelUp = view.FindViewById<ImageButton>(Resource.Id.btnNavUp);
            this.listFiles = view.FindViewById<ListView>(Resource.Id.directoryList);

            switch (this.filePickerMode)
            {
                case FilePickerMode.File:
                    view.FindViewById<TextView>(Resource.Id.txtvSelectedFolderLabel).Text = this.GetString(Resource.String.filepicker_selected_file_label);
                    break;
                case FilePickerMode.Directory:
                    view.FindViewById<TextView>(Resource.Id.txtvSelectedFolderLabel).Text = this.GetString(Resource.String.filepicker_selected_folder_label);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            this.btnConfirm.Click += (sender, args) => this.Confirm();
            this.btnCancel.Click += (sender, args) => this.CancelAction();
            this.btnCreateFolder.Click += (sender, args) => this.OpenNewFolderDialog();
            this.btnLevelUp.Click += (sender, args) => this.UpOneLevel();
            this.listFiles.ItemClick += this.ListFilesOnItemClick;

            if (!this.ShowsDialog || this.filePickerMode == FilePickerMode.File)
            {
                this.btnCreateFolder.Visibility = ViewStates.Gone;
            }

            this.adapter = new FileListAdapter(this.Activity, new FileSystemInfo[0]);
            this.listFiles.Adapter = this.adapter;

            if (!string.IsNullOrWhiteSpace(this.initialDirectory) && this.IsValidFile(this.initialDirectory))
            {
                this.selectedFile = new File(this.initialDirectory);
            }
            else
            {
                this.selectedFile = Environment.ExternalStorageDirectory;
            }

            return view;
        }

        /// <inheritdoc />
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.filepicker_menu, menu);
            var menuItem = menu.FindItem(Resource.Id.filepicker_new_folder_item);
            if (menuItem == null)
            {
                return;
            }
            menuItem.SetVisible(this.IsValidFile(this.selectedFile) && this.newDirectoryName != null);
        }

        /// <inheritdoc />
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId != Resource.Id.filepicker_new_folder_item)
            {
                return base.OnOptionsItemSelected(item);
            }
            this.OpenNewFolderDialog();
            return true;
        }

        /// <inheritdoc />
        public override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            outState.PutString(KeyCurrentDirectory, this.selectedFile.AbsolutePath);
        }

        /// <inheritdoc />
        public override void OnPause()
        {
            base.OnPause();
            if (this.fileObserver != null)
            {
                this.fileObserver.StopWatching();
            }
        }

        /// <inheritdoc />
        public override void OnResume()
        {
            base.OnResume();
            if (this.fileObserver != null)
            {
                this.fileObserver.StartWatching();
            }
            this.RefreshFilesList();
        }

        private void ListFilesOnItemClick(object sender, AdapterView.ItemClickEventArgs itemClickEventArgs)
        {
            var fileSystemInfo = this.adapter.GetItem(itemClickEventArgs.Position);

            if (fileSystemInfo.IsFile())
            {
                this.selectedFile = new File(fileSystemInfo.FullName);
                this.UpdateSelectedFileText();
                this.RefreshButtonState();
            }
            else
            {
                // Dig into this directory, and display it's contents
                RefreshFilesList(fileSystemInfo.FullName);
            }
        }

        private void RefreshFilesList()
        {
            if (this.selectedFile != null && this.selectedFile.IsDirectory)
            {
                this.RefreshFilesList(this.selectedFile);
            }
        }

        private void RefreshFilesList(string path)
        {
            var file = new File(path);
            this.RefreshFilesList(file);
            file.Dispose();
        }

        private void RefreshFilesList(File targetDirectory)
        {
            if (targetDirectory == null)
            {
                LogDebug("Directory can't be null");
            }
            else if (!targetDirectory.IsDirectory)
            {
                LogDebug("Cant change to a file");
            }
            else
            {
                var visibleThings = new List<FileSystemInfo>();
                var dir = new DirectoryInfo(targetDirectory.Path);
                try
                {
                    switch (this.filePickerMode)
                    {
                        case FilePickerMode.File:
                            visibleThings.AddRange(dir.GetFileSystemInfos().Where(item => item.IsVisible()));
                            break;
                        case FilePickerMode.Directory:
                            foreach (var item in
                                dir.GetFileSystemInfos()
                                    .Where(
                                        item =>
                                        item.IsVisible() && item.IsDirectory()
                                        && (item.Attributes & FileAttributes.System) != FileAttributes.System
                                        && (item.Attributes & FileAttributes.Hidden) != FileAttributes.Hidden))
                            {
                                try
                                {
                                    var directoryInfo = new DirectoryInfo(item.FullName);
                                    // Trying to access a subfolder. If it's ok I'll check if it's writable.
                                    // If everything is fine I'll add the folder to the list of visible things.
                                    directoryInfo.GetFileSystemInfos(); //Throws an exception if it can't access the folder
                                    var javaFile = new File(item.FullName);

                                    // native java method to check if a file or folder is writable
                                    if (javaFile.CanWrite())
                                    {
                                        visibleThings.Add(item);
                                    }
                                    javaFile.Dispose(); // remember to dispose to avoid keeping references to java objects.
                                }
                                catch (Exception ex)
                                {
                                    LogDebug("Directory " + item.FullName + "is not writable.");
                                    LogError(ex.Message);
                                }
                            }
                            break;
                        default:
                            // something went wrong
                            throw new ArgumentOutOfRangeException();
                    }
                }
                catch (Exception ex)
                {
                    LogError("Couldn't access the directory " + this.currentDirectory.FullName + "; " + ex);
                    Toast.MakeText(this.Activity, "Problem retrieving contents of " + targetDirectory, ToastLength.Long).Show();
                    return;
                }

                this.currentDirectory = dir;
                this.selectedFile = new File(this.currentDirectory.FullName);
                this.adapter.AddDirectoryContents(visibleThings);
                this.CreateFileObserver(this.currentDirectory.FullName);
                this.fileObserver.StartWatching();
                this.UpdateSelectedFileText();

                LogDebug(string.Format("Displaying the contents of directory {0}.", targetDirectory));
                this.RefreshButtonState();
            }
        }

        private void UpdateSelectedFileText()
        {
            if (this.View == null)
            {
                return;
            }

            var textView = this.View.FindViewById<TextView>(Resource.Id.txtvSelectedFolder);
            if (textView == null)
            {
                return;
            }
            switch (this.filePickerMode)
            {
                case FilePickerMode.File:
                    if (this.selectedFile.IsFile)
                    {
                        textView.Text = this.selectedFile.Name;
                    }
                    break;
                case FilePickerMode.Directory:
                    if (this.selectedFile.IsDirectory)
                    {
                        textView.Text = this.selectedFile.AbsolutePath;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void UpOneLevel()
        {
            var path = this.currentDirectory.Parent;
            if (path != null && !string.IsNullOrWhiteSpace(path.FullName))
            {
                this.RefreshFilesList(path.FullName);
            }
        }

        private void OpenNewFolderDialog()
        {
            var builder = new AlertDialog.Builder(this.Activity);
            var customView = this.Activity.LayoutInflater.Inflate(Resource.Layout.filepicker_createfolder, null);
            var editText = customView.FindViewById<EditText>(Resource.Id.folderName);
            builder.SetView(customView);
            builder.SetMessage(this.GetString(Resource.String.filepicker_enter_folder_msg));
            builder.SetPositiveButton(
                this.GetString(Resource.String.filepicker_ok),
                (s, e) =>
                {
                    this.CreateFolder(this.currentDirectory.FullName, editText.Text);
                    ((AlertDialog)s).Dismiss();
                });
            builder.SetNegativeButton(this.GetString(Resource.String.filepicker_cancel_label), (s, e) => ((AlertDialog)s).Dismiss());
            builder.Create().Show();
        }

        private void CreateFolder(string baseFolder, string newFolder)
        {
            try
            {
                var path = Path.Combine(baseFolder, newFolder);
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                    this.RefreshFilesList(baseFolder);
                }
                else if (Directory.Exists(path))
                {
                    Toast.MakeText(this.Activity, this.GetString(Resource.String.filepicker_create_folder_error_already_exists), ToastLength.Short).Show();
                }
            }
            catch (Exception ex)
            {
                Toast.MakeText(this.Activity, this.GetString(Resource.String.filepicker_create_folder_error), ToastLength.Short).Show();
                LogError(ex.Message);
            }
        }

        private void Confirm()
        {
            if (this.FileSelected != null)
            {
                this.FileSelected(this, this.selectedFile.AbsolutePath);
            }
        }

        private void CancelAction()
        {
            if (this.Cancel != null)
            {
                this.Cancel(this);
            }
        }

        private bool IsValidFile(string path)
        {
            var file = new File(path);
            var isValid = this.IsValidFile(file);
            file.Dispose();
            return isValid;
        }

        private bool IsValidFile(File file)
        {
            bool isValid;
            switch (this.filePickerMode)
            {
                case FilePickerMode.File:
                    isValid = file != null && file.IsFile && file.CanRead() && file.CanWrite();
                    break;
                case FilePickerMode.Directory:
                    isValid = file != null && file.IsDirectory && file.CanRead() && file.CanWrite();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return isValid;
        }

        private bool IsValidFile()
        {
            return this.IsValidFile(this.selectedFile);
        }

        private void RefreshButtonState()
        {
            if (this.Activity == null)
            {
                return;
            }
            this.btnConfirm.Enabled = this.IsValidFile();
            this.Activity.InvalidateOptionsMenu();
        }

        private static void LogDebug(string message)
        {
            Log.Debug(LogTag, message);
        }

        private static void LogError(string message)
        {
            Log.Error(LogTag, message);
        }

        private void CreateFileObserver(string path)
        {
            // FileObserverEvents.Create | FileObserverEvents.Delete | FileObserverEvents.MovedFrom | FileObserverEvents.MovedTo;
            const FileObserverEvents Mask = FileObserverEvents.Create | FileObserverEvents.Delete | FileObserverEvents.MovedFrom | FileObserverEvents.MovedTo | (FileObserverEvents)0x40000000;
            Console.WriteLine(Mask.ToString());
            this.fileObserver = new FilePickerFileObserver(path, Mask);
            this.fileObserver.OnFileEvent += (events, s) =>
            {
                LogDebug(string.Format("FileObserver event received - {0}", events));
                if ((events & (FileObserverEvents)0x40000000) == (FileObserverEvents)0x40000000)
                {
                    Console.WriteLine("Folder event");
                }
                events &= FileObserverEvents.AllEvents;
                var eventName = Enum.GetName(typeof(FileObserverEvents), events);
                Console.WriteLine(eventName);
                if ((events & Mask) == events)
                    if (this.Activity != null)
                    {
                        this.Activity.RunOnUiThread(this.RefreshFilesList);
                    }

            };
        }
    }

    public delegate void OnFileEventHandler(FileObserverEvents e, string path);

    public class FilePickerFileObserver : FileObserver
    {
        /// <inheritdoc />
        public event OnFileEventHandler OnFileEvent;

        /// <inheritdoc />
        public FilePickerFileObserver(IntPtr javaReference, JniHandleOwnership transfer)
            : base(javaReference, transfer)
        {
        }

        /// <inheritdoc />
        public FilePickerFileObserver(string path)
            : base(path)
        {
        }

        /// <inheritdoc />
        public FilePickerFileObserver(string path, FileObserverEvents mask)
            : base(path, mask)
        {
        }

        /// <inheritdoc />
        public override void OnEvent(FileObserverEvents e, string path)
        {
            OnFileEvent(e, path);
        }
    }

    public enum FilePickerMode
    {
        /// <summary>
        /// The file
        /// </summary>
        File = 1,

        /// <summary>
        /// The directory
        /// </summary>
        Directory = 2
    }

    public static class Helpers
    {
        /// <summary>
        /// Will obtain an instance of a LayoutInflater for the specified Context.
        /// </summary>
        /// <param name="context">The Context </param>
        /// <returns> </returns>
        public static LayoutInflater GetLayoutInflater(this Context context)
        {
            return context.GetSystemService(Context.LayoutInflaterService).JavaCast<LayoutInflater>();
        }

        /// <summary>
        /// This method will tell us if the given FileSystemInfo instance is a directory.
        /// </summary>
        /// <param name="fileSystemInfo">The FileSystemInfo </param>
        /// <returns> </returns>
        public static bool IsDirectory(this FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null)
            {
                return false;
            }

            return (fileSystemInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
        }

        /// <summary>
        /// This method will tell us if the given FileSystemInfo instance is a file.
        /// </summary>
        /// <param name="fileSystemInfo">The FileSystemInfo </param>
        /// <returns> </returns>
        public static bool IsFile(this FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null)
            {
                return false;
            }
            return !IsDirectory(fileSystemInfo);
        }

        /// <summary>
        /// This method will tell us if the given FileSystemInfo instance is visible.
        /// </summary>
        /// <param name="fileSystemInfo">The FileSystemInfo </param>
        /// <returns> </returns>
        public static bool IsVisible(this FileSystemInfo fileSystemInfo)
        {
            if (fileSystemInfo == null)
            {
                return false;
            }
            var isHidden = (fileSystemInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden;
            return !isHidden;
        }
    }

    public class FileListAdapter : ArrayAdapter<FileSystemInfo>
    {
        private readonly Context context;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileListAdapter"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="fileSystemInfos">The list of FileSystemInfo containing the directory contents.</param>
        public FileListAdapter(Context context, IList<FileSystemInfo> fileSystemInfos)
            : base(context, Resource.Layout.filepicker_listitem, Android.Resource.Id.Text1, fileSystemInfos)
        {
            this.context = context;
        }

        /// <summary>
        /// We provide this method to get around some of the //todo fix this comment
        /// </summary>
        /// <param name="directoryContents">The directory contents.</param>
        public void AddDirectoryContents(IEnumerable<FileSystemInfo> directoryContents)
        {
            Clear();
            // Notify the adapter that things have changed or that there is nothing to display.
            var fileSystemInfos = directoryContents as IList<FileSystemInfo> ?? directoryContents.ToList();
            if (fileSystemInfos.Any())
            {
                AddAll(fileSystemInfos.ToArray());
                NotifyDataSetChanged();
            }
            else
            {
                NotifyDataSetInvalidated();
            }
        }

        /// <inheritdoc/>
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var fileSystemEntry = GetItem(position);

            FileListRowViewHolder viewHolder;
            View row;
            if (convertView == null)
            {
                row = context.GetLayoutInflater().Inflate(Resource.Layout.filepicker_listitem, parent, false);
                viewHolder = new FileListRowViewHolder(row.FindViewById<TextView>(Resource.Id.file_picker_text), row.FindViewById<ImageView>(Resource.Id.file_picker_image));
                row.Tag = viewHolder;
            }
            else
            {
                row = convertView;
                viewHolder = (FileListRowViewHolder)row.Tag;
            }
            viewHolder.Update(fileSystemEntry.Name, fileSystemEntry.IsDirectory() ? Resource.Drawable.filepicker_folder : Resource.Drawable.filepicker_file);
            return row;
        }
    }

    [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1121:UseBuiltInTypeAlias", Justification = "Using Java.Lang.Object")]
    public class FileListRowViewHolder : Object
    {
        /// <summary>
        /// Gets the image view to display the file/folder icon.
        /// </summary>
        /// <value>
        /// The image view.
        /// </value>
        public ImageView IconImageView { get; private set; }

        /// <summary>
        /// Gets the text view.
        /// </summary>
        /// <value>
        /// The text view.
        /// </value>
        public TextView NameTextView { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileListRowViewHolder"/> class.
        /// </summary>
        /// <param name="textView">The text view.</param>
        /// <param name="imageView">The image view.</param>
        public FileListRowViewHolder(TextView textView, ImageView imageView)
        {
            NameTextView = textView;
            IconImageView = imageView;
        }

        /// <summary>
        /// This method will update the TextView and the ImageView with the filename and the file/folder icon
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileImageResourceId">The file image resource identifier.</param>
        public void Update(string fileName, int fileImageResourceId)
        {
            NameTextView.Text = fileName;
            IconImageView.SetImageResource(fileImageResourceId);
        }
    }

    public delegate void FileSelectedEventHandler(object sender, string path);
    public delegate void CancelEventHandler(object sender);
}