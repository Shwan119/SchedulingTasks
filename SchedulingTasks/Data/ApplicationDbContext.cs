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

    /* Top Selector Alignment */
    .top-selectors {
        display: flex;
        flex-direction: column;
        gap: 16px;
        margin-bottom: 24px;
    }

    .top-selectors .selector-row {
        display: flex;
        align-items: center;
        gap: 16px;
        margin-bottom: 0;
    }

    .top-selectors .selector-row label {
        font-size: 14px;
        color: #666;
        font-weight: 500;
        min-width: 160px;
        flex-shrink: 0;
    }

    .top-selectors .select-wrapper {
        position: relative;
        width: 100%;
        max-width: 500px;
    }

    /* Add New Row Styling */
    .add-new-row {
        background: #fffbeb !important;
        border-left: 3px solid #f59e0b;
    }

    .add-new-row td {
        padding: 12px 16px;
        vertical-align: middle;
    }

    /* User Input Group */
    .user-input-group {
        display: flex;
        align-items: center;
        gap: 8px;
        flex-wrap: wrap;
    }

    .user-input-group .nbk-input {
        width: 140px;
        padding: 8px 12px;
        font-size: 14px;
        border: 1px solid #d1d5db;
        border-radius: 4px 0 0 4px;
        outline: none;
    }

    .user-input-group .nbk-input:focus {
        border-color: #0049ac;
        box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.15);
    }

    .user-input-group .username-display {
        width: 160px;
        padding: 8px 12px;
        font-size: 14px;
        border: 1px solid #d1d5db;
        border-left: none;
        border-radius: 0 4px 4px 0;
        background: #f9fafb;
        color: #666;
    }

    .or-separator {
        font-size: 12px;
        color: #9ca3af;
        font-weight: 500;
        padding: 0 8px;
    }

    .user-input-group .assignee-input {
        flex: 1;
        min-width: 180px;
        padding: 8px 12px;
        font-size: 14px;
        border: 1px solid #d1d5db;
        border-radius: 4px;
        outline: none;
    }

    .user-input-group .assignee-input:focus {
        border-color: #0049ac;
        box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.15);
    }

    /* Standard Checkbox in Table */
    .table-checkbox {
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
    }

    .table-checkbox:checked {
        background-color: #0049ac;
        border-color: #0049ac;
    }

    .table-checkbox:checked::after {
        content: '';
        width: 5px;
        height: 10px;
        border: solid white;
        border-width: 0 2px 2px 0;
        transform: rotate(45deg);
        margin-bottom: 2px;
    }

    /* Status Text in Table */
    .status-text-active {
        color: #4a7c59;
        font-weight: 500;
    }

    .status-text-inactive {
        color: #c53030;
        font-weight: 500;
    }

    /* Add Button */
    .btn-add {
        background: #4a7c59;
        color: #fff;
        border: none;
        padding: 8px 16px;
        border-radius: 4px;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        gap: 6px;
        font-size: 14px;
        font-weight: 500;
        transition: all 0.2s;
    }

    .btn-add:hover {
        background: #3d6a4a;
    }

    .btn-add svg {
        width: 16px;
        height: 16px;
    }

    /* Action buttons for table rows */
    .row-actions {
        display: flex;
        gap: 12px;
        justify-content: flex-end;
    }

    .row-action-btn {
        width: 32px;
        height: 32px;
        border-radius: 4px;
        border: 1px solid;
        display: flex;
        align-items: center;
        justify-content: center;
        cursor: pointer;
        transition: all 0.2s;
        background: #fff;
    }

    .row-action-btn.save {
        border-color: #4a7c59;
        color: #4a7c59;
    }

    .row-action-btn.save:hover {
        background: #4a7c59;
        color: #fff;
    }

    .row-action-btn.reset {
        border-color: #d97706;
        color: #d97706;
    }

    .row-action-btn.reset:hover {
        background: #d97706;
        color: #fff;
    }

    .row-action-btn svg {
        width: 16px;
        height: 16px;
    }

    /* Responsive */
    @media (max-width: 1024px) {
        .user-input-group {
            flex-direction: column;
            align-items: stretch;
        }

        .user-input-group .nbk-input {
            width: 100%;
            border-radius: 4px;
        }

        .user-input-group .username-display {
            width: 100%;
            border-left: 1px solid #d1d5db;
            border-radius: 4px;
        }

        .or-separator {
            text-align: center;
            padding: 4px 0;
        }

        .user-input-group .assignee-input {
            width: 100%;
        }
    }

    @media (max-width: 768px) {
        .top-selectors .selector-row {
            flex-direction: column;
            align-items: flex-start;
            gap: 8px;
        }

        .top-selectors .selector-row label {
            min-width: auto;
        }

        .top-selectors .select-wrapper {
            max-width: 100%;
        }
    }
</style>

<div class="container layout-flex">
    @using (Html.BeginForm("ManageResources", "Division", FormMethod.Post, new { @id = "frmManageResources" }))
    {
        @Html.AntiForgeryToken()
        @Html.HiddenFor(m => m.PageAction)

        <h1>Manage Resources</h1>

        <div class="top-selectors">
            <!-- Division Selector -->
            <div class="selector-row">
                <label>Division</label>
                <div class="select-wrapper">
                    @Html.DropDownListFor(m => m.SelectedDivisionID,
                        new SelectList(Model.Divisions, "ID", "CodeName"),
                        "(Please Select)")
                </div>
            </div>
        </div>

        @if (Model.SelectedDivisionID > 0)
        {
            string acExecutive = Model.PageAction == ResourceManagementPageAction.paExecutive ? "active" : "";
            string acReportLead = Model.PageAction == ResourceManagementPageAction.paReportLead ? "active" : "";
            string acReportManager = Model.PageAction == ResourceManagementPageAction.paReportManager ? "active" : "";
            string aciDrive = Model.PageAction == ResourceManagementPageAction.paIDrive ? "active" : "";

            <!-- Tabs -->
            <div class="tabs">
                <div class="tab @acExecutive" data-tab="executive" id="btnSwitchTab_@((int)ResourceManagementPageAction.paExecutive)">Executives</div>
                <div class="tab @acReportLead" data-tab="reportlead" id="btnSwitchTab_@((int)ResourceManagementPageAction.paReportLead)">Report Leads</div>
                <div class="tab @acReportManager" data-tab="reportmanager" id="btnSwitchTab_@((int)ResourceManagementPageAction.paReportManager)">Report Managers</div>
                <div class="tab @aciDrive" data-tab="idrive" id="btnSwitchTab_@((int)ResourceManagementPageAction.paIDrive)">iDrives</div>
            </div>

            <!-- ==================== EXECUTIVES TAB ==================== -->
            <div id="executive-content" class="tab-content @(Model.PageAction == ResourceManagementPageAction.paExecutive ? "active" : "")">
                <!-- Table Header -->
                <div class="table-header">
                    <span class="total-count">Total (@Model.Executives.Count)</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by ID or Name">
                    </div>
                </div>

                <!-- Data Table -->
                <table class="data-table" id="tblExecutive">
                    <thead>
                        <tr>
                            <th style="width: 80px;">ID</th>
                            <th>User / Assignee</th>
                            <th style="width: 100px;">Active</th>
                            <th style="width: 120px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Add New Row -->
                        <tr class="add-new-row">
                            <td></td>
                            <td>
                                <div class="user-input-group">
                                    @Html.HiddenFor(m => m.TheExecutive.User_ID, new { @id = "Executive_User_ID_0" })
                                    <input type="text" class="nbk-input" id="Executive_NBK_0" name="Executive_NBK" placeholder="Enter NBK or Email..." />
                                    <input type="text" class="username-display" id="Executive_Username_0" disabled placeholder="Username" />
                                    <span class="or-separator">OR</span>
                                    @Html.TextBoxFor(m => m.TheExecutive.AssigneeName, new { @class = "assignee-input", @placeholder = "DG Group / Virtual Resource" })
                                </div>
                            </td>
                            <td>
                                @{ Model.TheExecutive.ActiveFlag = true; }
                                <input type="checkbox" class="table-checkbox" id="TheExecutive_ActiveFlag" name="TheExecutive.ActiveFlag" value="true" checked />
                            </td>
                            <td>
                                <button type="button" id="btnAddExecutive" class="btn-add" title="Add">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                                    Add
                                </button>
                            </td>
                        </tr>

                        <!-- Existing Rows -->
                        @for (int i = 0; i < Model.Executives.Count; i++)
                        {
                            var resource = Model.Executives.ToArray()[i];
                            <tr>
                                <td>
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "Executive_ID_" + resource.ID })
                                    @resource.ID
                                </td>
                                <td>
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "Executive_User_ID_" + resource.ID })
                                    @if (resource.User_ID > 0)
                                    {
                                        <div class="user-input-group">
                                            <input type="text" class="nbk-input cgFilter" id="Executive_NBK_@resource.ID" name="Executive_NBK" value="@resource.User.NBK" placeholder="Enter NBK or Email..." />
                                            <input type="text" class="username-display cgFilter" id="Executive_Username_@resource.ID" disabled value="@resource.User.Username" />
                                        </div>
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "Executive_AssigneeName_" + resource.ID })
                                    }
                                    else
                                    {
                                        <input type="text" class="assignee-input cgFilter" id="Executive_AssigneeName_@resource.ID" name="Executive_AssigneeName" value="@resource.AssigneeName" />
                                    }
                                </td>
                                <td>
                                    <input type="checkbox" class="table-checkbox" id="Executive_ActiveFlag_@resource.ID" name="Executive_ActiveFlag_@resource.ID" value="true" @(resource.ActiveFlag ? "checked" : "") />
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="row-action-btn save cgUpdateExecutive" id="btnUpdateExecutive_@resource.ID" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="row-action-btn reset cgResetExecutive" id="btnResetExecutive_@resource.ID" title="Reset">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        }

                        @if (Model.Executives.Count == 0)
                        {
                            <tr>
                                <td colspan="4" style="text-align: center; padding: 32px; color: #666;">
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

            <!-- ==================== REPORT LEADS TAB ==================== -->
            <div id="reportlead-content" class="tab-content @(Model.PageAction == ResourceManagementPageAction.paReportLead ? "active" : "")">
                <div class="table-header">
                    <span class="total-count">Total (@Model.ReportLeads.Count)</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by ID or Name">
                    </div>
                </div>

                <table class="data-table" id="tblReportLead">
                    <thead>
                        <tr>
                            <th style="width: 80px;">ID</th>
                            <th>User / Assignee</th>
                            <th style="width: 100px;">Active</th>
                            <th style="width: 120px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Add New Row -->
                        <tr class="add-new-row">
                            <td></td>
                            <td>
                                <div class="user-input-group">
                                    @Html.HiddenFor(m => m.TheReportLead.User_ID, new { @id = "ReportLead_User_ID_0" })
                                    <input type="text" class="nbk-input" id="ReportLead_NBK_0" name="ReportLead_NBK" placeholder="Enter NBK or Email..." />
                                    <input type="text" class="username-display" id="ReportLead_Username_0" disabled placeholder="Username" />
                                    <span class="or-separator">OR</span>
                                    @Html.TextBoxFor(m => m.TheReportLead.AssigneeName, new { @class = "assignee-input", @placeholder = "DG Group / Virtual Resource" })
                                </div>
                            </td>
                            <td>
                                @{ Model.TheReportLead.ActiveFlag = true; }
                                <input type="checkbox" class="table-checkbox" id="TheReportLead_ActiveFlag" name="TheReportLead.ActiveFlag" value="true" checked />
                            </td>
                            <td>
                                <button type="button" id="btnAddReportLead" class="btn-add" title="Add">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                                    Add
                                </button>
                            </td>
                        </tr>

                        <!-- Existing Rows -->
                        @for (int i = 0; i < Model.ReportLeads.Count; i++)
                        {
                            var resource = Model.ReportLeads.ToArray()[i];
                            <tr>
                                <td>
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "ReportLead_ID_" + resource.ID })
                                    @resource.ID
                                </td>
                                <td>
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "ReportLead_User_ID_" + resource.ID })
                                    @if (resource.User_ID > 0)
                                    {
                                        <div class="user-input-group">
                                            <input type="text" class="nbk-input cgFilter" id="ReportLead_NBK_@resource.ID" name="ReportLead_NBK" value="@resource.User.NBK" placeholder="Enter NBK or Email..." />
                                            <input type="text" class="username-display cgFilter" id="ReportLead_Username_@resource.ID" disabled value="@resource.User.Username" />
                                        </div>
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "ReportLead_AssigneeName_" + resource.ID })
                                    }
                                    else
                                    {
                                        <input type="text" class="assignee-input cgFilter" id="ReportLead_AssigneeName_@resource.ID" name="ReportLead_AssigneeName" value="@resource.AssigneeName" />
                                    }
                                </td>
                                <td>
                                    <input type="checkbox" class="table-checkbox" id="ReportLead_ActiveFlag_@resource.ID" name="ReportLead_ActiveFlag_@resource.ID" value="true" @(resource.ActiveFlag ? "checked" : "") />
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="row-action-btn save cgUpdateReportLead" id="btnUpdateReportLead_@resource.ID" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="row-action-btn reset cgResetReportLead" id="btnResetReportLead_@resource.ID" title="Reset">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        }

                        @if (Model.ReportLeads.Count == 0)
                        {
                            <tr>
                                <td colspan="4" style="text-align: center; padding: 32px; color: #666;">
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

            <!-- ==================== REPORT MANAGERS TAB ==================== -->
            <div id="reportmanager-content" class="tab-content @(Model.PageAction == ResourceManagementPageAction.paReportManager ? "active" : "")">
                <div class="table-header">
                    <span class="total-count">Total (@Model.ReportManagers.Count)</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by ID or Name">
                    </div>
                </div>

                <table class="data-table" id="tblReportManager">
                    <thead>
                        <tr>
                            <th style="width: 80px;">ID</th>
                            <th>User / Assignee</th>
                            <th style="width: 100px;">Active</th>
                            <th style="width: 120px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Add New Row -->
                        <tr class="add-new-row">
                            <td></td>
                            <td>
                                <div class="user-input-group">
                                    @Html.HiddenFor(m => m.TheReportManager.User_ID, new { @id = "ReportManager_User_ID_0" })
                                    <input type="text" class="nbk-input" id="ReportManager_NBK_0" name="ReportManager_NBK" placeholder="Enter NBK or Email..." />
                                    <input type="text" class="username-display" id="ReportManager_Username_0" disabled placeholder="Username" />
                                    <span class="or-separator">OR</span>
                                    @Html.TextBoxFor(m => m.TheReportManager.AssigneeName, new { @class = "assignee-input", @placeholder = "DG Group / Virtual Resource" })
                                </div>
                            </td>
                            <td>
                                @{ Model.TheReportManager.ActiveFlag = true; }
                                <input type="checkbox" class="table-checkbox" id="TheReportManager_ActiveFlag" name="TheReportManager.ActiveFlag" value="true" checked />
                            </td>
                            <td>
                                <button type="button" id="btnAddReportManager" class="btn-add" title="Add">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                                    Add
                                </button>
                            </td>
                        </tr>

                        <!-- Existing Rows -->
                        @for (int i = 0; i < Model.ReportManagers.Count; i++)
                        {
                            var resource = Model.ReportManagers.ToArray()[i];
                            <tr>
                                <td>
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "ReportManager_ID_" + resource.ID })
                                    @resource.ID
                                </td>
                                <td>
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "ReportManager_User_ID_" + resource.ID })
                                    @if (resource.User_ID > 0)
                                    {
                                        <div class="user-input-group">
                                            <input type="text" class="nbk-input cgFilter" id="ReportManager_NBK_@resource.ID" name="ReportManager_NBK" value="@resource.User.NBK" placeholder="Enter NBK or Email..." />
                                            <input type="text" class="username-display cgFilter" id="ReportManager_Username_@resource.ID" disabled value="@resource.User.Username" />
                                        </div>
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "ReportManager_AssigneeName_" + resource.ID })
                                    }
                                    else
                                    {
                                        <input type="text" class="assignee-input cgFilter" id="ReportManager_AssigneeName_@resource.ID" name="ReportManager_AssigneeName" value="@resource.AssigneeName" />
                                    }
                                </td>
                                <td>
                                    <input type="checkbox" class="table-checkbox" id="ReportManager_ActiveFlag_@resource.ID" name="ReportManager_ActiveFlag_@resource.ID" value="true" @(resource.ActiveFlag ? "checked" : "") />
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="row-action-btn save cgUpdateReportManager" id="btnUpdateReportManager_@resource.ID" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="row-action-btn reset cgResetReportManager" id="btnResetReportManager_@resource.ID" title="Reset">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        }

                        @if (Model.ReportManagers.Count == 0)
                        {
                            <tr>
                                <td colspan="4" style="text-align: center; padding: 32px; color: #666;">
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

            <!-- ==================== iDRIVES TAB ==================== -->
            <div id="idrive-content" class="tab-content @(Model.PageAction == ResourceManagementPageAction.paIDrive ? "active" : "")">
                <div class="table-header">
                    <span class="total-count">Total (@Model.iDrives.Count)</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by ID or Name">
                    </div>
                </div>

                <table class="data-table" id="tbliDrive">
                    <thead>
                        <tr>
                            <th style="width: 80px;">ID</th>
                            <th>User / Assignee</th>
                            <th style="width: 100px;">Active</th>
                            <th style="width: 120px;">Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        <!-- Add New Row -->
                        <tr class="add-new-row">
                            <td></td>
                            <td>
                                <div class="user-input-group">
                                    @Html.HiddenFor(m => m.TheiDrive.User_ID, new { @id = "iDrive_User_ID_0" })
                                    <input type="text" class="nbk-input" id="iDrive_NBK_0" name="iDrive_NBK" placeholder="Enter NBK or Email..." />
                                    <input type="text" class="username-display" id="iDrive_Username_0" disabled placeholder="Username" />
                                    <span class="or-separator">OR</span>
                                    @Html.TextBoxFor(m => m.TheiDrive.AssigneeName, new { @class = "assignee-input", @placeholder = "DG Group / Virtual Resource" })
                                </div>
                            </td>
                            <td>
                                @{ Model.TheiDrive.ActiveFlag = true; }
                                <input type="checkbox" class="table-checkbox" id="TheiDrive_ActiveFlag" name="TheiDrive.ActiveFlag" value="true" checked />
                            </td>
                            <td>
                                <button type="button" id="btnAddiDrive" class="btn-add" title="Add">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14m-7-7h14" /></svg>
                                    Add
                                </button>
                            </td>
                        </tr>

                        <!-- Existing Rows -->
                        @for (int i = 0; i < Model.iDrives.Count; i++)
                        {
                            var resource = Model.iDrives.ToArray()[i];
                            <tr>
                                <td>
                                    @Html.HiddenFor(m => resource.ID, new { @Id = "iDrive_ID_" + resource.ID })
                                    @resource.ID
                                </td>
                                <td>
                                    @Html.HiddenFor(m => resource.User_ID, new { @Id = "iDrive_User_ID_" + resource.ID })
                                    @if (resource.User_ID > 0)
                                    {
                                        <div class="user-input-group">
                                            <input type="text" class="nbk-input cgFilter" id="iDrive_NBK_@resource.ID" name="iDrive_NBK" value="@resource.User.NBK" placeholder="Enter NBK or Email..." />
                                            <input type="text" class="username-display cgFilter" id="iDrive_Username_@resource.ID" disabled value="@resource.User.Username" />
                                        </div>
                                        @Html.HiddenFor(m => resource.AssigneeName, new { @Id = "iDrive_AssigneeName_" + resource.ID })
                                    }
                                    else
                                    {
                                        <input type="text" class="assignee-input cgFilter" id="iDrive_AssigneeName_@resource.ID" name="iDrive_AssigneeName" value="@resource.AssigneeName" />
                                    }
                                </td>
                                <td>
                                    <input type="checkbox" class="table-checkbox" id="iDrive_ActiveFlag_@resource.ID" name="iDrive_ActiveFlag_@resource.ID" value="true" @(resource.ActiveFlag ? "checked" : "") />
                                </td>
                                <td>
                                    <div class="row-actions">
                                        <button type="button" class="row-action-btn save cgUpdateiDrive" id="btnUpdateiDrive_@resource.ID" title="Save">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M5 13l4 4L19 7" /></svg>
                                        </button>
                                        <button type="button" class="row-action-btn reset cgResetiDrive" id="btnResetiDrive_@resource.ID" title="Reset">
                                            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15" /></svg>
                                        </button>
                                    </div>
                                </td>
                            </tr>
                        }

                        @if (Model.iDrives.Count == 0)
                        {
                            <tr>
                                <td colspan="4" style="text-align: center; padding: 32px; color: #666;">
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

                // Visual tab switching for immediate feedback
                $('.tab').removeClass('active');
                $(this).addClass('active');
                var tabName = $(this).data('tab');
                $('.tab-content').removeClass('active');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function () {
                var searchTerm = $(this).val().toLowerCase();
                var $tabContent = $(this).closest('.tab-content');
                var $rows = $tabContent.find('.data-table tbody tr:not(.add-new-row)');
                var visibleCount = 0;

                $rows.each(function () {
                    var $row = $(this);
                    var id = $row.find('td:first').text().toLowerCase();
                    var nbk = $row.find('.nbk-input').val() ? $row.find('.nbk-input').val().toLowerCase() : '';
                    var username = $row.find('.username-display').val() ? $row.find('.username-display').val().toLowerCase() : '';
                    var assignee = $row.find('.assignee-input').val() ? $row.find('.assignee-input').val().toLowerCase() : '';

                    if (id.includes(searchTerm) || nbk.includes(searchTerm) || username.includes(searchTerm) || assignee.includes(searchTerm)) {
                        $row.show();
                        visibleCount++;
                    } else {
                        $row.hide();
                    }
                });

                // Update Count Text
                var $totalCount = $tabContent.find('.total-count');
                var totalRows = $rows.length;
                if (searchTerm === '') {
                    $totalCount.text('Total (' + totalRows + ')');
                } else {
                    $totalCount.text('Showing ' + visibleCount + ' of ' + totalRows);
                }
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
