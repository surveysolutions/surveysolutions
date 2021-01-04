namespace WB.UI.Headquarters.Controllers.Api.PublicApi.Models
{
    /// <summary>
    /// Assignment mode for workspaces
    /// </summary>
    public enum AssignWorkspacesMode
    {
        /// <summary>
        /// Assign and overwrite workspaces list for user
        /// </summary>
        Assign, 
        
        /// <summary>
        /// Add workspaces to user
        /// </summary>
        Add, 
        
        /// <summary>
        /// Remove workspaces from user
        /// </summary>
        Remove
    }
}
