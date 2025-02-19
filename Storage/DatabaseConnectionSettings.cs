﻿namespace Storage
{
    /// <summary>
    /// A class for the database connection settings.
    /// </summary>
    public class DatabaseConnectionSettings
    {
        /// <summary>
        /// Gets or sets the host of the database.
        /// </summary>
        public string Host { get; set; } = "localhost";

        /// <summary>
        /// Gets or sets the database name.
        /// </summary>
        public string Database { get; set; } = "mqtt";

        /// <summary>
        /// Gets or sets the user name.
        /// </summary>
        public string Username { get; set; } = "mqtt";

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        public string Password { get; set; } = "mqtt";

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        public int Port { get; set; } = 5432;

        /// <summary>
        /// Gets or sets a value indicating whether the pooling is enabled or not.
        /// </summary>
        public bool Pooling { get; set; } = false;

        /// <summary>
        /// Gets or sets the time zone.
        /// </summary>
        public string Timezone { get; set; } = "Europe/Berlin";

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <returns>The connection string from the connection settings.</returns>
        public string ToConnectionString()
        {
            return
                $"Server={this.Host};Port={this.Port};Database={this.Database};User Id={this.Username};Password={this.Password};Pooling={this.Pooling};Timezone={this.Timezone}";
        }
    }
}