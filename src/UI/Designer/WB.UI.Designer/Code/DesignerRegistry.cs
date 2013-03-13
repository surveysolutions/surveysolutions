// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DesignerRegistry.cs" company="">
//   
// </copyright>
// <summary>
//   The designer registry.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WB.UI.Designer
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Main.Core;

    using RavenQuestionnaire.Core.Views.Questionnaire;

    using WB.UI.Designer.Providers.CQRS.Accounts;

    /// <summary>
    ///     The designer registry.
    /// </summary>
    public class DesignerRegistry : CoreRegistry
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DesignerRegistry"/> class.
        /// </summary>
        /// <param name="repositoryPath">
        /// The repository path.
        /// </param>
        /// <param name="isEmbeded">
        /// The is embeded.
        /// </param>
        public DesignerRegistry(string repositoryPath, bool isEmbeded)
            : base(repositoryPath, isEmbeded)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The get assweblys for register.
        /// </summary>
        /// <returns>
        ///     The <see cref="IEnumerable" />.
        /// </returns>
        public override IEnumerable<Assembly> GetAssweblysForRegister()
        {
            return
                base.GetAssweblysForRegister()
                    .Concat(new[]
                    {
                        typeof(DesignerRegistry).Assembly,
                        typeof(QuestionnaireView).Assembly,
                        typeof(AccountAR).Assembly,
                    });
        }

        #endregion
    }
}