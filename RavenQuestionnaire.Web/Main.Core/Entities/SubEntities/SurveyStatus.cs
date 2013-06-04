// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SurveyStatus.cs" company="The World Bank">
//   2012
// </copyright>
// <summary>
//   The survey status.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Main.Core.Entities.SubEntities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// The survey status.
    /// </summary>
    public struct SurveyStatus
    {
        // IF YOU WANT TO CHANGE THIS STRUCTURE TO THE CLASS
        // PAY ATTENTION THAT IT IS USED AS A MEMBER OF OBJECTS
        // WHICH are used MEMBERWISECLONE 



        /* /// <summary>
        /// Initializes a new instance of the <see cref="SurveyStatus"/> class.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        public SurveyStatus(Guid id)
        {
            PublicId = id;
        }*/

        /*/// <summary>
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
            PublicId = id;
            this.Name = name;
        }*/
        #region Public Properties

        /// <summary>
        /// Gets the approve.
        /// </summary>
        public static SurveyStatus Approve
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("AA6C0DC1-23C4-4B03-A3ED-B24EF0055555"), Name = "Approved" 
                    };
            }
        }

        /// <summary>
        /// Gets the complete.
        /// </summary>
        public static SurveyStatus Complete
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("776C0DC1-23C4-4B03-A3ED-B24EF005559B"), Name = "Completed" 
                    };
            }
        }

        /// <summary>
        /// Gets the error.
        /// </summary>
        public static SurveyStatus Error
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("D65CF1F6-8A75-43FA-9158-B745EB4D6A1F"), Name = "Completed with error" 
                    };
            }
        }

        /// <summary>
        /// Gets the initial.
        /// </summary>
        public static SurveyStatus Initial
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("8927D124-3CFB-4374-AD36-2FD99B62CE13"), Name = "Initial" 
                    };
            }
        }
        /// <summary>
        /// Gets the initial.
        /// </summary>
        public static SurveyStatus Reinit
        {
            get
            {
                return new SurveyStatus
                {
                    PublicId = new Guid("67e7ec3e-66d3-40ff-995a-94b8b5d0583c"),
                    Name = "Reinit"
                };
            }
        }
        /// <summary>
        /// Gets Redo.
        /// </summary>
        public static SurveyStatus Redo
        {
            get
            {
                return new SurveyStatus { PublicId = new Guid("2bb6f94d-5beb-4374-8749-fac7cee1e020"), Name = "Redo" };
            }
        }

        /// <summary>
        /// Gets Redo.
        /// </summary>
        public static SurveyStatus Unassign
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("4da8dddb-b31d-4508-bde6-178160705ba1"), Name = "Unassigned" 
                    };
            }
        }

        /// <summary>
        /// Gets Unknown.
        /// </summary>
        public static SurveyStatus Unknown
        {
            get
            {
                return new SurveyStatus
                    {
                       PublicId = new Guid("EAA0AA81-CCF1-4FB6-91C7-861264EC3FC9"), Name = "Unknown" 
                    };
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
        /// The get all statuses.
        /// </summary>
        /// <returns>
        /// The System.Collections.Generic.IEnumerable`1[T -&gt; Main.Core.Entities.SubEntities.SurveyStatus].
        /// </returns>
        public static IEnumerable<SurveyStatus> GetAllStatuses()
        {
            return new[] { Unassign, Initial, Error, Complete, Approve, Redo, Unknown };
        }

        /// <summary>
        /// The get status by id or default.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="SurveyStatus"/>.
        /// </returns>
        public static SurveyStatus GetStatusByIdOrDefault(Guid? id)
        {
            if (!id.HasValue)
                return SurveyStatus.Unknown;
            return SurveyStatus.GetAllStatuses().FirstOrDefault(s => s.PublicId == id.Value);
        }

        /// <summary>
        /// The get status by name.
        /// </summary>
        /// <param name="statusName">
        /// The status name.
        /// </param>
        /// <returns>
        /// The <see cref="SurveyStatus"/>.
        /// </returns>
        public static SurveyStatus GetStatusByNameOrDefault(string statusName)
        {
            var status = SurveyStatus.GetAllStatuses().FirstOrDefault(s => s.Name == statusName);
            return status.PublicId == Guid.Empty ? SurveyStatus.Unknown : status;

            /*
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

            return Unknown;*/
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
            return GetListOfAllowerdStatusesForSync().Contains(status.PublicId);
        }
        public static IEnumerable<Guid> GetListOfAllowerdStatusesForSync()
        {
            return new Guid[] {Complete.PublicId, Error.PublicId};
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
            return status.PublicId == Initial.PublicId || status.PublicId == Approve.PublicId
                   || status.PublicId == Redo.PublicId;
        }

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

        #endregion

        public override bool Equals(object obj)
        {
         /* if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }*/


            if (obj.GetType() != typeof(SurveyStatus))
            {
                return false;
            }

            SurveyStatus item = (SurveyStatus)obj;

            return Equals((SurveyStatus)obj);
        }
       

        public bool Equals(SurveyStatus other)
        {
            /*if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }*/

            // ignoring name 
            return /*Equals(other.Name, this.Name) && */other.PublicId.Equals(this.PublicId);
        }

        public override int GetHashCode()
        {
            return this.PublicId.GetHashCode();

            /*unchecked
            {
                return ((this.Name != null ? this.Name.GetHashCode() : 0) * 947) ^ this.PublicId.GetHashCode();
            }*/
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