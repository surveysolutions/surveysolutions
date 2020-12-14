namespace StatData.Core
{
    /// <summary>
    /// Delegate for progress update event handlers 
    /// </summary>
    /// <param name="sender">Sender of the progress update</param>
    /// <param name="e">Progress update status</param>
    public delegate void ProgressChangedDelegate(object sender, ProgressChangedArgs e);
}
