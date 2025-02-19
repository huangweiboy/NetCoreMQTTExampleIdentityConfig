﻿
namespace NetCoreMQTTExampleIdentityConfig.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using NetCoreMQTTExampleIdentityConfig.Controllers.Extensions;

    using Newtonsoft.Json;

    using NSwag.Annotations;

    using Serilog;
    using Storage;
    using Storage.Database;
    using Storage.Dto;

    /// <summary>
    /// The claim controller class.
    /// </summary>
    /// <seealso cref="ControllerBase" />
    [Route("api/claim")]
    [ApiController]
    [OpenApiTag("Claim", Description = "Claim management.")]
    public class ClaimController : ControllerBase
    {
        /// <summary>
        /// The database context.
        /// </summary>
        private readonly MqttContext databaseContext;

        /// <summary>
        /// The automapper.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        private readonly IMapper autoMapper;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimController"/> class.
        /// </summary>
        /// <param name="databaseContext">The database context.</param>
        /// <param name="autoMapper">The automapper service.</param>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        public ClaimController(MqttContext databaseContext, IMapper autoMapper)
        {
            this.databaseContext = databaseContext;
            this.autoMapper = autoMapper;
        }

        /// <summary>
        /// Gets the claims. GET "api/claim".
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpGet]
        [SwaggerResponse(HttpStatusCode.OK, typeof(IEnumerable<DtoReadUserClaim>), Description = "Claims found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "Internal server error.")]
        [ProducesResponseType(typeof(IEnumerable<DtoReadUserClaim>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<DtoReadUserClaim>>> GetClaims()
        {
            try
            {
                Log.Information("Executed GetClaims().");

                var claims = await this.databaseContext.UserClaims.ToListAsync();

                if (claims?.Count == 0)
                {
                    return this.Ok("[]");
                }

                var returnUserClaims = this.autoMapper.Map<List<DtoReadUserClaim>>(claims);
                return this.Ok(returnUserClaims);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Gets the claim by id. GET "api/claim/5".
        /// </summary>
        /// <param name="claimId">
        /// The claim identifier.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpGet("{claimId}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DtoReadUserClaim), Description = "Claim found.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(int), Description = "Claim not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "Internal server error.")]
        [ProducesResponseType(typeof(DtoReadUserClaim), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(int), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DtoReadUserClaim>> GetClaimById(long claimId)
        {
            try
            {
                Log.Information($"Executed GetClaimById({claimId}).");

                var claim = await this.databaseContext.UserClaims.FirstOrDefaultAsync(u => u.Id == claimId);

                if (claim == null)
                {
                    Log.Warning($"Claim with identifier {claimId} not found.");
                    return this.NotFound(claimId);
                }

                var returnUserClaim = this.autoMapper.Map<DtoReadUserClaim>(claim);
                return this.Ok(returnUserClaim);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Creates the claim. POST "api/claim".
        /// </summary>
        /// <param name="createUserClaim">
        /// The create User Claim.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpPost]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DtoReadUserClaim), Description = "Claim created.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "Internal server error.")]
        [ProducesResponseType(typeof(DtoReadUserClaim), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> CreateOrUpdateClaim([FromBody] DtoCreateUpdateUserClaim createUserClaim)
        {
            try
            {
                Log.Information($"Executed CreateOrUpdateClaim({createUserClaim}).");

                var claim = this.autoMapper.Map<UserClaim>(createUserClaim);
                claim.CreatedAt = DateTimeOffset.Now;

                var foundClaim = await this.databaseContext.UserClaims.FirstOrDefaultAsync(
                    uc => uc.ClaimType == claim.ClaimType && uc.UserId == claim.UserId);

                DtoReadUserClaim returnUserClaim;

                if (foundClaim == null)
                {
                    claim.CreatedAt = DateTimeOffset.Now;
                    await this.databaseContext.UserClaims.AddAsync(claim);
                    await this.databaseContext.SaveChangesAsync();
                    returnUserClaim = this.autoMapper.Map<DtoReadUserClaim>(claim);
                }
                else
                {
                    foundClaim.UpdatedAt = DateTimeOffset.Now;
                    var currentClaimValue = JsonConvert.DeserializeObject<List<string>>(foundClaim.ClaimValue);
                    currentClaimValue.AddRange(createUserClaim.ClaimValues);
                    foundClaim.ClaimValue = JsonConvert.SerializeObject(currentClaimValue.Distinct());
                    this.databaseContext.UserClaims.Update(foundClaim);
                    await this.databaseContext.SaveChangesAsync();
                    returnUserClaim = this.autoMapper.Map<DtoReadUserClaim>(foundClaim);
                }

                return this.Ok(returnUserClaim);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Updates the claim. PUT "api/claim/5".
        /// </summary>
        /// <param name="claimId">
        /// The claim identifier.
        /// </param>
        /// <param name="updateUserClaim">
        /// The update User Claim.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpPut("{claimId}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(DtoReadUserClaim), Description = "Claim updated.")]
        [SwaggerResponse(HttpStatusCode.NotFound, typeof(int), Description = "Claim not found.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "Internal server error.")]
        [ProducesResponseType(typeof(DtoReadUserClaim), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(int), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateClaim(long claimId, [FromBody] DtoCreateUpdateUserClaim updateUserClaim)
        {
            try
            {
                Log.Information($"Executed UpdateClaim({updateUserClaim}) for claim identifier: {claimId}.");

                var resultClaim = await this.databaseContext.UserClaims.AsNoTracking().FirstOrDefaultAsync(b => b.Id == claimId);

                if (resultClaim == null)
                {
                    Log.Warning($"Claim with identifier {claimId} not found.");
                    return this.NotFound(claimId);
                }

                var createdAt = resultClaim.CreatedAt;
                resultClaim = this.autoMapper.Map<UserClaim>(updateUserClaim);
                resultClaim.UpdatedAt = DateTimeOffset.Now;
                resultClaim.CreatedAt = createdAt;
                resultClaim.Id = claimId;
                this.databaseContext.UserClaims.Update(resultClaim);
                await this.databaseContext.SaveChangesAsync();
                var returnUserClaim = this.autoMapper.Map<DtoReadUserClaim>(resultClaim);
                return this.Ok(returnUserClaim);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
                return this.InternalServerError(ex);
            }
        }

        /// <summary>
        /// Deletes the claim by id. DELETE "api/claim/5".
        /// </summary>
        /// <param name="claimId">
        /// The claim identifier.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        [HttpDelete("{claimId}")]
        [SwaggerResponse(HttpStatusCode.OK, typeof(int), Description = "Claim deleted.")]
        [SwaggerResponse(HttpStatusCode.InternalServerError, typeof(string), Description = "Internal server error.")]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteClaimById(long claimId)
        {
            try
            {
                Log.Information($"Executed DeleteClaimById({claimId}).");

                var claim = await this.databaseContext.UserClaims.AsNoTracking().FirstOrDefaultAsync(c => c.Id == claimId);

                if (claim == null)
                {
                    return this.Ok(claimId);
                }

                this.databaseContext.UserClaims.Remove(claim);
                await this.databaseContext.SaveChangesAsync();
                return this.Ok(claimId);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex.Message, ex);
                return this.InternalServerError(ex);
            }
        }
    }
}
