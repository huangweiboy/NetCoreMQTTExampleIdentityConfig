﻿namespace Storage.Database
{
    using System;

    using Microsoft.AspNetCore.Identity;

    /// <summary>
    /// This class contains the customized user data if necessary.
    /// </summary>
    /// <seealso cref="IdentityUser{TKey}" />
    public class User : IdentityUser<long>
    {
        /// <summary>
        /// Gets or sets the client identifier prefix.
        /// </summary>
        public virtual string ClientIdPrefix { get; set; }

        /// <summary>
        /// Gets or sets the client identifier.
        /// </summary>
        public virtual string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the created at timestamp.
        /// </summary>
        public virtual DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// Gets or sets the updated at timestamp.
        /// </summary>
        public virtual DateTimeOffset? UpdatedAt { get; set; } = null;

        /// <summary>
        /// Returns a <seealso cref="string"/> which represents the object instance.
        /// </summary>
        /// <returns>A <seealso cref="string"/> representation of the instance.</returns>
        public override string ToString()
        {
            return $"{base.ToString()}, {nameof(this.ClientIdPrefix)}: {this.ClientIdPrefix}, {nameof(this.ClientId)}: {this.ClientId}, {nameof(this.CreatedAt)}: {this.CreatedAt}, {nameof(this.UpdatedAt)}: {this.UpdatedAt}";
        }
    }
}