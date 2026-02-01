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
@using ROV.ReportHub.ViewModels
@model ManageResourcesViewModel
@{
    ViewBag.Title = "Manage Resources";
}

<link href="~/css/styles.css" rel="stylesheet" />

@* ============================================
   PAGE SPECIFIC STYLES - ADD TO GLOBAL CSS
   ============================================ *@
<style>
    /* ============================================
       PAGE SPECIFIC STYLES (Manage Resources)
       ============================================ */

    /* Top Selector Row */
    .top-selector-row {
        display: flex;
        align-items: center;
        gap: 16px;
        margin-bottom: 24px;
    }

    .top-selector-row label {
        font-size: 14px;
        color: #666;
        font-weight: 500;
        min-width: 60px;
    }

    .top-selector-row .select-wrapper {
        flex: 1;
        max-width: 500px;
    }

    /* Tabs with Count */
    .tabs-with-count .tab {
        padding: 14px 30px;
    }

    /* Table Controls Row */
    .table-controls {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 16px;
    }

    .btn-new {
        background: #1e3a5f;
        color: #fff;
        border: none;
        padding: 10px 20px;
        border-radius: 4px;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.2s;
    }

    .btn-new:hover {
        background: #162d4a;
    }

    .btn-new svg {
        width: 16px;
        height: 16px;
    }

    /* Inline Edit Row (Yellow) */
    .inline-edit-row {
        background: #fffbeb !important;
    }

    .inline-edit-row:hover {
        background: #fffbeb !important;
    }

    .inline-edit-row td {
        padding: 12px 16px;
        vertical-align: middle;
    }

    .inline-edit-row .inline-input {
        border: 1px solid #d1d5db;
        background: #fff;
    }

    /* Inline Input for Table */
    .inline-input {
        padding: 8px 12px;
        font-size: 14px;
        border: 1px solid transparent;
        border-radius: 4px;
        background: transparent;
        width: 100%;
        outline: none;
        transition: all 0.2s;
    }

    .inline-input:focus {
        border-color: #0049ac;
        background: #fff;
        box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.15);
    }

    /* Status Dropdown */
    .status-select {
        padding: 8px 32px 8px 12px;
        font-size: 14px;
        border: 1px solid #d1d5db;
        border-radius: 4px;
        background: #fff;
        appearance: none;
        cursor: pointer;
        min-width: 120px;
        background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23666'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'/%3E%3C/svg%3E");
        background-repeat: no-repeat;
        background-position: right 8px center;
        background-size: 16px;
    }

    /* Status Text */
    .status-text {
        font-size: 14px;
        font-weight: 500;
    }

    .status-text.active {
        color: #4a7c59;
    }

    .status-text.inactive {
        color: #c53030;
    }

    /* Row Action Buttons */
    .row-actions {
        display: flex;
        align-items: center;
        gap: 12px;
        justify-content: flex-end;
    }

    /* Save/Cancel buttons for inline edit */
    .btn-row-save {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        border: 2px solid #1e3a5f;
        background: transparent;
        color: #1e3a5f;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s;
    }

    .btn-row-save:hover {
        background: #1e3a5f;
        color: #fff;
    }

    .btn-row-cancel {
        width: 32px;
        height: 32px;
        border-radius: 50%;
        border: 2px solid #9b2c2c;
        background: transparent;
        color: #9b2c2c;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: all 0.2s;
    }

    .btn-row-cancel:hover {
        background: #9b2c2c;
        color: #fff;
    }

    .btn-row-save svg,
    .btn-row-cancel svg {
        width: 14px;
        height: 14px;
    }

    /* Edit Pencil Button */
    .btn-edit {
        background: none;
        border: none;
        cursor: pointer;
        color: #6b7280;
        padding: 4px;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: color 0.2s;
    }

    .btn-edit:hover {
        color: #1e3a5f;
    }

    .btn-edit svg {
        width: 18px;
        height: 18px;
    }

    /* 3-dot Menu Button */
    .btn-menu {
        background: none;
        border: none;
        cursor: pointer;
        color: #6b7280;
        padding: 4px;
        display: flex;
        align-items: center;
        justify-content: center;
        transition: color 0.2s;
    }

    .btn-menu:hover {
        color: #1e3a5f;
    }

    .btn-menu svg {
        width: 20px;
        height: 20px;
    }

    /* Hidden class */
    .hidden {
        display: none !important;
    }

    /* Responsive */
    @media (max-width: 768px) {
        .top-selector-row {
            flex-direction: column;
            align-items: flex-start;
            gap: 8px;
        }

        .top-selector-row .select-wrapper {
            max-width: 100%;
            width: 100%;
        }

        .table-controls {
            flex-direction: column;
            gap: 12px;
            align-items: flex-start;
        }
    }
</style>

<div class="container layout-flex">
    @using (Html.BeginForm("ManageResources", "Division", FormMethod.Post, new { @id = "frmManageResources" }))
    {
        @Html.AntiForgeryToken()
        @Html.HiddenFor(m => m.PageAction)

        <h1>Resources</h1>

        <!-- Division Selector -->
        <div class="top-selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                @Html.DropDownListFor(m => m.SelectedDivisionID,
                    new SelectList(Model.Divisions, "ID", "CodeName"),
                    "(Please Select)")
            </div>
            <button type="button" class="btn btn-secondary">View Fields</button>
        </div>

        @if (Model.SelectedDivisionID > 0)
        {
            string acExecutive = Model.PageAction == ResourceManagementPageAction.paExecutive ? "active" : "";
            string acReportLead = Model.PageAction == ResourceManagementPageAction.paReportLead ? "active" : "";
            string acReportManager = Model.PageAction == ResourceManagementPageAction.paReportManager ? "active" : "";
            string aciDrive = Model.PageAction == ResourceManagementPageAction.paIDrive ? "active" : "";

            <!-- Tabs with Counts -->
            <div class="tabs tabs-with-count">
                <div class="tab @acExecutive" data-tab="executive" id="btnSwitchTab_@((int)ResourceManagementPageAction.paExecutive)">Executives (@Model.Executives.Count)</div>
                <div class="tab @acReportLead" data-tab="reportlead" id="btnSwitchTab_@((int)ResourceManagementPageAction.paReportLead)">Report Leads (@Model.ReportLeads.Count)</div>
                <div class="tab @acReportManager" data-tab="reportmanager" id="btnSwitchTab_@((int)ResourceManagementPageAction.paReportManager)">Report Manager (@Model.ReportManagers.Count)</div>
                <div class="tab @aciDrive" data-tab="idrive" id="btnSwitchTab_@((int)ResourceManagementPageAction.paIDrive)">iDrives (@Model.iDrives.Count)</div>
            </div>

            <!-- ==================== EXECUTIVES TAB ==================== -->
            @if (Model.PageAction == ResourceManagementPageAction.paExecutive)
            {
                <div id="executive-content" class="tab-content active">
                    <!-- Table Controls -->
                    <div class="table-controls">
                        <button type="button" id="btnShowAddExecutive" class="btn-new">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                            New
                        </button>
                        <div class="search-box">
                            <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                        </div>
                    </div>

                    <!-- Data Table -->
                    <table class="data-table" id="tblExecutive">
                        <thead>
                            <tr>
                                <th style="width: 100px;">ID</th>
                                <th style="width: 180px;">NBK ID</th>
                                <th>Full Name</th>
                                <th style="width: 150px;">Status</th>
                                <th style="width: 100px;"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Add New Row (Hidden by default) -->
                            <tr class="inline-edit-row hidden" id="addExecutiveRow">
                                <td>
                                    <input type="text" class="inline-input" disabled placeholder="Auto" style="width: 80px;" />
                                </td>
                                <td>
                                    @Html.HiddenFor(m => m.TheExecutive.User_ID, new { @id = "Executive_User_ID_0" })
                                    <input type="text" class="inline-input" id="Executive_NBK_0" name="Executive_NBK" placeholder="Enter NBK or Email..." />
                                </td>
                                <td>
                                    <input type="text" class="inline-input" id="Executive_Username_0" disabled placeholder="Full Name" />
                                    @Html.HiddenFor(m => m.TheExecutive.AssigneeName)
                                </td>
                                <td>
                                    <select class="status-select" id="TheExecutive_ActiveFlag" name="TheExecutive.ActiveFlag">
                                        <option value="true" selected>Active</option>
                                        <option value="false">Inactive</option>
                                    </select>
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="btn-row-save" id="btnAddExecutive" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="btn-row-cancel" id="btnCancelAddExecutive" title="Cancel">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>

                            <!-- Existing Rows -->
                            @for (int i = 0; i < Model.Executives.Count; i++)
                            {
                                var resource = Model.Executives.ToArray()[i];
                                <tr id="Executive_Row_@resource.ID">
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "Executive_ID_" + resource.ID })
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "Executive_User_ID_" + resource.ID })

                                    <!-- Display Mode -->
                                    <td class="display-cell">@resource.ID</td>
                                    <td class="display-cell">
                                        @if (resource.User_ID > 0)
                                        {
                                            @resource.User.NBK
                                        }
                                        else
                                        {
                                            @resource.AssigneeName
                                        }
                                    </td>
                                    <td class="display-cell">
                                        @if (resource.User_ID > 0)
                                        {
                                            @resource.User.Username
                                        }
                                        else
                                        {
                                            <span class="email-text">@resource.AssigneeName</span>
                                        }
                                    </td>
                                    <td class="display-cell">
                                        @if (resource.ActiveFlag)
                                        {
                                            <span class="status-text active">Active</span>
                                        }
                                        else
                                        {
                                            <span class="status-text inactive">Inactive</span>
                                        }
                                    </td>
                                    <td class="display-cell">
                                        <div class="row-actions">
                                            <button type="button" class="btn-edit cgEditExecutive" data-id="@resource.ID" title="Edit">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                            </button>
                                            <button type="button" class="btn-menu" title="More options">
                                                <svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="2" /><circle cx="12" cy="12" r="2" /><circle cx="12" cy="19" r="2" /></svg>
                                            </button>
                                        </div>
                                    </td>

                                    <!-- Edit Mode (Hidden) -->
                                    <td class="edit-cell hidden">@resource.ID</td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="Executive_NBK_@resource.ID" name="Executive_NBK" value="@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)" placeholder="Enter NBK or Email..." />
                                    </td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="Executive_Username_@resource.ID" disabled value="@(resource.User_ID > 0 ? resource.User.Username : "")" placeholder="Full Name" />
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "Executive_AssigneeName_" + resource.ID })
                                    </td>
                                    <td class="edit-cell hidden">
                                        <select class="status-select" id="Executive_ActiveFlag_@resource.ID" name="Executive_ActiveFlag_@resource.ID">
                                            <option value="true" @(resource.ActiveFlag ? "selected" : "")>Active</option>
                                            <option value="false" @(!resource.ActiveFlag ? "selected" : "")>Inactive</option>
                                        </select>
                                    </td>
                                    <td class="edit-cell hidden">
                                        <div class="row-actions">
                                            <button type="button" class="btn-row-save cgUpdateExecutive" id="btnUpdateExecutive_@resource.ID" title="Save">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="btn-row-cancel cgCancelEditExecutive" data-id="@resource.ID" title="Cancel">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            }

                            @if (Model.Executives.Count == 0)
                            {
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 32px; color: #666;">
                                        No executives found.
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>

                    <!-- Pagination -->
                    <div class="pagination-wrapper">
                        <span class="showing-text">Showing 1-@Model.Executives.Count of @Model.Executives.Count</span>
                        <div class="pagination">
                            <button disabled>«</button>
                            <button disabled>‹</button>
                            <button class="active">1</button>
                            <button>2</button>
                            <button>3</button>
                            <span class="dots">...</span>
                            <button>›</button>
                            <button>»</button>
                        </div>
                        <div class="per-page">
                            <span>Show per page:</span>
                            <select>
                                <option>10</option>
                                <option>25</option>
                                <option>50</option>
                                <option>100</option>
                            </select>
                        </div>
                    </div>
                </div>
            }

            <!-- ==================== REPORT LEADS TAB ==================== -->
            @if (Model.PageAction == ResourceManagementPageAction.paReportLead)
            {
                <div id="reportlead-content" class="tab-content active">
                    <div class="table-controls">
                        <button type="button" id="btnShowAddReportLead" class="btn-new">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                            New
                        </button>
                        <div class="search-box">
                            <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                        </div>
                    </div>

                    <table class="data-table" id="tblReportLead">
                        <thead>
                            <tr>
                                <th style="width: 100px;">ID</th>
                                <th style="width: 180px;">NBK ID</th>
                                <th>Full Name</th>
                                <th style="width: 150px;">Status</th>
                                <th style="width: 100px;"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Add New Row (Hidden by default) -->
                            <tr class="inline-edit-row hidden" id="addReportLeadRow">
                                <td>
                                    <input type="text" class="inline-input" disabled placeholder="Auto" style="width: 80px;" />
                                </td>
                                <td>
                                    @Html.HiddenFor(m => m.TheReportLead.User_ID, new { @id = "ReportLead_User_ID_0" })
                                    <input type="text" class="inline-input" id="ReportLead_NBK_0" name="ReportLead_NBK" placeholder="Enter NBK or Email..." />
                                </td>
                                <td>
                                    <input type="text" class="inline-input" id="ReportLead_Username_0" disabled placeholder="Full Name" />
                                    @Html.HiddenFor(m => m.TheReportLead.AssigneeName)
                                </td>
                                <td>
                                    <select class="status-select" id="TheReportLead_ActiveFlag" name="TheReportLead.ActiveFlag">
                                        <option value="true" selected>Active</option>
                                        <option value="false">Inactive</option>
                                    </select>
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="btn-row-save" id="btnAddReportLead" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="btn-row-cancel" id="btnCancelAddReportLead" title="Cancel">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>

                            @for (int i = 0; i < Model.ReportLeads.Count; i++)
                            {
                                var resource = Model.ReportLeads.ToArray()[i];
                                <tr id="ReportLead_Row_@resource.ID">
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "ReportLead_ID_" + resource.ID })
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "ReportLead_User_ID_" + resource.ID })

                                    <!-- Display Mode -->
                                    <td class="display-cell">@resource.ID</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.Username : resource.AssigneeName)</td>
                                    <td class="display-cell">
                                        <span class="status-text @(resource.ActiveFlag ? "active" : "inactive")">@(resource.ActiveFlag ? "Active" : "Inactive")</span>
                                    </td>
                                    <td class="display-cell">
                                        <div class="row-actions">
                                            <button type="button" class="btn-edit cgEditReportLead" data-id="@resource.ID" title="Edit">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                            </button>
                                            <button type="button" class="btn-menu" title="More options">
                                                <svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="2" /><circle cx="12" cy="12" r="2" /><circle cx="12" cy="19" r="2" /></svg>
                                            </button>
                                        </div>
                                    </td>

                                    <!-- Edit Mode (Hidden) -->
                                    <td class="edit-cell hidden">@resource.ID</td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="ReportLead_NBK_@resource.ID" name="ReportLead_NBK" value="@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)" />
                                    </td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="ReportLead_Username_@resource.ID" disabled value="@(resource.User_ID > 0 ? resource.User.Username : "")" />
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "ReportLead_AssigneeName_" + resource.ID })
                                    </td>
                                    <td class="edit-cell hidden">
                                        <select class="status-select" id="ReportLead_ActiveFlag_@resource.ID">
                                            <option value="true" @(resource.ActiveFlag ? "selected" : "")>Active</option>
                                            <option value="false" @(!resource.ActiveFlag ? "selected" : "")>Inactive</option>
                                        </select>
                                    </td>
                                    <td class="edit-cell hidden">
                                        <div class="row-actions">
                                            <button type="button" class="btn-row-save cgUpdateReportLead" id="btnUpdateReportLead_@resource.ID" title="Save">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="btn-row-cancel cgCancelEditReportLead" data-id="@resource.ID" title="Cancel">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            }

                            @if (Model.ReportLeads.Count == 0)
                            {
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 32px; color: #666;">
                                        No report leads found.
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>

                    <div class="pagination-wrapper">
                        <span class="showing-text">Showing 1-@Model.ReportLeads.Count of @Model.ReportLeads.Count</span>
                        <div class="pagination">
                            <button disabled>«</button>
                            <button disabled>‹</button>
                            <button class="active">1</button>
                            <button>2</button>
                            <button>3</button>
                            <span class="dots">...</span>
                            <button>›</button>
                            <button>»</button>
                        </div>
                        <div class="per-page">
                            <span>Show per page:</span>
                            <select>
                                <option>10</option>
                                <option>25</option>
                                <option>50</option>
                                <option>100</option>
                            </select>
                        </div>
                    </div>
                </div>
            }

            <!-- ==================== REPORT MANAGERS TAB ==================== -->
            @if (Model.PageAction == ResourceManagementPageAction.paReportManager)
            {
                <div id="reportmanager-content" class="tab-content active">
                    <div class="table-controls">
                        <button type="button" id="btnShowAddReportManager" class="btn-new">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                            New
                        </button>
                        <div class="search-box">
                            <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                        </div>
                    </div>

                    <table class="data-table" id="tblReportManager">
                        <thead>
                            <tr>
                                <th style="width: 100px;">ID</th>
                                <th style="width: 180px;">NBK ID</th>
                                <th>Full Name</th>
                                <th style="width: 150px;">Status</th>
                                <th style="width: 100px;"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Add New Row (Hidden by default) -->
                            <tr class="inline-edit-row hidden" id="addReportManagerRow">
                                <td>
                                    <input type="text" class="inline-input" disabled placeholder="Auto" style="width: 80px;" />
                                </td>
                                <td>
                                    @Html.HiddenFor(m => m.TheReportManager.User_ID, new { @id = "ReportManager_User_ID_0" })
                                    <input type="text" class="inline-input" id="ReportManager_NBK_0" name="ReportManager_NBK" placeholder="Enter NBK or Email..." />
                                </td>
                                <td>
                                    <input type="text" class="inline-input" id="ReportManager_Username_0" disabled placeholder="Full Name" />
                                    @Html.HiddenFor(m => m.TheReportManager.AssigneeName)
                                </td>
                                <td>
                                    <select class="status-select" id="TheReportManager_ActiveFlag" name="TheReportManager.ActiveFlag">
                                        <option value="true" selected>Active</option>
                                        <option value="false">Inactive</option>
                                    </select>
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="btn-row-save" id="btnAddReportManager" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="btn-row-cancel" id="btnCancelAddReportManager" title="Cancel">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>

                            @for (int i = 0; i < Model.ReportManagers.Count; i++)
                            {
                                var resource = Model.ReportManagers.ToArray()[i];
                                <tr id="ReportManager_Row_@resource.ID">
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "ReportManager_ID_" + resource.ID })
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "ReportManager_User_ID_" + resource.ID })

                                    <!-- Display Mode -->
                                    <td class="display-cell">@resource.ID</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.Username : resource.AssigneeName)</td>
                                    <td class="display-cell">
                                        <span class="status-text @(resource.ActiveFlag ? "active" : "inactive")">@(resource.ActiveFlag ? "Active" : "Inactive")</span>
                                    </td>
                                    <td class="display-cell">
                                        <div class="row-actions">
                                            <button type="button" class="btn-edit cgEditReportManager" data-id="@resource.ID" title="Edit">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                            </button>
                                            <button type="button" class="btn-menu" title="More options">
                                                <svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="2" /><circle cx="12" cy="12" r="2" /><circle cx="12" cy="19" r="2" /></svg>
                                            </button>
                                        </div>
                                    </td>

                                    <!-- Edit Mode (Hidden) -->
                                    <td class="edit-cell hidden">@resource.ID</td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="ReportManager_NBK_@resource.ID" name="ReportManager_NBK" value="@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)" />
                                    </td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="ReportManager_Username_@resource.ID" disabled value="@(resource.User_ID > 0 ? resource.User.Username : "")" />
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "ReportManager_AssigneeName_" + resource.ID })
                                    </td>
                                    <td class="edit-cell hidden">
                                        <select class="status-select" id="ReportManager_ActiveFlag_@resource.ID">
                                            <option value="true" @(resource.ActiveFlag ? "selected" : "")>Active</option>
                                            <option value="false" @(!resource.ActiveFlag ? "selected" : "")>Inactive</option>
                                        </select>
                                    </td>
                                    <td class="edit-cell hidden">
                                        <div class="row-actions">
                                            <button type="button" class="btn-row-save cgUpdateReportManager" id="btnUpdateReportManager_@resource.ID" title="Save">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="btn-row-cancel cgCancelEditReportManager" data-id="@resource.ID" title="Cancel">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            }

                            @if (Model.ReportManagers.Count == 0)
                            {
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 32px; color: #666;">
                                        No report managers found.
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>

                    <div class="pagination-wrapper">
                        <span class="showing-text">Showing 1-@Model.ReportManagers.Count of @Model.ReportManagers.Count</span>
                        <div class="pagination">
                            <button disabled>«</button>
                            <button disabled>‹</button>
                            <button class="active">1</button>
                            <button>2</button>
                            <button>3</button>
                            <span class="dots">...</span>
                            <button>›</button>
                            <button>»</button>
                        </div>
                        <div class="per-page">
                            <span>Show per page:</span>
                            <select>
                                <option>10</option>
                                <option>25</option>
                                <option>50</option>
                                <option>100</option>
                            </select>
                        </div>
                    </div>
                </div>
            }

            <!-- ==================== iDRIVES TAB ==================== -->
            @if (Model.PageAction == ResourceManagementPageAction.paIDrive)
            {
                <div id="idrive-content" class="tab-content active">
                    <div class="table-controls">
                        <button type="button" id="btnShowAddiDrive" class="btn-new">
                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                            New
                        </button>
                        <div class="search-box">
                            <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                        </div>
                    </div>

                    <table class="data-table" id="tbliDrive">
                        <thead>
                            <tr>
                                <th style="width: 100px;">ID</th>
                                <th style="width: 180px;">NBK ID</th>
                                <th>Full Name</th>
                                <th style="width: 150px;">Status</th>
                                <th style="width: 100px;"></th>
                            </tr>
                        </thead>
                        <tbody>
                            <!-- Add New Row (Hidden by default) -->
                            <tr class="inline-edit-row hidden" id="addiDriveRow">
                                <td>
                                    <input type="text" class="inline-input" disabled placeholder="Auto" style="width: 80px;" />
                                </td>
                                <td>
                                    @Html.HiddenFor(m => m.TheiDrive.User_ID, new { @id = "iDrive_User_ID_0" })
                                    <input type="text" class="inline-input" id="iDrive_NBK_0" name="iDrive_NBK" placeholder="Enter NBK or Email..." />
                                </td>
                                <td>
                                    <input type="text" class="inline-input" id="iDrive_Username_0" disabled placeholder="Full Name" />
                                    @Html.HiddenFor(m => m.TheiDrive.AssigneeName)
                                </td>
                                <td>
                                    <select class="status-select" id="TheiDrive_ActiveFlag" name="TheiDrive.ActiveFlag">
                                        <option value="true" selected>Active</option>
                                        <option value="false">Inactive</option>
                                    </select>
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="btn-row-save" id="btnAddiDrive" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="btn-row-cancel" id="btnCancelAddiDrive" title="Cancel">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>

                            @for (int i = 0; i < Model.iDrives.Count; i++)
                            {
                                var resource = Model.iDrives.ToArray()[i];
                                <tr id="iDrive_Row_@resource.ID">
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "iDrive_ID_" + resource.ID })
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "iDrive_User_ID_" + resource.ID })

                                    <!-- Display Mode -->
                                    <td class="display-cell">@resource.ID</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)</td>
                                    <td class="display-cell">@(resource.User_ID > 0 ? resource.User.Username : resource.AssigneeName)</td>
                                    <td class="display-cell">
                                        <span class="status-text @(resource.ActiveFlag ? "active" : "inactive")">@(resource.ActiveFlag ? "Active" : "Inactive")</span>
                                    </td>
                                    <td class="display-cell">
                                        <div class="row-actions">
                                            <button type="button" class="btn-edit cgEditiDrive" data-id="@resource.ID" title="Edit">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z" /></svg>
                                            </button>
                                            <button type="button" class="btn-menu" title="More options">
                                                <svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="2" /><circle cx="12" cy="12" r="2" /><circle cx="12" cy="19" r="2" /></svg>
                                            </button>
                                        </div>
                                    </td>

                                    <!-- Edit Mode (Hidden) -->
                                    <td class="edit-cell hidden">@resource.ID</td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="iDrive_NBK_@resource.ID" name="iDrive_NBK" value="@(resource.User_ID > 0 ? resource.User.NBK : resource.AssigneeName)" />
                                    </td>
                                    <td class="edit-cell hidden">
                                        <input type="text" class="inline-input" id="iDrive_Username_@resource.ID" disabled value="@(resource.User_ID > 0 ? resource.User.Username : "")" />
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "iDrive_AssigneeName_" + resource.ID })
                                    </td>
                                    <td class="edit-cell hidden">
                                        <select class="status-select" id="iDrive_ActiveFlag_@resource.ID">
                                            <option value="true" @(resource.ActiveFlag ? "selected" : "")>Active</option>
                                            <option value="false" @(!resource.ActiveFlag ? "selected" : "")>Inactive</option>
                                        </select>
                                    </td>
                                    <td class="edit-cell hidden">
                                        <div class="row-actions">
                                            <button type="button" class="btn-row-save cgUpdateiDrive" id="btnUpdateiDrive_@resource.ID" title="Save">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                            </button>
                                            <button type="button" class="btn-row-cancel cgCancelEditiDrive" data-id="@resource.ID" title="Cancel">
                                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            }

                            @if (Model.iDrives.Count == 0)
                            {
                                <tr>
                                    <td colspan="5" style="text-align: center; padding: 32px; color: #666;">
                                        No iDrives found.
                                    </td>
                                </tr>
                            }
                        </tbody>
                    </table>

                    <div class="pagination-wrapper">
                        <span class="showing-text">Showing 1-@Model.iDrives.Count of @Model.iDrives.Count</span>
                        <div class="pagination">
                            <button disabled>«</button>
                            <button disabled>‹</button>
                            <button class="active">1</button>
                            <button>2</button>
                            <button>3</button>
                            <span class="dots">...</span>
                            <button>›</button>
                            <button>»</button>
                        </div>
                        <div class="per-page">
                            <span>Show per page:</span>
                            <select>
                                <option>10</option>
                                <option>25</option>
                                <option>50</option>
                                <option>100</option>
                            </select>
                        </div>
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
            // ==========================================
            // DROPDOWN CHANGE HANDLER
            // ==========================================
            $('#SelectedDivisionID').on('change', function () {
                $('#frmManageResources').submit();
            });

            // ==========================================
            // TAB SWITCHING
            // ==========================================
            $('.tab').on('click', function () {
                var tabId = $(this).attr('id');
                if (tabId && tabId.indexOf('btnSwitchTab_') === 0) {
                    var pageAction = tabId.replace('btnSwitchTab_', '');
                    $('#PageAction').val(pageAction);
                    $('#frmManageResources').submit();
                }
            });

            // ==========================================
            // SHOW/HIDE ADD ROW
            // ==========================================
            // Executives
            $('#btnShowAddExecutive').on('click', function () {
                $('#addExecutiveRow').removeClass('hidden');
                $(this).addClass('hidden');
            });
            $('#btnCancelAddExecutive').on('click', function () {
                $('#addExecutiveRow').addClass('hidden');
                $('#btnShowAddExecutive').removeClass('hidden');
                // Clear inputs
                $('#Executive_NBK_0').val('');
                $('#Executive_Username_0').val('');
                $('#Executive_User_ID_0').val('');
            });

            // Report Leads
            $('#btnShowAddReportLead').on('click', function () {
                $('#addReportLeadRow').removeClass('hidden');
                $(this).addClass('hidden');
            });
            $('#btnCancelAddReportLead').on('click', function () {
                $('#addReportLeadRow').addClass('hidden');
                $('#btnShowAddReportLead').removeClass('hidden');
                $('#ReportLead_NBK_0').val('');
                $('#ReportLead_Username_0').val('');
                $('#ReportLead_User_ID_0').val('');
            });

            // Report Managers
            $('#btnShowAddReportManager').on('click', function () {
                $('#addReportManagerRow').removeClass('hidden');
                $(this).addClass('hidden');
            });
            $('#btnCancelAddReportManager').on('click', function () {
                $('#addReportManagerRow').addClass('hidden');
                $('#btnShowAddReportManager').removeClass('hidden');
                $('#ReportManager_NBK_0').val('');
                $('#ReportManager_Username_0').val('');
                $('#ReportManager_User_ID_0').val('');
            });

            // iDrives
            $('#btnShowAddiDrive').on('click', function () {
                $('#addiDriveRow').removeClass('hidden');
                $(this).addClass('hidden');
            });
            $('#btnCancelAddiDrive').on('click', function () {
                $('#addiDriveRow').addClass('hidden');
                $('#btnShowAddiDrive').removeClass('hidden');
                $('#iDrive_NBK_0').val('');
                $('#iDrive_Username_0').val('');
                $('#iDrive_User_ID_0').val('');
            });

            // ==========================================
            // INLINE EDIT MODE TOGGLE
            // ==========================================
            function enterEditMode(resourceType, id) {
                var $row = $('#' + resourceType + '_Row_' + id);
                $row.find('.display-cell').addClass('hidden');
                $row.find('.edit-cell').removeClass('hidden');
                $row.addClass('inline-edit-row');
            }

            function exitEditMode(resourceType, id) {
                var $row = $('#' + resourceType + '_Row_' + id);
                $row.find('.edit-cell').addClass('hidden');
                $row.find('.display-cell').removeClass('hidden');
                $row.removeClass('inline-edit-row');
            }

            // Edit button handlers
            $(document).on('click', '.cgEditExecutive', function () {
                enterEditMode('Executive', $(this).data('id'));
            });
            $(document).on('click', '.cgCancelEditExecutive', function () {
                exitEditMode('Executive', $(this).data('id'));
            });

            $(document).on('click', '.cgEditReportLead', function () {
                enterEditMode('ReportLead', $(this).data('id'));
            });
            $(document).on('click', '.cgCancelEditReportLead', function () {
                exitEditMode('ReportLead', $(this).data('id'));
            });

            $(document).on('click', '.cgEditReportManager', function () {
                enterEditMode('ReportManager', $(this).data('id'));
            });
            $(document).on('click', '.cgCancelEditReportManager', function () {
                exitEditMode('ReportManager', $(this).data('id'));
            });

            $(document).on('click', '.cgEditiDrive', function () {
                enterEditMode('iDrive', $(this).data('id'));
            });
            $(document).on('click', '.cgCancelEditiDrive', function () {
                exitEditMode('iDrive', $(this).data('id'));
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function () {
                var searchTerm = $(this).val().toLowerCase();
                var $tabContent = $(this).closest('.tab-content');
                var $rows = $tabContent.find('.data-table tbody tr:not(.inline-edit-row)');
                var visibleCount = 0;

                $rows.each(function () {
                    var rowText = $(this).text().toLowerCase();
                    if (rowText.includes(searchTerm)) {
                        $(this).show();
                        visibleCount++;
                    } else {
                        $(this).hide();
                    }
                });
            });

            // ==========================================
            // PAGINATION (UI only)
            // ==========================================
            $(document).on('click', '.pagination button:not(:disabled)', function () {
                if (!$(this).hasClass('active') && /\d/.test($(this).text())) {
                    $(this).closest('.pagination').find('button').removeClass('active');
                    $(this).addClass('active');
                }
            });
        });
    </script>
    <script src="~/js/manageResources.js"></script>
}
