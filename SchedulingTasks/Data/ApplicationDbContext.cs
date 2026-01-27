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

@* ============================================
   NEW/MODIFIED CSS CLASSES - ADD TO GLOBAL CSS
   ============================================ *@
<style>
    /* Aligned Selector Row - ensures label and select align properly */
    .selector-row-aligned {
        display: flex;
        align-items: center;
        gap: 16px;
        margin-bottom: 16px;
    }

    .selector-row-aligned label {
        font-size: 14px;
        color: #666;
        font-weight: 500;
        min-width: 120px;
        /* Fixed width for alignment */
    }

    .selector-row-aligned .select-wrapper {
        flex: 1;
        max-width: 600px;
    }

    /* Editable Field Row - horizontal layout with edit button */
    .editable-field-row {
        display: flex;
        align-items: center;
        gap: 16px;
        padding: 12px 0;
        border-bottom: 1px solid #f0f0f0;
    }

    .editable-field-row:last-child {
        border-bottom: none;
    }

    .editable-field-label {
        font-size: 14px;
        color: #333;
        font-weight: 600;
        min-width: 200px;
        flex-shrink: 0;
    }

    .editable-field-input {
        flex: 1;
        max-width: 600px;
    }

    .editable-field-input input[type="text"] {
        width: 100%;
        padding: 10px 14px;
        font-size: 14px;
        border: 1px solid #ddd;
        border-radius: 4px;
        background: #fff;
        color: #333;
    }

    .editable-field-input input[type="text"]:focus {
        outline: none;
        border-color: #0049ac;
        box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.15);
    }

    .editable-field-input input[type="text"]:disabled {
        background: #f8f9fa;
        border-color: #e5e7eb;
        color: #333;
    }

    .editable-field-action {
        flex-shrink: 0;
    }

    .btn-update {
        width: 40px;
        height: 40px;
        border-radius: 4px;
        border: none;
        background: #4a7c59;
        color: #fff;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s;
    }

    .btn-update:hover {
        background: #3d6a4a;
    }

    .btn-update svg {
        width: 18px;
        height: 18px;
        stroke-width: 3;
    }

    /* Checkbox Field Row */
    .checkbox-field-row {
        display: flex;
        align-items: flex-start;
        gap: 12px;
        padding: 14px 0;
        border-bottom: 1px solid #f0f0f0;
    }

    .checkbox-field-row:last-child {
        border-bottom: none;
    }

    .checkbox-field-label {
        display: flex;
        align-items: center;
        gap: 10px;
        min-width: 250px;
        flex-shrink: 0;
        font-size: 14px;
        font-weight: 600;
        color: #333;
        cursor: pointer;
    }

    .checkbox-field-label input[type="checkbox"] {
        width: 18px;
        height: 18px;
        accent-color: #0049ac;
        cursor: pointer;
    }

    .checkbox-field-description {
        font-size: 14px;
        color: #666;
        line-height: 1.5;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .checkbox-field-description .warning-icon {
        width: 20px;
        height: 20px;
        flex-shrink: 0;
    }

    /* Section Divider */
    .section-divider {
        border: none;
        border-top: 1px solid #ddd;
        margin: 24px 0;
    }

    /* Fields Container */
    .fields-container {
        background: #fff;
        padding: 0 20px;
    }

    /* Retention Section */
    .retention-section {
        padding: 0 20px;
        margin-top: 8px;
    }
</style>

@{
    AppUser _curUser = PageData["CurrentUser"] as AppUser;
}

<div class="container">
    @using (Html.BeginForm("ManageReportingOrg", "Division", FormMethod.Post, new { id = "frmManageReportingOrg" }))
    {
        @Html.AntiForgeryToken()

        <h1>Reporting Organisation</h1>

        <!-- Division Selector - Aligned -->
        <div class="selector-row-aligned">
            <label>Division:</label>
            <div class="select-wrapper">
                @Html.DropDownListFor(m => m.SelectedDivisionID,
                    new SelectList(Model.Divisions, "ID", "CodeName"),
                    "(Please Select)")
            </div>
        </div>

        <!-- Reporting Org Selector - Aligned (always visible when division selected) -->
        @if (Model.SelectedDivisionID > 0)
        {
            <div class="selector-row-aligned">
                <label>Reporting Org:</label>
                <div class="select-wrapper">
                    @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                        new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                        "(Please Select)")
                </div>
            </div>

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
                    </div>

                    <!-- Editable Text Fields Section -->
                    <div class="fields-container">
                        <!-- Name Field -->
                        <div class="editable-field-row">
                            <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.Name)</label>
                            <div class="editable-field-input">
                                @Html.TextBoxFor(m => m.TheReportingOrg.Name)
                            </div>
                            <div class="editable-field-action">
                                <button type="button" class="btn-update cgUpdateReportingOrg" title="Update Reporting Org. Name">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                        <path d="M5 13l4 4L19 7" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <!-- Storage Path Field -->
                        @if (!_curUser.IsTechAdmin)
                        {
                            <div class="editable-field-row">
                                <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.StoragePath)</label>
                                <div class="editable-field-input">
                                    <input type="text" value="@repOrg.StoragePath" disabled />
                                </div>
                            </div>

                            <div class="editable-field-row">
                                <label class="editable-field-label">Auto Publishing</label>
                                <div class="editable-field-input">
                                    <input type="text" value="@(repOrg.AutoPublishFlag == true ? "Active" : "Inactive") - @repOrg.AutoPublishSource" disabled />
                                </div>
                            </div>
                        }
                        else
                        {
                            <div class="editable-field-row">
                                <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.StoragePath)</label>
                                <div class="editable-field-input">
                                    @Html.TextBoxFor(m => m.TheReportingOrg.StoragePath, new { @id = "storagePath" })
                                </div>
                                <div class="editable-field-action">
                                    <button type="button" class="btn-update cgUpdateReportingOrgStoragePath" title="Update Reporting Org. Storage Path">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                            <path d="M5 13l4 4L19 7" />
                                        </svg>
                                    </button>
                                </div>
                            </div>

                            <div class="editable-field-row">
                                <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishSource)</label>
                                <div class="editable-field-input">
                                    @Html.TextBoxFor(m => m.TheReportingOrg.AutoPublishSource, new { @id = "autoPublishSource" })
                                </div>
                                <div class="editable-field-action">
                                    <button type="button" class="btn-update cgUpdateReportingOrgAutoPublishSource" title="Update Reporting Org. Auto Publish Source">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                            <path d="M5 13l4 4L19 7" />
                                        </svg>
                                    </button>
                                </div>
                            </div>
                        }

                        <!-- AutoPublishFlag Checkbox -->
                        <div class="checkbox-field-row">
                            <label class="checkbox-field-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.AutoPublishFlag, new { @class = "cgUpdateReportingOrgAutoPublishFlag", @id = "TheReportingOrg_AutoPublishFlag" })
                                @Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishFlag)
                            </label>
                            <span class="checkbox-field-description">
                                Check to activate this publisher, uncheck to inactivate
                            </span>
                        </div>
                    </div>

                    <hr class="section-divider" />

                    <!-- Retention Settings Section -->
                    <div class="retention-section">
                        <!-- Report Expiration Alert Checkbox -->
                        <div class="checkbox-field-row">
                            <label class="checkbox-field-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.ExpirationAlert, new { @class = "cgUpdateReportingOrg" })
                                @Html.DisplayNameFor(m => m.TheReportingOrg.ExpirationAlert)
                            </label>
                            <span class="checkbox-field-description">
                                Select this option if you wish to receive monthly email about expiring reports.
                            </span>
                        </div>

                        <!-- Automatic Retention Checkbox -->
                        <div class="checkbox-field-row">
                            <label class="checkbox-field-label">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.AutomaticRetention, new { @class = "cgUpdateReportingOrg csGetConfirmation" })
                                @Html.DisplayNameFor(m => m.TheReportingOrg.AutomaticRetention)
                            </label>
                            <span class="checkbox-field-description">
                                <svg class="warning-icon" viewBox="0 0 24 24" fill="none" stroke="#c53030" stroke-width="2">
                                    <path d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                </svg>
                                Selecting this option enables automatic retirement for your reports.
                            </span>
                        </div>
                    </div>
                </div>

                <!-- Communication Tab Content -->
                <div id="communication-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paCommunication ? "active" : "")">
                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Communication Settings</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                    </div>

                    <div class="fields-container">
                        <!-- Support Email Field -->
                        <div class="editable-field-row">
                            <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportEmail)</label>
                            <div class="editable-field-input">
                                @Html.TextBoxFor(m => m.TheReportingOrg.SupportEmail)
                            </div>
                            <div class="editable-field-action">
                                <button type="button" class="btn-update cgUpdateReportingOrg" title="Update">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                        <path d="M5 13l4 4L19 7" />
                                    </svg>
                                </button>
                            </div>
                        </div>

                        <!-- Support Site Field -->
                        <div class="editable-field-row">
                            <label class="editable-field-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportSite)</label>
                            <div class="editable-field-input">
                                @Html.TextBoxFor(m => m.TheReportingOrg.SupportSite)
                            </div>
                            <div class="editable-field-action">
                                <button type="button" class="btn-update cgUpdateReportingOrg" title="Update">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                        <path d="M5 13l4 4L19 7" />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>

                    <hr class="section-divider" />

                    <!-- Report Access Approvers Section -->
                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Report Access Approvers</span>
                        </div>
                    </div>

                    <div class="fields-container">
                        <div class="details-grid" style="padding: 16px 0;">
                            <div class="detail-item">
                                <label class="checkbox-label">
                                    @Html.CheckBoxFor(m => m.TheReportingOrg.PACanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.PACanApproveRequest)</span>
                                </label>
                                <span class="info-icon" title="Select here for if you want the Primary Analyst aligned to the report to be able to approve access">ⓘ</span>
                            </div>

                            <div class="detail-item">
                                <label class="checkbox-label">
                                    @Html.CheckBoxFor(m => m.TheReportingOrg.BACanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.BACanApproveRequest)</span>
                                </label>
                                <span class="info-icon" title="Select here for if you want the Backup Analyst aligned to the report to be able to approve access">ⓘ</span>
                            </div>

                            <div class="detail-item">
                                <label class="checkbox-label">
                                    @Html.CheckBoxFor(m => m.TheReportingOrg.RTMCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.RTMCanApproveRequest)</span>
                                </label>
                                <span class="info-icon" title="Select here for if you want the Reporting Team Manager aligned to the report to be able to approve access">ⓘ</span>
                            </div>

                            <div class="detail-item">
                                <label class="checkbox-label">
                                    @Html.CheckBoxFor(m => m.TheReportingOrg.PLCCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.PLCCanApproveRequest)</span>
                                </label>
                                <span class="info-icon" title="Select here for if you want the Primary LOB Contact/Owner aligned to the report to be able to approve access">ⓘ</span>
                            </div>

                            <div class="detail-item">
                                <label class="checkbox-label">
                                    @Html.CheckBoxFor(m => m.TheReportingOrg.BLCCanApproveRequest, new { @class = "cgUpdateReportingOrg" })
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportingOrg.BLCCanApproveRequest)</span>
                                </label>
                                <span class="info-icon" title="Select here for if you want the Backup LOB Contact/Owner aligned to the report to be able to approve access">ⓘ</span>
                            </div>
                        </div>
                    </div>

                    <hr class="section-divider" />

                    <!-- Global Approvers Section -->
                    @Html.HiddenFor(m => m.TheReportingOrg.GlobalApprovers)

                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Global Approvers</span>
                            <span class="info-icon" title="Select here for if you want an associate to be able to approve access requests for any report aligned to this Reporting Org. The user must have Admin or Analyst permission on this Reporting Org.">ⓘ</span>
                        </div>
                    </div>

                    <div class="fields-container">
                        <div class="editable-field-row">
                            <label class="editable-field-label">Add Global Approver</label>
                            <div class="editable-field-input" style="display: flex; gap: 12px;">
                                @Html.TextBoxFor(m => m.TheGlobalApprover.NBK, new { @placeholder = "Enter NBK or Email...", @style = "flex: 1;" })
                                @Html.TextBoxFor(m => m.TheGlobalApprover.Username, new { @disabled = "", @placeholder = "Username", @style = "flex: 1;" })
                                @Html.TextBoxFor(m => m.TheGlobalApprover.Email, new { @disabled = "", @placeholder = "Email", @style = "flex: 1;" })
                            </div>
                            <div class="editable-field-action">
                                <button type="button" id="btnAddGlobalApprover" class="btn-update" title="Add as Global Approver" style="background: #1e3a5f;">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor">
                                        <path d="M12 5v14m-7-7h14" />
                                    </svg>
                                </button>
                            </div>
                        </div>
                    </div>

                    @if (Model.GlobalApprovers.Count > 0)
                    {
                        <table class="data-table" id="divGlobalApprover" style="margin-top: 16px;">
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
                        <p style="color: #666; padding: 16px 20px;">No one has been selected as Global Approver.</p>
                    }
                </div>

                <!-- Report Custom Fields Tab Content -->
                <div id="report-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paReportCustomFields ? "active" : "")">
                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Report Inventory Custom Fields</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                    </div>

                    <!-- Add New Report Field -->
                    <div class="fields-container">
                        <div class="editable-field-row">
                            <label class="editable-field-label">Field Definition</label>
                            <div class="editable-field-input" style="display: flex; gap: 12px; align-items: center;">
                                @Html.TextBoxFor(m => m.TheReportFieldDefinition.Label, new { @maxlength = "30", @placeholder = "Display Label", @style = "flex: 2;" })
                                <div class="select-wrapper" style="flex: 1; max-width: none;">
                                    @Html.DropDownListFor(m => m.TheReportFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                                </div>
                                @Html.TextBoxFor(m => m.TheReportFieldDefinition.DisplayOrder, new { @maxlength = "3", @placeholder = "Order", @style = "width: 80px; flex: none;" })
                            </div>
                        </div>

                        <div class="editable-field-row">
                            <label class="editable-field-label"></label>
                            <div class="editable-field-input" style="display: flex; gap: 24px; align-items: center;">
                                <label class="checkbox-label" style="min-width: auto;">
                                    @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Mandatory)
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Mandatory)</span>
                                </label>
                                <label class="checkbox-label" style="min-width: auto;">
                                    @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Visible)
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Visible)</span>
                                </label>
                            </div>
                        </div>

                        <div class="editable-field-row">
                            <label class="editable-field-label">Tooltip</label>
                            <div class="editable-field-input">
                                @Html.TextAreaFor(m => m.TheReportFieldDefinition.Tooltip, new { @placeholder = "Tooltip", @rows = "2", @style = "width: 100%; padding: 10px 14px; font-size: 14px; border: 1px solid #ddd; border-radius: 4px; resize: vertical;" })
                            </div>
                            <div class="editable-field-action">
                                <button type="button" id="btnAddReportCustomField" class="btn btn-primary">Add Field</button>
                            </div>
                        </div>

                        <div class="editable-field-row" id="row_ReportLookupValues">
                            <label class="editable-field-label">Lookup Values</label>
                            <div class="editable-field-input">
                                @Html.TextBoxFor(m => m.TheReportFieldDefinition.LookupValues, new { @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)" })
                            </div>
                        </div>
                    </div>

                    @if (Model.ReportCustomFields.Count > 0)
                    {
                        <table class="data-table" id="tblReportCustomFields" style="margin-top: 24px;">
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
                        <p style="color: #666; padding: 16px 20px;">No custom field is defined.</p>
                    }
                </div>

                <!-- Process Custom Fields Tab Content -->
                <div id="process-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paProcessCustomFields ? "active" : "")">
                    <div class="info-card">
                        <div class="info-card-left">
                            <span class="info-card-title">Process Inventory Custom Fields</span>
                            <span class="status-badge status-active">Active</span>
                        </div>
                    </div>

                    <!-- Add New Process Field -->
                    <div class="fields-container">
                        <div class="editable-field-row">
                            <label class="editable-field-label">Field Definition</label>
                            <div class="editable-field-input" style="display: flex; gap: 12px; align-items: center;">
                                @Html.TextBoxFor(m => m.TheProcessFieldDefinition.Label, new { @maxlength = "30", @placeholder = "Display Label", @style = "flex: 2;" })
                                <div class="select-wrapper" style="flex: 1; max-width: none;">
                                    @Html.DropDownListFor(m => m.TheProcessFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                                </div>
                                @Html.TextBoxFor(m => m.TheProcessFieldDefinition.DisplayOrder, new { @maxlength = "3", @placeholder = "Order", @style = "width: 80px; flex: none;" })
                            </div>
                        </div>

                        <div class="editable-field-row">
                            <label class="editable-field-label"></label>
                            <div class="editable-field-input" style="display: flex; gap: 24px; align-items: center;">
                                <label class="checkbox-label" style="min-width: auto;">
                                    @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Mandatory)
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Mandatory)</span>
                                </label>
                                <label class="checkbox-label" style="min-width: auto;">
                                    @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Visible)
                                    <span class="checkbox-custom"></span>
                                    <span class="checkbox-text">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Visible)</span>
                                </label>
                            </div>
                        </div>

                        <div class="editable-field-row">
                            <label class="editable-field-label">Tooltip</label>
                            <div class="editable-field-input">
                                @Html.TextAreaFor(m => m.TheProcessFieldDefinition.Tooltip, new { @placeholder = "Tooltip", @rows = "2", @style = "width: 100%; padding: 10px 14px; font-size: 14px; border: 1px solid #ddd; border-radius: 4px; resize: vertical;" })
                            </div>
                            <div class="editable-field-action">
                                <button type="button" id="btnAddProcessCustomField" class="btn btn-primary">Add Field</button>
                            </div>
                        </div>

                        <div class="editable-field-row" id="row_ProcessLookupValues">
                            <label class="editable-field-label">Lookup Values</label>
                            <div class="editable-field-input">
                                @Html.TextBoxFor(m => m.TheProcessFieldDefinition.LookupValues, new { @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)" })
                            </div>
                        </div>
                    </div>

                    @if (Model.ProcessCustomFields.Count > 0)
                    {
                        <table class="data-table" id="tblProcessCustomFields" style="margin-top: 24px;">
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
                        <p style="color: #666; padding: 16px 20px;">No custom field is defined.</p>
                    }
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
            $('#SelectedReportingOrgID').on('change', function () {
                $('#frmManageReportingOrg').submit();
            });
        });
    </script>
    <script src="~/js2/manageReportingOrgs.js"></script>
}
