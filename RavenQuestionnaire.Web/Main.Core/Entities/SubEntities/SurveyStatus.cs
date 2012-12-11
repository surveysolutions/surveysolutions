// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyStatus.cs" company="">
//   
// </copyright>
// <summary>
//   The survey status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// The survey status.
    /// </summary>
    public class SurveyStatus
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        public SurveyStatus()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public SurveyStatus(Guid id)
        {
            this.PublicId = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        public SurveyStatus(Guid id, string name)
        {
            this.PublicId = id;
            this.Name = name;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the approve.
        /// </summary>
        public static SurveyStatus Approve
        {
            get
            {
                var identifier = new Guid("AA6C0DC1-23C4-4B03-A3ED-B24EF0055555");
                string name = "Approved";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the complete.
        /// </summary>
        public static SurveyStatus Complete
        {
            get
            {
                var identifier = new Guid("776C0DC1-23C4-4B03-A3ED-B24EF005559B");
                string name = "Completed";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public static SurveyStatus Error
        {
            get
            {
                var identifier = new Guid("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F");
                string name = "Completed with error";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets the initial.
        /// </summary>
        public static SurveyStatus Initial
        {
            get
            {
                var identifier = new Guid("8927D124-3CFB-4374-AD36-2FD99B62CE13");
                string name = "Initial";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets Redo.
        /// </summary>
        public static SurveyStatus Redo
        {
            get
            {
                var identifier = new Guid("2bb6f94d-5beb-4374-8749-fac7cee1e020");
                string name = "Redo";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets Redo.
        /// </summary>
        public static SurveyStatus Unassign
        {
            get
            {
                var identifier = new Guid("4da8dddb-b31d-4508-bde6-178160705ba1");
                string name = "Unassign";
                return new SurveyStatus(identifier, name);
            }
        }

        /// <summary>
        /// Gets or sets the change comment.
        /// </summary>
        public string ChangeComment { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the public id.
        /// </summary>
        public Guid PublicId { get; set; }

        #endregion

        #region Public Methods and Operators
        /// <summary>
        /// To string
        /// </summary>
        /// <returns>
        /// The string
        /// </returns>
        public override string ToString()
        {
            return string.Format("{0}: [{1}]", this.Name, this.PublicId);
        }

        /// <summary>
        /// The get all statuses.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Main.Core.Entities.SubEntities.SurveyStatus].
        /// </returns>
        public static IEnumerable<SurveyStatus> GetAllStatuses()
        {
            return new[] { Unassign, Initial, Error, Complete, Approve, Redo };
        }

        /// <summary>
        /// check status on allowance to be pushed from capi
        /// </summary>
        /// <param name="status">
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public static bool IsStatusAllowCapiSync(SurveyStatus status)
        {
            return status.PublicId == Complete.PublicId || status.PublicId == Error.PublicId;
        }

        /// <summary>
        /// The is status allow down supervisor sync.
        /// </summary>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <returns>
        /// The System.Boolean.
        /// </returns>
        public static bool IsStatusAllowDownSupervisorSync(SurveyStatus status)
        {            
             return status.PublicId == SurveyStatus.Initial.PublicId || status.PublicId == SurveyStatus.Approve.PublicId || status.PublicId == SurveyStatus.Redo.PublicId;
        }

        /// <summary>
        /// check status on allowance to be pushed from capi
        /// </summary>
        /// <param name="status">
        /// </param>
        /// <returns>
        /// The Main.Core.Entities.SubEntities.SurveyStatus.
        /// </returns>
        public static SurveyStatus IsValidStatus(string statusName)
        {
            if (statusName == Approve.Name)
            {
                return Approve;
            }

            if (statusName == Error.Name)
            {
                return Error;
            }

            if (statusName == Complete.Name)
            {
                return Complete;
            }

            if (statusName == Initial.Name)
            {
                return Initial;
            }

            if (statusName == Redo.Name)
            {
                return Redo;
            }

            if (statusName == Unassign.Name)
            {
                return Unassign;
            }

            return null;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(SurveyStatus))
            {
                return false;
            }

            return Equals((SurveyStatus)obj);
        }
        #endregion

        public bool Equals(SurveyStatus other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Equals(other.Name, this.Name) && other.PublicId.Equals(this.PublicId);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((this.Name != null ? this.Name.GetHashCode() : 0) * 947) ^ this.PublicId.GetHashCode();
            }
        }

        public static bool operator ==(SurveyStatus left, SurveyStatus right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SurveyStatus left, SurveyStatus right)
        {
            return !Equals(left, right);
        }
    }
}