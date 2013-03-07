namespace WB.UI.Designer.Providers.Repositories.RavenDb
{
    /// <summary>
    /// Role document
    /// </summary>
    public class Role
    {
        /// <summary>
        /// Gets or sets role name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets application that the role exists in
        /// </summary>
        public string ApplicationName { get; set; }
    }
}