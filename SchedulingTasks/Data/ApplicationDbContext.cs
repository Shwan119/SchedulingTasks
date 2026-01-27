using Entities;
using Microsoft.EntityFrameworkCore;
using Proxies;
using ReportInventory.Api.Mock.Controllers.ReportSubscription.SharedKernel;
using ReportSubscription.Application.Abstractions;
using ReportSubscription.Application.DTOs;
using ReportSubscription.Infrastructure.Clients;
using Request;
using Response;
using SchedulingTasks.Models;
using Shared.Request;
using Shared.Seeding;
using Subscription.Validators;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Validators;
using YourProject.Domain.Common;
using YourProject.Domain.Entities;
using static ReportInventory.Api.Mock.Controllers.ReportSubscription.Application.Services.ReportAccessService;
using Endpoint = SchedulingTasks.Models.Endpoint;

namespace SchedulingTasks.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<ScheduledTask> ScheduledTasks { get; set; }
        public DbSet<Endpoint> Endpoints { get; set; }
        public DbSet<TaskExecution> TaskExecutions { get; set; }
        public DbSet<TaskExecutionLog> TaskExecutionLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}

@using ROV.Common.DataModel
@using ROV.ReportHub.ViewModels
@model ManageReportingOrgViewModel
@{
    ViewBag.Title = "Manage ReportingOrg";
}

<link href="~/css/styles.css" rel="stylesheet" />

@{
    AppUser _curUser = PageData["CurrentUser"] as AppUser;
}

<div class="container">
    @using (Html.BeginForm("ManageReportingOrg", "Division", FormMethod.Post, new { id = "frmManageReportingOrg" }))
    {
        @Html.AntiForgeryToken()

        <h1>Reporting Organisation</h1>

        <!-- Division Selector -->
        <div class="selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                @Html.DropDownListFor(m => m.SelectedDivisionID,
                    new SelectList(Model.Divisions, "ID", "CodeName"),
                    "(Please Select)")
            </div>
        </div>

        @if (Model.SelectedDivisionID > 0)
        {
            @Html.HiddenFor(m => m.PageAction)

            @if (Model.SelectedReportingOrgID > 0)
            {
                var repOrg = Model.TheReportingOrg;

                @Html.HiddenFor(m => m.TheReportingOrg.ID)
                @Html.HiddenFor(m => m.TheReportingOrg.Code)
                @Html.HiddenFor(m => m.TheReportingOrg.Description)
                @Html.HiddenFor(m => m.TheReportingOrg.ActiveFlag)
                @Html.HiddenFor(m => m.TheReportingOrg.AutoPublishFlag)
                @Html.HiddenFor(m => m.TheReportingOrg.Division_ID)
                @Html.HiddenFor(m => m.TheReportingOrg.StoragePath)
                @Html.HiddenFor(m => m.TheReportingOrg.AutoPublishSource)

                if (Model.TheReportingOrg.Code == "SOAR")
                {
                    @Html.HiddenFor(m => m.TheReportingOrg.MSPAdmins)
                    @Html.HiddenFor(m => m.TheReportingOrg.MSPVisible)
                    @Html.HiddenFor(m => m.TheReportingOrg.SDPAdmins)
                    @Html.HiddenFor(m => m.TheReportingOrg.SDPVisible)
                }

                string acGeneral = Model.PageAction == ManageReportingOrgPageAction.paGeneral ? "active" : "";
                string acCommunication = Model.PageAction == ManageReportingOrgPageAction.paCommunication ? "active" : "";
                string acReportCustomFields = Model.PageAction == ManageReportingOrgPageAction.paReportCustomFields ? "active" : "";
                string acProcessCustomFields = Model.PageAction == ManageReportingOrgPageAction.paProcessCustomFields ? "active" : "";

                <!-- Tabs -->
                <div class="tabs">
                    <div class="tab @acGeneral" data-tab="general" id="btnSwitchTab_@((int)ManageReportingOrgPageAction.paGeneral)">General</div>
                    <div class="tab @acCommunication" data-tab="communication" id="btnSwitchTab_@((int)ManageReportingOrgPageAction.paCommunication)">Communication</div>
                    <div class="tab @acReportCustomFields" data-tab="report-custom" id="btnSwitchTab_@((int)ManageReportingOrgPageAction.paReportCustomFields)">Report Custom Fields</div>
                    <div class="tab @acProcessCustomFields" data-tab="process-custom" id="btnSwitchTab_@((int)ManageReportingOrgPageAction.paProcessCustomFields)">Process Custom Fields</div>
                </div>

                <!-- General Tab Content -->
                <div id="general-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paGeneral ? "active" : "")">
                    <!-- Reporting Organisation Selector -->
                    <div class="selector-row">
                        <label>Reporting Organisation</label>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                                new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                                "(Please Select)")
                        </div>
                    </div>

                    <!-- Organisation Info Card -->
                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title" id="org-name-display">@repOrg.NameCode</span>
                            @if (repOrg.ActiveFlag == true)
                            {
                                <span class="status-badge status-active" id="status-badge">Active</span>
                            }
                            else
                            {
                                <span class="status-badge status-inactive" id="status-badge">Inactive</span>
                            }
                        </div>
                        <div class="header-actions">
                            <button type="button" class="btn btn-outline" id="edit-details-btn">Edit Details</button>
                            <button type="button" class="btn btn-save hidden" id="save-details-btn">Save Changes</button>
                            <button type="button" class="btn btn-cancel hidden" id="cancel-details-btn">Cancel</button>
                        </div>
                    </div>

                    <!-- Details Grid -->
                    <div class="details-grid" id="details-grid-container">
                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.Name)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="Name">@repOrg.Name</span>
                            </div>
                            @Html.HiddenFor(m => m.TheReportingOrg.Name, new { @class = "edit-field" })
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.StoragePath)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="StoragePath">@repOrg.StoragePath</span>
                            </div>
                            @if (_curUser.IsTechAdmin)
                            {
                                @Html.HiddenFor(m => m.TheReportingOrg.StoragePath, new { @class = "edit-field", @id = "storagePath" })
                            }
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishSource)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="AutoPublishSource">@repOrg.AutoPublishSource</span>
                            </div>
                            @if (_curUser.IsTechAdmin)
                            {
                                @Html.HiddenFor(m => m.TheReportingOrg.AutoPublishSource, new { @class = "edit-field", @id = "autoPublishSource" })
                            }
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishFlag)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="AutoPublishFlag">@(repOrg.AutoPublishFlag == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.ExpirationAlert)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="ExpirationAlert">@(repOrg.ExpirationAlert == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutomaticRetention)</span>
                            <div class="detail-value-box">
                                <span class="display-value" data-field="AutomaticRetention">@(repOrg.AutomaticRetention == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>
                    </div>

                    <!-- Retention Settings Section -->
                    <div class="details-grid" id="divRetentionParent" style="border-top: none; padding-top: 0;">
                        <div class="detail-item" style="grid-column: span 3;">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.ExpirationAlert, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.ExpirationAlert) - Select this option if you wish to receive monthly email about expiring reports.</span>
                            </label>
                        </div>

                        <div class="detail-item" style="grid-column: span 3;">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.AutomaticRetention, new { @class = "cgUpdateReportingOrg csGetConfirmation" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.AutomaticRetention) - Selecting this option enables automatic retirement for your reports.</span>
                            </label>
                        </div>
                    </div>
                </div>

                <!-- Communication Tab Content -->
                <div id="communication-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paCommunication ? "active" : "")">
                    <div class="selector-row">
                        <label>Reporting Organisation</label>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                                new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                                "(Please Select)",
                                new { @id = "commReportingOrgDropdown" })
                        </div>
                    </div>

                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Communication Settings</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                        <button type="button" class="btn btn-outline">Edit Details</button>
                    </div>

                    <div class="details-grid">
                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportEmail)</span>
                            <div class="detail-value-box">
                                <span class="display-value">@repOrg.SupportEmail</span>
                            </div>
                            @Html.HiddenFor(m => m.TheReportingOrg.SupportEmail)
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportSite)</span>
                            <div class="detail-value-box">
                                <span class="display-value">@repOrg.SupportSite</span>
                            </div>
                            @Html.HiddenFor(m => m.TheReportingOrg.SupportSite)
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.PACanApproveRequest) <span class="info-icon" title="Select here for if you want the Primary Analyst aligned to the report to be able to approve access">ⓘ</span></span>
                            <div class="detail-value-box">
                                <span class="display-value">@(repOrg.PACanApproveRequest == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.BACanApproveRequest) <span class="info-icon" title="Select here for if you want the Backup Analyst aligned to the report to be able to approve access">ⓘ</span></span>
                            <div class="detail-value-box">
                                <span class="display-value">@(repOrg.BACanApproveRequest == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.RTMCanApproveRequest) <span class="info-icon" title="Select here for if you want the Reporting Team Manager aligned to the report to be able to approve access">ⓘ</span></span>
                            <div class="detail-value-box">
                                <span class="display-value">@(repOrg.RTMCanApproveRequest == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.PLCCanApproveRequest) <span class="info-icon" title="Select here for if you want the Primary LOB Contact/Owner aligned to the report to be able to approve access">ⓘ</span></span>
                            <div class="detail-value-box">
                                <span class="display-value">@(repOrg.PLCCanApproveRequest == true ? "Enabled" : "Disabled")</span>
                            </div>
                        </div>
                    </div>

                    <!-- Report Access Approvers Section -->
                    <div class="info-card" style="margin-top: 24px;">
                        <div class="info-card-left">
                            <span class="info-card-title">Report Access Approvers</span>
                        </div>
                    </div>

                    <div class="details-grid">
                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.PACanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.PACanApproveRequest)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.BACanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.BACanApproveRequest)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.RTMCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.RTMCanApproveRequest)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.PLCCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.PLCCanApproveRequest)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.BLCCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.BLCCanApproveRequest)</span>
                            </label>
                        </div>
                    </div>

                    <!-- Global Approvers Section -->
                    @Html.HiddenFor(m => m.TheReportingOrg.GlobalApprovers)

                    <div class="info-card" style="margin-top: 24px;">
                        <div class="info-card-left">
                            <span class="info-card-title">Global Approvers <span class="info-icon" title="Select here for if you want an associate to be able to approve access requests for any report aligned to this Reporting Org. The user must have Admin or Analyst permission on this Reporting Org.">ⓘ</span></span>
                        </div>
                    </div>

                    <div class="selector-row">
                        <label>Add Global Approver</label>
                        <div class="select-wrapper">
                            @Html.TextBoxFor(m => m.TheGlobalApprover.NBK, new { @class = "inline-input", @placeholder = "Enter NBK or Email..." })
                        </div>
                        @Html.TextBoxFor(m => m.TheGlobalApprover.Username, new { @class = "inline-input", @disabled = "", @placeholder = "Username" })
                        @Html.TextBoxFor(m => m.TheGlobalApprover.Email, new { @class = "inline-input", @disabled = "", @placeholder = "Email" })
                        <button type="button" id="btnAddGlobalApprover" class="btn btn-primary" title="Add as Global Approver">Add</button>
                    </div>

                    @if (Model.GlobalApprovers.Count > 0)
                    {
                        <table class="data-table" id="divGlobalApprover">
                            <thead>
                                <tr>
                                    <th>Username</th>
                                    <th>NBK</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                @foreach (var user in Model.GlobalApprovers)
                                {
                                    <tr>
                                        <td>@user.Username</td>
                                        <td>@user.NBK</td>
                                        <td>
                                            <div class="action-buttons">
                                                <button type="button" class="action-btn reject cgRemoveGlobalApprover" id="btnRemoveGlobalApprover_@user.NBK" title="Remove from Global Approvers">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                                        <path d="M6 18L18 6M6 6l12 12" />
                                                    </svg>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                }
                            </tbody>
                        </table>
                    }
                    else
                    {
                        <p style="color: #666; padding: 16px 0;">No one has been selected as Global Approver.</p>
                    }
                </div>

                <!-- Report Custom Fields Tab Content -->
                <div id="report-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paReportCustomFields ? "active" : "")">
                    <div class="selector-row">
                        <label>Reporting Organisation</label>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                                new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                                "(Please Select)",
                                new { @id = "reportCustomReportingOrgDropdown" })
                        </div>
                    </div>

                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Report Inventory Custom Fields</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                    </div>

                    <!-- Add New Report Field -->
                    <div class="details-grid" id="tblAddNewReportField">
                        <div class="detail-item" style="grid-column: span 2;">
                            <span class="detail-label">Display Label</span>
                            @Html.TextBoxFor(m => m.TheReportFieldDefinition.Label, new { @class = "inline-input", @maxlength = "30", @placeholder = "Display Label" })
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">Data Type</span>
                            <div class="select-wrapper" style="max-width: 100%;">
                                @Html.DropDownListFor(m => m.TheReportFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">Display Order</span>
                            @Html.TextBoxFor(m => m.TheReportFieldDefinition.DisplayOrder, new { @class = "inline-input", @maxlength = "3", @placeholder = "Order" })
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Mandatory)
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Mandatory)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Visible)
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Visible)</span>
                            </label>
                        </div>
                    </div>

                    <div class="selector-row">
                        <div class="select-wrapper" style="max-width: 100%; flex: 2;">
                            @Html.TextAreaFor(m => m.TheReportFieldDefinition.Tooltip, new { @class = "inline-input", @placeholder = "Tooltip", @rows = "2" })
                        </div>
                        <button type="button" id="btnAddReportCustomField" class="btn btn-primary">Add Field</button>
                    </div>

                    <div class="selector-row" id="row_ReportLookupValues">
                        <div class="select-wrapper" style="max-width: 100%;">
                            @Html.TextBoxFor(m => m.TheReportFieldDefinition.LookupValues, new { @class = "inline-input", @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)" })
                        </div>
                    </div>

                    @if (Model.ReportCustomFields.Count > 0)
                    {
                        <table class="data-table" id="tblReportCustomFields">
                            <thead>
                                <tr>
                                    <th>Label</th>
                                    <th>Data Type</th>
                                    <th>Mandatory</th>
                                    <th>Visible</th>
                                    <th>Display Order</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                @for (int i = 0; i < Model.ReportCustomFields.Count; i++)
                                {
                                    CustomField field = Model.ReportCustomFields[i];
                                    <tr>
                                        <td>
                                            @Html.HiddenFor(m => field.Name, new { @Name = "ReportCustomFields[" + i + "].Name" })
                                            @Html.TextBoxFor(m => field.Label, new { @class = "inline-input", @Name = "ReportCustomFields[" + i + "].Label", @maxlength = "30" })
                                        </td>
                                        <td>
                                            @field.DataType
                                            @Html.HiddenFor(m => field.DataType, new { @Name = "ReportCustomFields[" + i + "].DataType" })
                                        </td>
                                        <td>
                                            <label class="checkbox-label">
                                                @Html.CheckBoxFor(m => field.Mandatory, new { @Name = "ReportCustomFields[" + i + "].Mandatory" })
                                                <span class="checkbox-custom"></span>
                                            </label>
                                        </td>
                                        <td>
                                            <label class="checkbox-label">
                                                @Html.CheckBoxFor(m => field.Visible, new { @Name = "ReportCustomFields[" + i + "].Visible" })
                                                <span class="checkbox-custom"></span>
                                            </label>
                                        </td>
                                        <td>
                                            @Html.TextBoxFor(m => field.DisplayOrder, new { @class = "inline-input", @Name = "ReportCustomFields[" + i + "].DisplayOrder", @maxlength = "3", @style = "width: 60px;" })
                                        </td>
                                        <td>
                                            <div class="action-buttons">
                                                <button type="button" class="action-btn approve cgUpdateReportCustomField" id="btnUpdateReportCustomField_@field.Name" title="Update">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                                        <path d="M5 13l4 4L19 7" />
                                                    </svg>
                                                </button>
                                                <button type="button" class="action-btn reject cgRemoveReportCustomField" id="btnRemoveReportCustomField_@field.Name" title="Remove">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                                        <path d="M6 18L18 6M6 6l12 12" />
                                                    </svg>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="6">
                                            @Html.TextAreaFor(m => field.Tooltip, new { @class = "inline-input", @Name = "ReportCustomFields[" + i + "].Tooltip", @placeholder = "Tooltip", @rows = "1" })
                                        </td>
                                    </tr>
                                    if (field.DataType == "Lookup")
                                    {
                                        <tr>
                                            <td colspan="6">
                                                @Html.TextBoxFor(m => field.LookupValues, new { @class = "inline-input", @Name = "ReportCustomFields[" + i + "].LookupValues", @placeholder = "LookupValues" })
                                            </td>
                                        </tr>
                                    }
                                }
                            </tbody>
                        </table>
                    }
                    else
                    {
                        <p style="color: #666; padding: 16px 0;">No custom field is defined.</p>
                    }
                </div>

                <!-- Process Custom Fields Tab Content -->
                <div id="process-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paProcessCustomFields ? "active" : "")">
                    <div class="selector-row">
                        <label>Reporting Organisation</label>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                                new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                                "(Please Select)",
                                new { @id = "processCustomReportingOrgDropdown" })
                        </div>
                    </div>

                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Process Inventory Custom Fields</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                    </div>

                    <!-- Add New Process Field -->
                    <div class="details-grid">
                        <div class="detail-item" style="grid-column: span 2;">
                            <span class="detail-label">Display Label</span>
                            @Html.TextBoxFor(m => m.TheProcessFieldDefinition.Label, new { @class = "inline-input", @maxlength = "30", @placeholder = "Display Label" })
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">Data Type</span>
                            <div class="select-wrapper" style="max-width: 100%;">
                                @Html.DropDownListFor(m => m.TheProcessFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                            </div>
                        </div>

                        <div class="detail-item">
                            <span class="detail-label">Display Order</span>
                            @Html.TextBoxFor(m => m.TheProcessFieldDefinition.DisplayOrder, new { @class = "inline-input", @maxlength = "3", @placeholder = "Order" })
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Mandatory)
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Mandatory)</span>
                            </label>
                        </div>

                        <div class="detail-item">
                            <label class="checkbox-label">
                                @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Visible)
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Visible)</span>
                            </label>
                        </div>
                    </div>

                    <div class="selector-row">
                        <div class="select-wrapper" style="max-width: 100%; flex: 2;">
                            @Html.TextAreaFor(m => m.TheProcessFieldDefinition.Tooltip, new { @class = "inline-input", @placeholder = "Tooltip", @rows = "2" })
                        </div>
                        <button type="button" id="btnAddProcessCustomField" class="btn btn-primary">Add Field</button>
                    </div>

                    <div class="selector-row" id="row_ProcessLookupValues">
                        <div class="select-wrapper" style="max-width: 100%;">
                            @Html.TextBoxFor(m => m.TheProcessFieldDefinition.LookupValues, new { @class = "inline-input", @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)" })
                        </div>
                    </div>

                    @if (Model.ProcessCustomFields.Count > 0)
                    {
                        <table class="data-table" id="tblProcessCustomFields">
                            <thead>
                                <tr>
                                    <th>Label</th>
                                    <th>Data Type</th>
                                    <th>Mandatory</th>
                                    <th>Visible</th>
                                    <th>Display Order</th>
                                    <th>Actions</th>
                                </tr>
                            </thead>
                            <tbody>
                                @for (int i = 0; i < Model.ProcessCustomFields.Count; i++)
                                {
                                    CustomField field = Model.ProcessCustomFields[i];
                                    <tr>
                                        <td>
                                            @Html.HiddenFor(m => field.Name, new { @Name = "ProcessCustomFields[" + i + "].Name" })
                                            @Html.TextBoxFor(m => field.Label, new { @class = "inline-input", @Name = "ProcessCustomFields[" + i + "].Label", @maxlength = "30" })
                                        </td>
                                        <td>
                                            @field.DataType
                                            @Html.HiddenFor(m => field.DataType, new { @Name = "ProcessCustomFields[" + i + "].DataType" })
                                        </td>
                                        <td>
                                            <label class="checkbox-label">
                                                @Html.CheckBoxFor(m => field.Mandatory, new { @Name = "ProcessCustomFields[" + i + "].Mandatory" })
                                                <span class="checkbox-custom"></span>
                                            </label>
                                        </td>
                                        <td>
                                            <label class="checkbox-label">
                                                @Html.CheckBoxFor(m => field.Visible, new { @Name = "ProcessCustomFields[" + i + "].Visible" })
                                                <span class="checkbox-custom"></span>
                                            </label>
                                        </td>
                                        <td>
                                            @Html.TextBoxFor(m => field.DisplayOrder, new { @class = "inline-input", @Name = "ProcessCustomFields[" + i + "].DisplayOrder", @maxlength = "3", @style = "width: 60px;" })
                                        </td>
                                        <td>
                                            <div class="action-buttons">
                                                <button type="button" class="action-btn approve cgUpdateProcessCustomField" id="btnUpdateProcessCustomField_@field.Name" title="Update">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                                        <path d="M5 13l4 4L19 7" />
                                                    </svg>
                                                </button>
                                                <button type="button" class="action-btn reject cgRemoveProcessCustomField" id="btnRemoveProcessCustomField_@field.Name" title="Remove">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                                        <path d="M6 18L18 6M6 6l12 12" />
                                                    </svg>
                                                </button>
                                            </div>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td colspan="6">
                                            @Html.TextAreaFor(m => field.Tooltip, new { @class = "inline-input", @Name = "ProcessCustomFields[" + i + "].Tooltip", @placeholder = "Tooltip", @rows = "1" })
                                        </td>
                                    </tr>
                                    if (field.DataType == "Lookup")
                                    {
                                        <tr>
                                            <td colspan="6">
                                                @Html.TextBoxFor(m => field.LookupValues, new { @class = "inline-input", @Name = "ProcessCustomFields[" + i + "].LookupValues", @placeholder = "LookupValues" })
                                            </td>
                                        </tr>
                                    }
                                }
                            </tbody>
                        </table>
                    }
                    else
                    {
                        <p style="color: #666; padding: 16px 0;">No custom field is defined.</p>
                    }
                </div>
            }
            else
            {
                <!-- Show only Reporting Org dropdown when division is selected but org is not -->
                <div class="selector-row">
                    <label>Reporting Organisation</label>
                    <div class="select-wrapper">
                        @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                            new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                            "(Please Select)")
                    </div>
                </div>
            }
        }
    }
</div>

@section Scripts
{
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function () {
            // Tab switching
            $('.tab').on('click', function () {
                var tabId = $(this).attr('id');
                if (tabId && tabId.indexOf('btnSwitchTab_') === 0) {
                    var pageAction = tabId.replace('btnSwitchTab_', '');
                    $('#PageAction').val(pageAction);
                    $('#frmManageReportingOrg').submit();
                }

                // Visual tab switching for immediate feedback
                $('.tab').removeClass('active');
                $(this).addClass('active');
                $('.tab-content').removeClass('active');
                var tabName = $(this).data('tab');
                $('#' + tabName + '-content').addClass('active');
            });

            // Division dropdown change
            $('#SelectedDivisionID').on('change', function () {
                $('#frmManageReportingOrg').submit();
            });

            // Reporting Org dropdown change
            $('[id^="SelectedReportingOrgID"], #commReportingOrgDropdown, #reportCustomReportingOrgDropdown, #processCustomReportingOrgDropdown').on('change', function () {
                $('#frmManageReportingOrg').submit();
            });

            // Edit Details functionality
            const $editBtn = $('#edit-details-btn');
            const $saveBtn = $('#save-details-btn');
            const $cancelBtn = $('#cancel-details-btn');
            const $gridContainer = $('#details-grid-container');

            let originalValues = {};

            $editBtn.on('click', function () {
                $editBtn.addClass('hidden');
                $saveBtn.removeClass('hidden');
                $cancelBtn.removeClass('hidden');

                // Store original values and convert to inputs
                $gridContainer.find('.display-value').each(function () {
                    var $el = $(this);
                    var field = $el.data('field');
                    var value = $el.text().trim();
                    originalValues[field] = value;

                    // Create edit dropdown/input based on field type
                    var $wrapper = $('<div class="edit-select-wrapper"></div>');
                    var $select;

                    if (field === 'AutoPublishFlag' || field === 'ExpirationAlert' || field === 'AutomaticRetention') {
                        $select = $('<select><option value="Enabled">Enabled</option><option value="Disabled">Disabled</option></select>');
                        $select.val(value);
                    } else {
                        $select = $('<input type="text" value="' + value + '" />');
                    }

                    $wrapper.append($select);
                    $el.parent().find('.edit-select-wrapper').remove();
                    $el.parent().append($wrapper);
                    $el.hide();
                });
            });

            $saveBtn.on('click', function () {
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                $gridContainer.find('.display-value').each(function () {
                    var $el = $(this);
                    var $wrapper = $el.parent().find('.edit-select-wrapper');
                    if ($wrapper.length) {
                        var newValue = $wrapper.find('select, input').val();
                        $el.text(newValue);
                        $wrapper.remove();
                    }
                    $el.show();
                });

                // Trigger update via AJAX or form submit
                $('.cgUpdateReportingOrg').first().trigger('click');
            });

            $cancelBtn.on('click', function () {
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                $gridContainer.find('.display-value').each(function () {
                    var $el = $(this);
                    var field = $el.data('field');
                    $el.text(originalValues[field] || $el.text());
                    $el.parent().find('.edit-select-wrapper').remove();
                    $el.show();
                });
            });
        });
    </script>
    <script src="~/js2/manageReportingOrgs.js"></script>
}
