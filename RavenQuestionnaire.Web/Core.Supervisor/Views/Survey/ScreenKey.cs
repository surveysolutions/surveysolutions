namespace Core.Supervisor.Views.Survey
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    using Main.Core.Entities.SubEntities;
    using Main.Core.Entities.SubEntities.Complete;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ScreenKey
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenKey"/> class.
        /// </summary>
        /// <param name="group">
        /// The group.
        /// </param>
        public ScreenKey(ICompleteGroup group)
            : this(group.PublicKey, group.PropagationPublicKey, group.Propagated)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScreenKey"/> class.
        /// </summary>
        /// <param name="publicKey">
        /// The public key.
        /// </param>
        /// <param name="propagationPublicKey">
        /// The propagation public key.
        /// </param>
        /// <param name="propagated">
        /// The propagated.
        /// </param>
        public ScreenKey(Guid publicKey, Guid? propagationPublicKey, Propagate propagated)
        {
            this.Propagated = propagated;
            this.PublicKey = publicKey;
            this.PropagationKey = propagationPublicKey.HasValue ? propagationPublicKey.Value : Guid.Empty;
        }

        /// <summary>
        /// Gets a value indicating whether isPropagated.
        /// </summary>
        public bool IsPropagated
        { 
            get
            {
                return this.Propagated != Propagate.None;
            }
        }

        /// <summary>
        /// Gets a value indicating whether isTemplate.
        /// </summary>
        public bool IsTemplate
        {
            get
            {
                return this.IsPropagated && this.PropagationKey == Guid.Empty;
            }
        }


        /// <summary>
        /// Gets or sets the propagated.
        /// </summary>
        public Propagate Propagated { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PublicKey { get; set; }

        /// <summary>
        /// Gets or sets the public key.
        /// </summary>
        public Guid PropagationKey { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="obj">
        /// The obj.
        /// </param>
        /// <returns>
        /// </returns>
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

            if (obj.GetType() != typeof(ScreenKey))
            {
                return false;
            }

            return Equals((ScreenKey)obj);
        }

        /// <summary>
        /// </summary>
        /// <param name="other">
        /// The other.
        /// </param>
        /// <returns>
        /// </returns>
        public bool Equals(ScreenKey other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return other.PublicKey.Equals(this.PublicKey) && other.PropagationKey.Equals(this.PropagationKey);
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (this.PublicKey.GetHashCode() * 397) ^ this.PropagationKey.GetHashCode();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <returns>
        /// </returns>
        public static bool operator ==(ScreenKey left, ScreenKey right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// </summary>
        /// <param name="left">
        /// The left.
        /// </param>
        /// <param name="right">
        /// The right.
        /// </param>
        /// <returns>
        /// </returns>
        public static bool operator !=(ScreenKey left, ScreenKey right)
        {
            return !Equals(left, right);
        }
    }
}
