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

//
@using ROV.ReportHub.ViewModels
@model ManageFieldMappingViewModel
@{
    ViewBag.Title = "Manage Field Mapping";
}
<link href="~/_css/styles.css" rel="stylesheet" />

<div class="container">
    <h1>Field Mapping</h1>

    @using (Html.BeginForm("ManageFieldMapping", "Division", FormMethod.Post, new { @id = "frmManageMapping" }))
    {
        @Html.AntiForgeryToken()

        <!-- Division Selector -->
        <div class="selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                @Html.DropDownListFor(m => m.SelectedDivisionID,
                    new SelectList(Model.Divisions, "ID", "CodeName"), 
                    "(Please Select)", 
                    new { @id = "SelectedDivisionID" })
            </div>
            <button type="button" class="btn btn-secondary" id="btnViewFields">View Fields</button>
        </div>

        @if (Model.SelectedDivisionID > 0)
        {
            string acLOBMapping = "";
            string inLOBMapping = "";

            switch (Model.PageAction)
            {
                case FieldMappingManagementrPageAction.paLOBMapping:
                    acLOBMapping = "active";
                    inLOBMapping = "in";
                    break;
                case FieldMappingManagementrPageAction.paStackholders:
                    break;
                case FieldMappingManagementrPageAction.paCustomFields:
                    break;
                default:
                    break;
            }

            <!-- Tabs -->
            <div class="tabs">
                <div class="tab @(acLOBMapping == "active" ? "active" : "")" data-tab="lob-mapping">
                    <a data-toggle="tab" href="#divLOBMapping">LOB Mapping</a>
                </div>
            </div>

            <!-- Tab Content -->
            <div class="tab-content">
                <div id="divLOBMapping" class="tab-pane fade @acLOBMapping @inLOBMapping">

                    <!-- LOB Selector -->
                    <div class="selector-row">
                        <label>LOB</label>
                        <div class="select-wrapper">
                            @if (Model.SelectedDivisionID.HasValue)
                            {
                                @Html.DropDownListFor(m => m.SelectedLOBID, 
                                    new SelectList(Model.LOBs.FindAll(r => r.ActiveFlag == true), "ID", "AlternateName"), 
                                    "(Please Select)", 
                                    new { @id = "SelectedLOBID" })
                            }
                        </div>
                    </div>

                    @if (Model.SelectedLOBID > 0)
                    {
                        @Html.HiddenFor(m => m.TheLOB.ID)

                        <!-- Report Information Section -->
                        <div class="section-header">
                            <span class="section-title">Report Information</span>
                            <div class="header-actions">
                                <button type="button" class="btn btn-outline" id="edit-details-btn">Edit Details</button>
                                <button type="button" class="btn btn-save hidden" id="save-details-btn">Save Changes</button>
                                <button type="button" class="btn btn-cancel hidden" id="cancel-details-btn">Cancel</button>
                            </div>
                        </div>

                        <!-- Details Grid - 6 columns -->
                        <div class="details-grid" id="details-grid-container">
                            <!-- LOB Group -->
                            <div class="detail-item" data-field="lobGroup">
                                <span class="detail-label">LOB Group</span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOB.LOBGroup_ID == null ? "empty" : "")">
                                        @(Model.TheLOB.LOBGroup_ID != null ? Model.LOBGroups.FirstOrDefault(x => x.ID == Model.TheLOB.LOBGroup_ID)?.Name : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOB.LOBGroup_ID, 
                                        new SelectList(Model.LOBGroups, "ID", "Name", Model.TheLOB.LOBGroup_ID), 
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>

                            <!-- Reporting Org -->
                            <div class="detail-item" data-field="reportingOrg">
                                <span class="detail-label">
                                    Reporting Org
                                    <span class="info-icon" data-toggle="popover" data-trigger="hover" 
                                          data-content="For adding a new Reporting Org., please contact site owner" 
                                          title="">ⓘ</span>
                                </span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOB.ReportingOrg_ID == null ? "empty" : "")">
                                        @(Model.TheLOB.ReportingOrg_ID != null ? Model.ReportingOrgs.FirstOrDefault(x => x.ID == Model.TheLOB.ReportingOrg_ID)?.CodeName : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOB.ReportingOrg_ID, 
                                        new SelectList(Model.ReportingOrgs, "ID", "CodeName", Model.TheLOB.ReportingOrg_ID),
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>

                            <!-- Report Manager -->
                            <div class="detail-item" data-field="reportManager">
                                <span class="detail-label">Report Manager</span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOBReportManager == null || Model.TheLOBReportManager.ID == 0 ? "empty" : "")">
                                        @(Model.TheLOBReportManager != null && Model.TheLOBReportManager.ID > 0 ? Model.ReportManagers.FirstOrDefault(x => x.ID == Model.TheLOBReportManager.ID)?.Name : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOBReportManager.ID,
                                        new SelectList(Model.ReportManagers, "ID", "Name", Model.TheLOBReportManager != null ? Model.TheLOBReportManager.ID : 0), 
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>

                            <!-- Report Lead -->
                            <div class="detail-item" data-field="reportLead">
                                <span class="detail-label">Report Lead</span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOBReportLead == null || Model.TheLOBReportLead.ID == 0 ? "empty" : "")">
                                        @(Model.TheLOBReportLead != null && Model.TheLOBReportLead.ID > 0 ? Model.ReportLeads.FirstOrDefault(x => x.ID == Model.TheLOBReportLead.ID)?.Name : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOBReportLead.ID,
                                        new SelectList(Model.ReportLeads, "ID", "Name", Model.TheLOBReportLead != null ? Model.TheLOBReportLead.ID : 0), 
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>

                            <!-- iDrive -->
                            <div class="detail-item" data-field="iDrive">
                                <span class="detail-label">iDrive</span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOBiDrive == null || Model.TheLOBiDrive.ID == 0 ? "empty" : "")">
                                        @(Model.TheLOBiDrive != null && Model.TheLOBiDrive.ID > 0 ? Model.iDrives.FirstOrDefault(x => x.ID == Model.TheLOBiDrive.ID)?.Name : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOBiDrive.ID,
                                        new SelectList(Model.iDrives, "ID", "Name", Model.TheLOBiDrive != null ? Model.TheLOBiDrive.ID : 0), 
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>

                            <!-- Executive -->
                            <div class="detail-item" data-field="executive">
                                <span class="detail-label">Executive</span>
                                <div class="detail-value-box">
                                    <span class="display-value @(Model.TheLOBExecutive == null || Model.TheLOBExecutive.ID == 0 ? "empty" : "")">
                                        @(Model.TheLOBExecutive != null && Model.TheLOBExecutive.ID > 0 ? Model.Executives.FirstOrDefault(x => x.ID == Model.TheLOBExecutive.ID)?.Name : "(Please Select)")
                                    </span>
                                    @Html.DropDownListFor(m => m.TheLOBExecutive.ID,
                                        new SelectList(Model.Executives, "ID", "Name", Model.TheLOBExecutive != null ? Model.TheLOBExecutive.ID : 0), 
                                        "(Please Select)", 
                                        new { @class = "edit-select hidden" })
                                </div>
                            </div>
                        </div>

                        <!-- LOB Details Section -->
                        <div class="lob-details-section">
                            <label class="lob-details-label">LOB Details</label>
                            <div class="lob-details-container">
                                <!-- Filter Input -->
                                <div class="lob-filter-wrapper">
                                    <input type="text" 
                                           class="lob-filter-input" 
                                           id="txtLOBDetailFilter" 
                                           name="txtLOBDetailFilter" 
                                           placeholder="Start typing to filter on LOB Details..." />
                                </div>

                                <!-- Checkbox Grid -->
                                <div class="lob-checkbox-grid" id="divLOBDetailContainer">
                                    @{
                                        List<ROV.Common.DataModel.LOBDetail> dtlList = new List<ROV.Common.DataModel.LOBDetail>();
                                        foreach (var item in Model.TheLOB.LOBDetails)
                                        {
                                            dtlList.Add(item);
                                        }
                                        foreach (var item in Model.LOBDetails.OrderByDescending(r => r.ActiveFlag).ToList())
                                        {
                                            if ((item.ActiveFlag) && (!dtlList.Any(r => r.ID == item.ID)))
                                            {
                                                dtlList.Add(item);
                                            }
                                        }
                                    }

                                    @foreach (var lobDtl in dtlList)
                                    {
                                        bool isChecked = Model.TheLOB.LOBDetails.SingleOrDefault(r => r.ID == lobDtl.ID) != null;
                                        bool isInactive = !lobDtl.ActiveFlag && !isChecked;
                                        
                                        <div class="checkbox-item" data-label="@lobDtl.Name.ToLower()">
                                            <label class="checkbox-label">
                                                <input type="checkbox" 
                                                       class="checkbox-input"
                                                       id="cbx_LOBDetail_@lobDtl.ID" 
                                                       name="cbx_LOBDetail_@lobDtl.ID" 
                                                       value="@lobDtl.ID" 
                                                       @(isChecked ? "checked" : "") 
                                                       @(isInactive ? "data-inactive=true" : "")
                                                       disabled />
                                                <span class="checkbox-custom"></span>
                                                <span class="checkbox-text">@lobDtl.Name</span>
                                            </label>
                                        </div>
                                    }
                                </div>
                            </div>
                        </div>
                    }
                </div>
            </div>
        }
    }
</div>

@section Scripts
{
    <script>
        $(document).ready(function () {
            // Initialize popovers
            $('[data-toggle="popover"]').popover();

            // Store original values
            var originalValues = {};
            var originalLobSelections = {};
            var isEditMode = false;

            // Cache selectors
            var $editBtn = $('#edit-details-btn');
            var $saveBtn = $('#save-details-btn');
            var $cancelBtn = $('#cancel-details-btn');
            var $gridContainer = $('#details-grid-container');
            var $lobDetailsSection = $('.lob-details-section');

            // ==========================================
            // DIVISION DROPDOWN CHANGE
            // ==========================================
            $('#SelectedDivisionID').change(function () {
                $('#frmManageMapping').submit();
            });

            // ==========================================
            // LOB DROPDOWN CHANGE
            // ==========================================
            $('#SelectedLOBID').change(function () {
                $('#frmManageMapping').submit();
            });

            // ==========================================
            // LOB DETAILS FILTER
            // ==========================================
            $('#txtLOBDetailFilter').on('input', function () {
                var filterText = $(this).val().toLowerCase();

                $('.checkbox-item').each(function () {
                    var labelText = $(this).data('label');
                    if (labelText.indexOf(filterText) !== -1) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });
            });

            // ==========================================
            // EDIT MODE
            // ==========================================
            $editBtn.on('click', function () {
                isEditMode = true;

                // Toggle Buttons
                $editBtn.addClass('hidden');
                $saveBtn.removeClass('hidden');
                $cancelBtn.removeClass('hidden');

                // Save original values and show dropdowns
                $gridContainer.find('.detail-item').each(function () {
                    var $item = $(this);
                    var field = $item.data('field');
                    var $valueBox = $item.find('.detail-value-box');
                    var $displayValue = $valueBox.find('.display-value');
                    var $select = $valueBox.find('.edit-select');

                    // Store original selected value
                    originalValues[field] = $select.val();

                    // Hide display value, show select
                    $displayValue.addClass('hidden');
                    $select.removeClass('hidden');
                });

                // Save original LOB selections and enable checkboxes
                $('.checkbox-input').each(function () {
                    var id = $(this).attr('id');
                    originalLobSelections[id] = $(this).is(':checked');
                    
                    // Only enable if not inactive
                    if (!$(this).data('inactive')) {
                        $(this).prop('disabled', false);
                    }
                });

                $lobDetailsSection.addClass('edit-mode');
            });

            // ==========================================
            // SAVE CHANGES
            // ==========================================
            $saveBtn.on('click', function () {
                // Submit the form to save changes
                $('#frmManageMapping').submit();
            });

            // ==========================================
            // CANCEL CHANGES
            // ==========================================
            $cancelBtn.on('click', function () {
                isEditMode = false;

                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // Restore original values and hide dropdowns
                $gridContainer.find('.detail-item').each(function () {
                    var $item = $(this);
                    var field = $item.data('field');
                    var $valueBox = $item.find('.detail-value-box');
                    var $displayValue = $valueBox.find('.display-value');
                    var $select = $valueBox.find('.edit-select');

                    // Restore original selected value
                    $select.val(originalValues[field]);

                    // Show display value, hide select
                    $displayValue.removeClass('hidden');
                    $select.addClass('hidden');
                });

                // Restore LOB selections and disable checkboxes
                $('.checkbox-input').each(function () {
                    var id = $(this).attr('id');
                    $(this).prop('checked', originalLobSelections[id]);
                    $(this).prop('disabled', true);
                });

                $lobDetailsSection.removeClass('edit-mode');
            });
        });
    </script>
}




//
/* ============================================
   RESET & BASICS
   ============================================ */
* {
    margin: 0;
    padding: 0;
    box-sizing: border-box;
}

body {
    font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
    background-color: #f5f5f5;
    color: #333;
    line-height: 1.5;
    padding: 20px;
}

/* ============================================
   LAYOUT & CONTAINER
   ============================================ */
.container {
    max-width: 1400px;
    margin: 0 auto;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    padding: 24px 32px;
}

/* Variant for Resources and Field Values pages */
.container.layout-flex {
    min-height: 80vh;
    display: flex;
    flex-direction: column;
}

h1 {
    font-size: 20px;
    font-weight: 600;
    margin-bottom: 24px;
    color: #1a1a1a;
}

/* ============================================
   SELECTORS & INPUTS
   ============================================ */
/* Standard Selector Row (used in most files) */
.selector-row,
.division-selector {
    display: flex;
    align-items: center;
    gap: 16px;
    margin-bottom: 24px;
}

.selector-row label,
.division-selector label {
    font-size: 14px;
    color: #666;
    font-weight: 500;
}

.select-wrapper {
    position: relative;
    flex: 1;
    max-width: 400px;
}

/* Field Values has a slightly wider max-width */
.container.layout-flex .select-wrapper {
    max-width: 500px;
}

.select-wrapper select {
    width: 100%;
    padding: 10px 40px 10px 14px;
    font-size: 14px;
    border: 1px solid #ddd;
    border-radius: 4px;
    background: #fff;
    appearance: none;
    cursor: pointer;
    color: #333;
}

.select-wrapper::after {
    content: '';
    position: absolute;
    right: 14px;
    top: 50%;
    transform: translateY(-50%);
    width: 0;
    height: 0;
    border-left: 5px solid transparent;
    border-right: 5px solid transparent;
    border-top: 5px solid #666;
    pointer-events: none;
}

/* Inline Inputs (Resources, Field Values) */
.inline-input {
    padding: 6px 10px;
    font-size: 14px;
    border: 1px solid #ccc;
    border-radius: 4px;
    width: 100%;
    min-width: 60px;
}

td .inline-input {
    width: 90%;
    min-width: 80px;
}

.inline-input:focus {
    outline: none;
    border-color: #0049ac;
    box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.2);
}

.inline-input.wide {
    max-width: 300px;
}

/* ============================================
   BUTTONS
   ============================================ */
.btn {
    padding: 8px 16px;
    font-size: 14px;
    border-radius: 4px;
    cursor: pointer;
    font-weight: 500;
    transition: all 0.2s;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 6px;
}

/* Larger padding for some pages */
.container.layout-flex .btn {
    padding: 10px 20px;
}

.btn-secondary {
    background: #6b7280;
    color: #fff;
    border: none;
}

.btn-secondary:hover {
    background: #5a6170;
}

.btn-primary {
    background: #1e3a5f;
    color: #fff;
    border: none;
}

.btn-primary:hover {
    background: #162d4a;
}

.btn-outline {
    background: #fff;
    color: #0049ac;
    border: 1px solid #0049ac;
}

.btn-outline:hover {
    background: #f0f7ff;
}

.btn-save {
    background: #1e3a5f;
    color: #fff;
    border: 1px solid #1e3a5f;
}

.btn-save:hover {
    background: #162d4a;
}

.btn-cancel {
    background: #fff;
    color: #c53030;
    border: 1px solid #c53030;
}

.btn-cancel:hover {
    background: #fff5f5;
}

/* Action/Icon Buttons */
.action-buttons {
    display: flex;
    gap: 24px;
    justify-content: flex-end;
    align-items: center;
}

/* Variant gap for Resources/Field Values */
.container.layout-flex .action-buttons {
    gap: 28px;
}

.action-btn {
    width: 22px;
    height: 22px;
    border-radius: 50%;
    border: 2px solid;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s;
    background: transparent;
    padding: 0;
}

/* Slightly larger action btn for Resources/Field Values */
.container.layout-flex .action-btn {
    width: 24px;
    height: 24px;
}

.action-btn.approve {
    border-color: #1e3a5f;
    color: #1e3a5f;
}

.action-btn.approve:hover {
    background: #1e3a5f;
    color: #fff;
}

.action-btn.reject {
    border-color: #9b2c2c;
    color: #9b2c2c;
}

.action-btn.reject:hover {
    background: #9b2c2c;
    color: #fff;
}

.action-btn svg {
    width: 11px;
    height: 11px;
    stroke-width: 3;
}

.container.layout-flex .action-btn svg {
    width: 12px;
    height: 12px;
}

.icon-btn {
    background: none;
    border: none;
    cursor: pointer;
    color: #333;
    padding: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: color 0.2s;
}

.icon-btn:hover {
    color: #1e3a5f;
}

.icon-btn svg {
    width: 20px;
    height: 20px;
}

/* ============================================
   TABS
   ============================================ */
.tabs {
    display: flex;
    margin-bottom: 24px;
    position: relative;
    border-bottom: 1px solid #c5c5c5;
    flex-wrap: wrap; /* Added for responsiveness */
}

.tab {
    padding: 14px 40px;
    font-size: 15px;
    color: #1e3a5f;
    cursor: pointer;
    transition: all 0.2s;
    position: relative;
    background: transparent;
    font-weight: 500;
    margin-bottom: -1px;
}

/* Variant padding for Field Values */
.container.layout-flex .tab {
    padding: 14px 30px; 
}
   
.tab:hover {
    color: #1e3a5f;
}

.tab.active {
    color: #1a1a1a;
    background: #fff;
    border-left: 1px solid #c5c5c5;
    border-right: 1px solid #c5c5c5;
    border-top: 1px solid #c5c5c5;
    border-bottom: 1px solid #fff;
}

.tab.active::before {
    content: '';
    position: absolute;
    top: -1px;
    left: -1px;
    right: -1px;
    height: 3px;
    background: #1e3a5f;
}

.tab-content {
    display: none;
}

.tab-content.active {
    display: block;
}

.container.layout-flex .tab-content.active {
    display: flex;
    flex-direction: column;
    flex: 1;
}

/* ============================================
   TABLES & LISTS
   ============================================ */
.table-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 16px;
}

.total-count {
    font-size: 14px;
    color: #666;
}

.search-box {
    position: relative;
}

.search-box input {
    padding: 8px 14px 8px 36px;
    font-size: 13px;
    border: 1px solid #ddd;
    border-radius: 4px;
    width: 240px;
    background: #fff;
}

.search-box input:focus {
    outline: none;
    border-color: #999;
}

.search-box::before {
    content: '';
    position: absolute;
    left: 12px;
    top: 50%;
    transform: translateY(-50%);
    width: 14px;
    height: 14px;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23999'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z'/%3E%3C/svg%3E");
    background-size: contain;
}

.data-table {
    width: 100%;
    border-collapse: collapse;
}

.data-table th {
    text-align: left;
    padding: 12px 16px;
    font-size: 13px;
    font-weight: 500;
    color: #666;
    border-bottom: 1px solid #e5e5e5;
    white-space: nowrap;
}

.data-table td {
    padding: 16px;
    font-size: 14px;
    border-bottom: 1px solid #f0f0f0;
    color: #333;
}

/* Slightly tighter padding for Field Values */
.container.layout-flex .data-table td {
    padding: 12px 16px;
    vertical-align: middle;
}

.data-table tbody tr:nth-child(odd) {
    background: #f7f7f8;
}

.data-table tbody tr:nth-child(even) {
    background: #fff;
}

.data-table tbody tr:hover {
    background: #e6fcff;
}

.data-table tbody tr.editing,
.data-table tbody tr.editing:hover {
    background: #fffde7 !important;
}

/* Sort Icons */
.data-table th .sort-icon {
    display: inline-flex;
    flex-direction: column;
    margin-left: 6px;
    vertical-align: middle;
    gap: 2px;
}

.data-table th .sort-icon span {
    display: block;
    width: 0;
    height: 0;
    border-left: 4px solid transparent;
    border-right: 4px solid transparent;
}

/* Different sort icon styles across files - unifying to standard */
.data-table th .sort-icon .up {
    border-bottom: 4px solid #999;
}
.data-table th .sort-icon .down {
    border-top: 4px solid #999;
}

/* Resources/Field Values use slightly different sort icon style */
.container.layout-flex .data-table th .sort-icon .up {
    border-bottom: 6px solid #1e3a5f;
}
.container.layout-flex .data-table th .sort-icon .down {
    border-top: 6px solid #1e3a5f;
}

/* ============================================
   PAGINATION
   ============================================ */
.pagination-wrapper {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-top: 24px;
    padding-top: 16px;
}

/* Push to bottom for flex layouts */
.container.layout-flex .pagination-wrapper {
    margin-top: auto;
}

.showing-text {
    font-size: 13px;
    color: #666;
}

.pagination {
    display: flex;
    align-items: center;
    gap: 4px;
}

.pagination button {
    width: 32px;
    height: 32px;
    border: none;
    background: transparent;
    color: #666;
    cursor: pointer;
    font-size: 13px;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
}

.pagination button:hover:not(.active):not(:disabled) {
    background: #f0f0f0;
}

.pagination button.active {
    background: #2563eb;
    color: #fff;
}

.pagination button:disabled {
    color: #ccc;
    cursor: not-allowed;
}

.pagination .dots {
    padding: 0 4px;
    color: #666;
}

.per-page {
    display: flex;
    align-items: center;
    gap: 8px;
    font-size: 13px;
    color: #666;
}

.per-page select {
    padding: 6px 28px 6px 10px;
    font-size: 13px;
    border: 1px solid #ddd;
    border-radius: 4px;
    background: #fff;
    appearance: none;
    cursor: pointer;
    background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23666'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'/%3E%3C/svg%3E");
    background-repeat: no-repeat;
    background-position: right 8px center;
    background-size: 14px;
}

/* ============================================
   STATUS BADGES
   ============================================ */
.status-badge {
    display: inline-block;
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 500;
    border-radius: 4px;
}

.status-active {
    background: #4a7c59;
    color: #fff;
}
/* Text-only status variant (Resources/Field Values) */
.container.layout-flex .status-active {
    background: transparent;
    color: #4a7c59;
    padding: 0;
}

.status-inactive,
.status-rejected {
    background: #c53030;
    color: #fff;
}
/* Text-only status variant */
.container.layout-flex .status-inactive {
    background: transparent;
    color: #c53030;
    padding: 0;
}

.status-approved {
    background: #4a7c59;
    color: #fff;
}

/* ============================================
   SPECIFIC COMPONENTS (Manage Division / Details Cards)
   ============================================ */
/* Info Card & Section Header */
.division-card,
.info-card,
.section-header {
    display: flex;
    align-items: center;
    justify-content: space-between;
    padding: 16px 20px;
    background: #fafafa;
    border-radius: 6px;
    margin-bottom: 24px;
}

.division-info,
.info-card-left {
    display: flex;
    align-items: center;
    gap: 12px;
}

.division-name,
.info-card-title,
.section-title {
    font-size: 15px;
    font-weight: 600;
    color: #1a1a1a;
}

.division-actions,
.header-actions {
    display: flex;
    gap: 8px;
}

.division-name-input {
    font-size: 15px;
    font-weight: 600;
    color: #1a1a1a;
    padding: 6px 12px;
    border: 1px solid #0049ac;
    border-radius: 4px;
    outline: none;
    min-width: 150px;
}

.division-name-input:focus {
    box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.2);
}

/* ============================================
   DETAILS GRID - 6 Columns with Input Box Styling
   ============================================ */
.details-grid {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 20px;
    padding: 24px 0;
    border-bottom: 1px solid #eee;
}

.detail-item {
    display: flex;
    flex-direction: column;
    gap: 8px;
}

.detail-label {
    font-size: 14px;
    color: #333;
    font-weight: 600;
    display: flex;
    align-items: center;
    gap: 6px;
}

.info-icon {
    color: #0049ac;
    cursor: help;
    font-size: 14px;
    font-weight: normal;
}

/* Value Box - Input-like appearance - FIXED WIDTH */
.detail-value-box {
    position: relative;
    background: #f8f9fa;
    border: 1px solid #e5e7eb;
    border-radius: 4px;
    height: 42px;
    display: flex;
    align-items: center;
}

.display-value {
    font-size: 14px;
    color: #333;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
    padding: 0 14px;
    width: 100%;
}

.display-value.empty {
    color: #6b7280;
}

/* Edit Select Wrapper - Same size as display box */
.edit-select-wrapper {
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    width: 100%;
    height: 100%;
}

.edit-select-wrapper select {
    width: 100%;
    height: 100%;
    padding: 0 36px 0 14px;
    font-size: 14px;
    border: 1px solid #1e3a5f;
    border-radius: 4px;
    background: #fff;
    appearance: none;
    -webkit-appearance: none;
    -moz-appearance: none;
    cursor: pointer;
    color: #333;
    outline: none;
}

.edit-select-wrapper select:focus {
    border-color: #0049ac;
    box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.15);
}

/* Dropdown Arrow - Consistent styling */
.edit-select-wrapper::after {
    content: '';
    position: absolute;
    right: 12px;
    top: 50%;
    transform: translateY(-50%);
    width: 0;
    height: 0;
    border-left: 5px solid transparent;
    border-right: 5px solid transparent;
    border-top: 6px solid #1e3a5f;
    pointer-events: none;
}

/* ============================================
   LOB DETAILS SECTION
   ============================================ */
.lob-details-section {
    margin-top: 24px;
    padding-top: 0;
}

.lob-details-label {
    font-size: 14px;
    color: #333;
    font-weight: 600;
    margin-bottom: 12px;
    display: block;
}

.lob-details-container {
    border: 1px solid #d1d5db;
    border-radius: 6px;
    overflow: hidden;
    background: #fff;
}

/* Filter Input */
.lob-filter-wrapper {
    padding: 0;
    background: #f3f4f6;
    border-bottom: 1px solid #d1d5db;
}

.lob-filter-input {
    width: 100%;
    padding: 14px 16px;
    font-size: 14px;
    border: none;
    background: transparent;
    color: #333;
    outline: none;
}

.lob-filter-input::placeholder {
    color: #6b7280;
}

.lob-filter-input:focus {
    background: #fff;
}

/* Checkbox Grid */
.lob-checkbox-grid {
    display: grid;
    grid-template-columns: repeat(3, 1fr);
    gap: 0;
    max-height: 350px;
    overflow-y: auto;
    padding: 16px;
}

.checkbox-item {
    padding: 10px 8px;
    display: flex;
    align-items: flex-start;
}

/* Alternating row backgrounds - every 3 items is a row */
.checkbox-item:nth-child(6n+1),
.checkbox-item:nth-child(6n+2),
.checkbox-item:nth-child(6n+3) {
    background: #fff;
}

.checkbox-item:nth-child(6n+4),
.checkbox-item:nth-child(6n+5),
.checkbox-item:nth-child(6n+6) {
    background: #f9fafb;
}

.checkbox-label {
    display: flex;
    align-items: flex-start;
    gap: 10px;
    cursor: pointer;
    font-size: 14px;
    color: #333;
    line-height: 1.4;
}

/* Disabled state for checkboxes */
.checkbox-label input[type="checkbox"]:disabled + .checkbox-custom {
    opacity: 0.6;
    cursor: not-allowed;
}

.checkbox-label input[type="checkbox"]:disabled ~ .checkbox-text {
    opacity: 0.7;
}

/* When in edit mode, restore full opacity */
.lob-details-section.edit-mode .checkbox-label input[type="checkbox"]:disabled + .checkbox-custom,
.lob-details-section.edit-mode .checkbox-label input[type="checkbox"]:disabled ~ .checkbox-text {
    opacity: 1;
}

.checkbox-label input[type="checkbox"] {
    display: none;
}

.checkbox-custom {
    width: 18px;
    height: 18px;
    min-width: 18px;
    border: 2px solid #9ca3af;
    border-radius: 3px;
    background: #fff;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s;
    margin-top: 1px;
}

.checkbox-label input[type="checkbox"]:checked + .checkbox-custom {
    background: #0049ac;
    border-color: #0049ac;
}

.checkbox-label input[type="checkbox"]:checked + .checkbox-custom::after {
    content: '';
    width: 5px;
    height: 9px;
    border: solid #fff;
    border-width: 0 2px 2px 0;
    transform: rotate(45deg);
    margin-bottom: 2px;
}

.checkbox-text {
    flex: 1;
    word-break: break-word;
}

/* Scrollbar Styling */
.lob-checkbox-grid::-webkit-scrollbar {
    width: 8px;
}

.lob-checkbox-grid::-webkit-scrollbar-track {
    background: #f1f1f1;
    border-radius: 4px;
}

.lob-checkbox-grid::-webkit-scrollbar-thumb {
    background: #c1c1c1;
    border-radius: 4px;
}

.lob-checkbox-grid::-webkit-scrollbar-thumb:hover {
    background: #a1a1a1;
}

/* Utilities */
.nbk-link {
    color: #2563eb;
    text-decoration: none;
    font-weight: 500;
}

.nbk-link:hover {
    text-decoration: underline;
}

.email-text {
    color: #666;
}

.hidden {
    display: none !important;
}

/* ============================================
   RESPONSIVE
   ============================================ */
@media (max-width: 1400px) {
    .details-grid {
        grid-template-columns: repeat(3, 1fr);
    }
}

@media (max-width: 1200px) {
    .lob-checkbox-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 1024px) {
    .data-table {
        display: block;
        overflow-x: auto;
    }
}

@media (max-width: 900px) {
    .details-grid {
        grid-template-columns: repeat(2, 1fr);
    }
}

@media (max-width: 768px) {
    .container {
        padding: 16px;
    }

    .selector-row,
    .division-selector {
        flex-wrap: wrap;
    }

    .select-wrapper {
        max-width: 100%;
        width: 100%;
    }

    .pagination-wrapper {
        flex-direction: column;
        gap: 16px;
    }

    .tabs {
        flex-wrap: wrap;
    }

    .tab {
        padding: 12px 20px;
        font-size: 14px;
    }
    
    .division-card,
    .info-card,
    .section-header {
        flex-direction: column;
        gap: 12px;
        align-items: flex-start;
    }
    
    .header-actions {
        width: 100%;
    }
    
    .table-header {
        flex-direction: column;
        gap: 12px;
        align-items: flex-start;
    }
    
    .lob-checkbox-grid {
        grid-template-columns: 1fr;
    }
}

@media (max-width: 480px) {
    .details-grid {
        grid-template-columns: 1fr;
    }
}
