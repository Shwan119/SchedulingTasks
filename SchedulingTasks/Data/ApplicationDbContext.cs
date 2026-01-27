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
   PAGE SPECIFIC STYLES - ADD TO GLOBAL CSS
   ============================================ *@
<style>
    /* ============================================
       PAGE SPECIFIC STYLES (Inline Editing)
       ============================================ */

    .header-actions {
        display: none;
    }

    /* Update Selector Row Layout for Alignment */
    .top-selectors {
        display: flex;
        flex-direction: column;
        gap: 16px;
        margin-bottom: 24px;
        max-width: 600px;
    }

    .top-selectors .selector-row {
        display: grid;
        grid-template-columns: 180px 1fr auto;
        align-items: center;
        gap: 16px;
        margin-bottom: 0;
    }

    .top-selectors .selector-row label {
        font-size: 14px;
        color: #666;
        font-weight: 500;
    }

    .top-selectors .select-wrapper {
        position: relative;
        width: 100%;
    }

    /* 3 Column Layout */
    .details-grid {
        display: grid;
        grid-template-columns: repeat(3, 1fr);
        gap: 24px;
        padding: 24px 0;
    }

    .detail-item {
        display: flex;
        flex-direction: column;
        gap: 8px;
        position: relative;
    }

    .detail-label {
        font-size: 13px;
        color: #555;
        font-weight: 600;
    }

    /* Value Box - Looks like an input but displays text */
    .detail-value-box {
        position: relative;
        background: #fff;
        border: 1px solid #d1d5db;
        border-radius: 4px;
        height: 42px;
        display: flex;
        align-items: center;
        padding: 0 40px 0 12px;
        transition: border-color 0.2s, box-shadow 0.2s;
    }

    .detail-value-box:hover {
        border-color: #999;
    }

    .detail-value {
        font-size: 14px;
        color: #333;
        font-weight: 400;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        width: 100%;
    }

    /* Edit Icon - Positioned at the Edge */
    .edit-icon-btn {
        position: absolute;
        right: 8px;
        top: 50%;
        transform: translateY(-50%);
        background: none;
        border: none;
        cursor: pointer;
        color: #9ca3af;
        padding: 4px;
        display: flex;
        align-items: center;
        justify-content: center;
        border-radius: 4px;
        transition: all 0.2s;
    }

    .edit-icon-btn:hover {
        color: #0049ac;
        background-color: #f0f7ff;
    }

    .edit-icon-btn svg {
        width: 16px;
        height: 16px;
    }

    /* Edit Mode Container - Overlays the display box */
    .edit-mode-container {
        display: none;
        flex-direction: row;
        align-items: center;
        gap: 4px;
        width: 100%;
        height: 42px;
    }

    .edit-mode-container.active {
        display: flex;
    }

    /* Inputs in Edit Mode */
    .field-input {
        flex: 1;
        height: 42px;
        padding: 8px 12px;
        font-size: 14px;
        border: 1px solid #0049ac;
        border-radius: 4px;
        outline: none;
        width: 100%;
        min-width: 0;
        background: #fff;
    }

    /* Checkbox Styling - Standard Look */
    .checkbox-display-container {
        display: flex;
        flex-direction: column;
        justify-content: flex-start;
        min-height: 42px;
        padding: 0;
    }

    .checkbox-row {
        display: flex;
        align-items: center;
        gap: 10px;
        margin-bottom: 6px;
    }

    /* Custom Checkbox */
    .standard-checkbox {
        appearance: none;
        -webkit-appearance: none;
        width: 20px;
        height: 20px;
        border: 2px solid #6b7280;
        border-radius: 4px;
        background-color: #fff;
        cursor: pointer;
        position: relative;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s;
        flex-shrink: 0;
    }

    .standard-checkbox:checked {
        background-color: #0049ac;
        border-color: #0049ac;
    }

    /* Checkmark */
    .standard-checkbox:checked::after {
        content: '';
        width: 5px;
        height: 10px;
        border: solid white;
        border-width: 0 2px 2px 0;
        transform: rotate(45deg);
        margin-bottom: 2px;
    }

    .checkbox-status-label {
        font-size: 14px;
        font-weight: 600;
        color: #333;
    }

    /* Helper Text for Checkboxes */
    .checkbox-helper-text {
        font-size: 12px;
        color: #666;
        line-height: 1.4;
    }

    /* Action Buttons (Save/Cancel) next to input */
    .mini-btn {
        width: 36px;
        height: 36px;
        border-radius: 4px;
        border: 1px solid transparent;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        transition: all 0.2s;
        background: #fff;
    }

    .mini-btn-save {
        color: #0049ac;
        border-color: #b3c7e6;
        background: #f0f7ff;
    }

    .mini-btn-save:hover {
        background: #dcebff;
    }

    .mini-btn-cancel {
        color: #c62828;
        border-color: #f5c6cb;
        background: #fff5f5;
    }

    .mini-btn-cancel:hover {
        background: #ffebee;
    }

    .mini-btn svg {
        width: 18px;
        height: 18px;
        stroke-width: 2.5;
    }

    .hidden {
        display: none !important;
    }

    /* Responsive */
    @media (max-width: 1200px) {
        .details-grid {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    @media (max-width: 768px) {
        .details-grid {
            grid-template-columns: 1fr;
        }

        .top-selectors .selector-row {
            grid-template-columns: 1fr;
            gap: 8px;
        }

        .top-selectors {
            max-width: 100%;
        }
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

        <div class="top-selectors">
            <!-- Division Selector -->
            <div class="selector-row">
                <label>Division</label>
                <div class="select-wrapper">
                    @Html.DropDownListFor(m => m.SelectedDivisionID,
                        new SelectList(Model.Divisions, "ID", "CodeName"),
                        "(Please Select)")
                </div>
                <div></div>
            </div>

            <!-- Reporting Organisation Selector -->
            @if (Model.SelectedDivisionID > 0)
            {
                <div class="selector-row">
                    <label>Reporting Organisation</label>
                    <div class="select-wrapper">
                        @Html.DropDownListFor(m => m.SelectedReportingOrgID,
                            new SelectList(Model.ReportingOrgs, "ID", "CodeName"),
                            "(Please Select)")
                    </div>
                    <div></div>
                </div>
            }
        </div>

        @if (Model.SelectedDivisionID > 0 && Model.SelectedReportingOrgID > 0)
        {
            var repOrg = Model.TheReportingOrg;

            @Html.HiddenFor(m => m.PageAction)
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

            <!-- ==================== GENERAL TAB ==================== -->
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

                <!-- Details Grid -->
                <div class="details-grid" id="details-grid-container">

                    <!-- ROW 1: Text Fields -->

                    <!-- 1. Name Field -->
                    <div class="detail-item" data-type="text" data-field="Name">
                        <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.Name)</span>
                        <!-- Display View -->
                        <div class="detail-value-box">
                            <span class="detail-value">@repOrg.Name</span>
                            <button type="button" class="edit-icon-btn" title="Edit">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                            </button>
                        </div>
                        <!-- Edit View -->
                        <div class="edit-mode-container">
                            @Html.TextBoxFor(m => m.TheReportingOrg.Name, new { @class = "field-input" })
                            <button type="button" class="mini-btn mini-btn-save cgUpdateReportingOrg" title="Save">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                            </button>
                            <button type="button" class="mini-btn mini-btn-cancel" title="Cancel">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12" /></svg>
                            </button>
                        </div>
                    </div>

                    <!-- 2. Storage Path Field -->
                    <div class="detail-item" data-type="text" data-field="StoragePath">
                        <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.StoragePath)</span>
                        <!-- Display View -->
                        <div class="detail-value-box">
                            <span class="detail-value">@repOrg.StoragePath</span>
                            @if (_curUser.IsTechAdmin)
                            {
                                <button type="button" class="edit-icon-btn" title="Edit">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                </button>
                            }
                        </div>
                        <!-- Edit View (Tech Admin Only) -->
                        @if (_curUser.IsTechAdmin)
                        {
                            <div class="edit-mode-container">
                                @Html.TextBoxFor(m => m.TheReportingOrg.StoragePath, new { @class = "field-input", @id = "storagePath" })
                                <button type="button" class="mini-btn mini-btn-save cgUpdateReportingOrgStoragePath" title="Save">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                                </button>
                                <button type="button" class="mini-btn mini-btn-cancel" title="Cancel">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12" /></svg>
                                </button>
                            </div>
                        }
                    </div>

                    <!-- 3. Auto Publish Source Path Field -->
                    <div class="detail-item" data-type="text" data-field="AutoPublishSource">
                        <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishSource)</span>
                        <!-- Display View -->
                        <div class="detail-value-box">
                            <span class="detail-value">@repOrg.AutoPublishSource</span>
                            @if (_curUser.IsTechAdmin)
                            {
                                <button type="button" class="edit-icon-btn" title="Edit">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                </button>
                            }
                        </div>
                        <!-- Edit View (Tech Admin Only) -->
                        @if (_curUser.IsTechAdmin)
                        {
                            <div class="edit-mode-container">
                                @Html.TextBoxFor(m => m.TheReportingOrg.AutoPublishSource, new { @class = "field-input", @id = "autoPublishSource" })
                                <button type="button" class="mini-btn mini-btn-save cgUpdateReportingOrgAutoPublishSource" title="Save">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                                </button>
                                <button type="button" class="mini-btn mini-btn-cancel" title="Cancel">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12" /></svg>
                                </button>
                            </div>
                        }
                    </div>

                    <!-- ROW 2: Checkboxes (Always Visible, No Edit Mode) -->

                    <!-- 4. Auto Publish Flag Checkbox -->
                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.AutoPublishFlag, new { @class = "standard-checkbox cgUpdateReportingOrgAutoPublishFlag", @id = "TheReportingOrg_AutoPublishFlag" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutoPublishFlag)</span>
                            </div>
                            <div class="checkbox-helper-text">Check to activate this publisher, uncheck to inactivate</div>
                        </div>
                    </div>

                    <!-- 5. Report Expiration Alert Checkbox -->
                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.ExpirationAlert, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.ExpirationAlert)</span>
                            </div>
                            <div class="checkbox-helper-text">Select this option if you wish to receive monthly email about expiring reports</div>
                        </div>
                    </div>

                    <!-- 6. Automatic Retention Checkbox -->
                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.AutomaticRetention, new { @class = "standard-checkbox cgUpdateReportingOrg csGetConfirmation" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.AutomaticRetention)</span>
                            </div>
                            <div class="checkbox-helper-text">⚠️ Selecting this option enables automatic retirement for your reports</div>
                        </div>
                    </div>

                </div>
            </div>

            <!-- ==================== COMMUNICATION TAB ==================== -->
            <div id="communication-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paCommunication ? "active" : "")">

                <div class="info-card">
                    <div class="info-card-left">
                        <span class="info-card-title">Communication Settings</span>
                        <span class="status-badge status-active">Active</span>
                    </div>
                </div>

                <div class="details-grid">
                    <!-- Support Email Field -->
                    <div class="detail-item" data-type="text" data-field="SupportEmail">
                        <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportEmail)</span>
                        <div class="detail-value-box">
                            <span class="detail-value">@repOrg.SupportEmail</span>
                            <button type="button" class="edit-icon-btn" title="Edit">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                            </button>
                        </div>
                        <div class="edit-mode-container">
                            @Html.TextBoxFor(m => m.TheReportingOrg.SupportEmail, new { @class = "field-input" })
                            <button type="button" class="mini-btn mini-btn-save cgUpdateReportingOrg" title="Save">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                            </button>
                            <button type="button" class="mini-btn mini-btn-cancel" title="Cancel">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12" /></svg>
                            </button>
                        </div>
                    </div>

                    <!-- Support Site Field -->
                    <div class="detail-item" data-type="text" data-field="SupportSite">
                        <span class="detail-label">@Html.DisplayNameFor(m => m.TheReportingOrg.SupportSite)</span>
                        <div class="detail-value-box">
                            <span class="detail-value">@repOrg.SupportSite</span>
                            <button type="button" class="edit-icon-btn" title="Edit">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                            </button>
                        </div>
                        <div class="edit-mode-container">
                            @Html.TextBoxFor(m => m.TheReportingOrg.SupportSite, new { @class = "field-input" })
                            <button type="button" class="mini-btn mini-btn-save cgUpdateReportingOrg" title="Save">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                            </button>
                            <button type="button" class="mini-btn mini-btn-cancel" title="Cancel">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12" /></svg>
                            </button>
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
                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.PACanApproveRequest, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.PACanApproveRequest)</span>
                            </div>
                            <div class="checkbox-helper-text">Primary Analyst can approve access requests</div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.BACanApproveRequest, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.BACanApproveRequest)</span>
                            </div>
                            <div class="checkbox-helper-text">Backup Analyst can approve access requests</div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.RTMCanApproveRequest, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.RTMCanApproveRequest)</span>
                            </div>
                            <div class="checkbox-helper-text">Reporting Team Manager can approve access requests</div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.PLCCanApproveRequest, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.PLCCanApproveRequest)</span>
                            </div>
                            <div class="checkbox-helper-text">Primary LOB Contact/Owner can approve access requests</div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportingOrg.BLCCanApproveRequest, new { @class = "standard-checkbox cgUpdateReportingOrg" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportingOrg.BLCCanApproveRequest)</span>
                            </div>
                            <div class="checkbox-helper-text">Backup LOB Contact/Owner can approve access requests</div>
                        </div>
                    </div>
                </div>

                <!-- Global Approvers Section -->
                @Html.HiddenFor(m => m.TheReportingOrg.GlobalApprovers)

                <div class="info-card" style="margin-top: 24px;">
                    <div class="info-card-left">
                        <span class="info-card-title">Global Approvers</span>
                    </div>
                </div>

                <div class="details-grid">
                    <div class="detail-item" data-type="text" style="grid-column: span 3;">
                        <span class="detail-label">Add Global Approver</span>
                        <div style="display: flex; gap: 12px; align-items: center;">
                            @Html.TextBoxFor(m => m.TheGlobalApprover.NBK, new { @class = "field-input", @placeholder = "Enter NBK or Email...", @style = "flex: 1; border-color: #d1d5db;" })
                            @Html.TextBoxFor(m => m.TheGlobalApprover.Username, new { @class = "field-input", @disabled = "", @placeholder = "Username", @style = "flex: 1; border-color: #d1d5db;" })
                            @Html.TextBoxFor(m => m.TheGlobalApprover.Email, new { @class = "field-input", @disabled = "", @placeholder = "Email", @style = "flex: 1; border-color: #d1d5db;" })
                            <button type="button" id="btnAddGlobalApprover" class="btn btn-primary" title="Add as Global Approver">Add</button>
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
                    <p style="color: #666; padding: 16px 0;">No one has been selected as Global Approver.</p>
                }
            </div>

            <!-- ==================== REPORT CUSTOM FIELDS TAB ==================== -->
            <div id="report-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paReportCustomFields ? "active" : "")">

                <div class="info-card">
                    <div class="info-card-left">
                        <span class="info-card-title">Report Inventory Custom Fields</span>
                        <span class="status-badge status-active">Active</span>
                    </div>
                </div>

                <!-- Add New Report Field Form -->
                <div class="details-grid" id="tblAddNewReportField">
                    <div class="detail-item" data-type="text" style="grid-column: span 2;">
                        <span class="detail-label">Display Label</span>
                        @Html.TextBoxFor(m => m.TheReportFieldDefinition.Label, new { @class = "field-input", @maxlength = "30", @placeholder = "Display Label", @style = "border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" data-type="select">
                        <span class="detail-label">Data Type</span>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.TheReportFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                        </div>
                    </div>

                    <div class="detail-item" data-type="text">
                        <span class="detail-label">Display Order</span>
                        @Html.TextBoxFor(m => m.TheReportFieldDefinition.DisplayOrder, new { @class = "field-input", @maxlength = "3", @placeholder = "Order", @style = "border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Mandatory, new { @class = "standard-checkbox" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Mandatory)</span>
                            </div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheReportFieldDefinition.Visible, new { @class = "standard-checkbox" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheReportFieldDefinition.Visible)</span>
                            </div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="text" style="grid-column: span 2;">
                        <span class="detail-label">Tooltip</span>
                        @Html.TextAreaFor(m => m.TheReportFieldDefinition.Tooltip, new { @class = "field-input", @placeholder = "Tooltip", @rows = "2", @style = "height: auto; border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" style="display: flex; align-items: flex-end;">
                        <button type="button" id="btnAddReportCustomField" class="btn btn-primary">Add Field</button>
                    </div>

                    <div class="detail-item" data-type="text" style="grid-column: span 3;" id="row_ReportLookupValues">
                        <span class="detail-label">Lookup Values</span>
                        @Html.TextBoxFor(m => m.TheReportFieldDefinition.LookupValues, new { @class = "field-input", @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)", @style = "border-color: #d1d5db;" })
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
                                        @Html.CheckBoxFor(m => field.Mandatory, new { @class = "standard-checkbox", @Name = "ReportCustomFields[" + i + "].Mandatory" })
                                    </td>
                                    <td>
                                        @Html.CheckBoxFor(m => field.Visible, new { @class = "standard-checkbox", @Name = "ReportCustomFields[" + i + "].Visible" })
                                    </td>
                                    <td>
                                        @Html.TextBoxFor(m => field.DisplayOrder, new { @class = "inline-input", @Name = "ReportCustomFields[" + i + "].DisplayOrder", @maxlength = "3", @style = "width: 60px;" })
                                    </td>
                                    <td>
                                        <div class="action-buttons">
                                            <button type="button" class="action-btn approve cgUpdateReportCustomField" id="btnUpdateReportCustomField_@field.Name" title="Update">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="action-btn reject cgRemoveReportCustomField" id="btnRemoveReportCustomField_@field.Name" title="Remove">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M6 18L18 6M6 6l12 12" /></svg>
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

            <!-- ==================== PROCESS CUSTOM FIELDS TAB ==================== -->
            <div id="process-custom-content" class="tab-content @(Model.PageAction == ManageReportingOrgPageAction.paProcessCustomFields ? "active" : "")">

                <div class="info-card">
                    <div class="info-card-left">
                        <span class="info-card-title">Process Inventory Custom Fields</span>
                        <span class="status-badge status-active">Active</span>
                    </div>
                </div>

                <!-- Add New Process Field Form -->
                <div class="details-grid">
                    <div class="detail-item" data-type="text" style="grid-column: span 2;">
                        <span class="detail-label">Display Label</span>
                        @Html.TextBoxFor(m => m.TheProcessFieldDefinition.Label, new { @class = "field-input", @maxlength = "30", @placeholder = "Display Label", @style = "border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" data-type="select">
                        <span class="detail-label">Data Type</span>
                        <div class="select-wrapper">
                            @Html.DropDownListFor(m => m.TheProcessFieldDefinition.DataType, new SelectList(Model.FieldTypes), "(Select Data Type)")
                        </div>
                    </div>

                    <div class="detail-item" data-type="text">
                        <span class="detail-label">Display Order</span>
                        @Html.TextBoxFor(m => m.TheProcessFieldDefinition.DisplayOrder, new { @class = "field-input", @maxlength = "3", @placeholder = "Order", @style = "border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Mandatory, new { @class = "standard-checkbox" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Mandatory)</span>
                            </div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="checkbox">
                        <span class="detail-label"></span>
                        <div class="checkbox-display-container">
                            <div class="checkbox-row">
                                @Html.CheckBoxFor(m => m.TheProcessFieldDefinition.Visible, new { @class = "standard-checkbox" })
                                <span class="checkbox-status-label">@Html.DisplayNameFor(m => m.TheProcessFieldDefinition.Visible)</span>
                            </div>
                        </div>
                    </div>

                    <div class="detail-item" data-type="text" style="grid-column: span 2;">
                        <span class="detail-label">Tooltip</span>
                        @Html.TextAreaFor(m => m.TheProcessFieldDefinition.Tooltip, new { @class = "field-input", @placeholder = "Tooltip", @rows = "2", @style = "height: auto; border-color: #d1d5db;" })
                    </div>

                    <div class="detail-item" style="display: flex; align-items: flex-end;">
                        <button type="button" id="btnAddProcessCustomField" class="btn btn-primary">Add Field</button>
                    </div>

                    <div class="detail-item" data-type="text" style="grid-column: span 3;" id="row_ProcessLookupValues">
                        <span class="detail-label">Lookup Values</span>
                        @Html.TextBoxFor(m => m.TheProcessFieldDefinition.LookupValues, new { @class = "field-input", @placeholder = "Enter lookup values separated by Semicolon, like (Shared Drive;SDP;Database;)", @style = "border-color: #d1d5db;" })
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
                                        @Html.CheckBoxFor(m => field.Mandatory, new { @class = "standard-checkbox", @Name = "ProcessCustomFields[" + i + "].Mandatory" })
                                    </td>
                                    <td>
                                        @Html.CheckBoxFor(m => field.Visible, new { @class = "standard-checkbox", @Name = "ProcessCustomFields[" + i + "].Visible" })
                                    </td>
                                    <td>
                                        @Html.TextBoxFor(m => field.DisplayOrder, new { @class = "inline-input", @Name = "ProcessCustomFields[" + i + "].DisplayOrder", @maxlength = "3", @style = "width: 60px;" })
                                    </td>
                                    <td>
                                        <div class="action-buttons">
                                            <button type="button" class="action-btn approve cgUpdateProcessCustomField" id="btnUpdateProcessCustomField_@field.Name" title="Update">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="action-btn reject cgRemoveProcessCustomField" id="btnRemoveProcessCustomField_@field.Name" title="Remove">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor"><path d="M6 18L18 6M6 6l12 12" /></svg>
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
    }
</div>

@section Scripts
{
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <script>
        $(document).ready(function () {
            // ==========================================
            // TABS LOGIC
            // ==========================================
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
                var tabName = $(this).data('tab');
                $('.tab-content').removeClass('active');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // DROPDOWN CHANGE HANDLERS
            // ==========================================
            $('#SelectedDivisionID').on('change', function () {
                $('#frmManageReportingOrg').submit();
            });

            $('#SelectedReportingOrgID').on('change', function () {
                $('#frmManageReportingOrg').submit();
            });

            // ==========================================
            // TEXT FIELD EDIT LOGIC
            // ==========================================

            // 1. Enter Edit Mode (Only for text fields)
            $(document).on('click', '.edit-icon-btn', function () {
                var $item = $(this).closest('.detail-item');
                var $displayContainer = $item.find('.detail-value-box');
                var $editContainer = $item.find('.edit-mode-container');
                var currentVal = $item.find('.detail-value').text().trim();

                // Pre-fill input
                $editContainer.find('.field-input').val(currentVal).focus();

                // Switch Views
                $displayContainer.addClass('hidden');
                $editContainer.addClass('active');
            });

            // 2. Cancel Changes (Text Fields)
            $(document).on('click', '.mini-btn-cancel', function () {
                var $item = $(this).closest('.detail-item');
                var $displayContainer = $item.find('.detail-value-box');
                var $editContainer = $item.find('.edit-mode-container');

                // Just Switch Views Back (No Save)
                $editContainer.removeClass('active');
                $displayContainer.removeClass('hidden');
            });

            // 3. Handle Enter Key on Text Inputs
            $(document).on('keypress', '.field-input', function (e) {
                if (e.which === 13) { // Enter key
                    $(this).siblings('.mini-btn-save').click();
                }
            });

            // ==========================================
            // CHECKBOX LOGIC (Direct Update)
            // ==========================================
            $(document).on('change', '.standard-checkbox', function () {
                var isChecked = $(this).is(':checked');
                var status = isChecked ? 'Enabled' : 'Disabled';

                // Log for debugging - actual save handled by existing cgUpdateReportingOrg class
                console.log("Checkbox updated:", $(this).closest('.detail-item').find('.checkbox-status-label').text(), status);
            });
        });
    </script>
    <script src="~/js2/manageReportingOrgs.js"></script>
}
