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

@using System.Web.Mvc.Html
@using System.Collections.Generic

@{
    Layout = null;

    // ==========================================================================
    // MOCK VIEW MODEL (Simulating Server-Side Data)
    // ==========================================================================
    
    // Dropdown Lists
    var divisionList = new List<SelectListItem>
    {
        new SelectListItem { Text = "DWO (ETS)", Value = "DWO", Selected = true },
        new SelectListItem { Text = "Division 2", Value = "DIV2" },
        new SelectListItem { Text = "Division 3", Value = "DIV3" }
    };

    var lobList = new List<SelectListItem>
    {
        new SelectListItem { Text = "All DSRA Access Request", Value = "DSRA", Selected = true },
        new SelectListItem { Text = "LOB Option 2", Value = "LOB2" },
        new SelectListItem { Text = "LOB Option 3", Value = "LOB3" }
    };

    // Report Details Data
    var reportDetails = new Dictionary<string, string>
    {
        { "LOB Group", "ETS Issues Report" },
        { "Reporting Organisation", "RAM Reporting Org" },
        { "Reporting Manager", "RAM Reporting Org" },
        { "Reporting Lead", "RAM Reporting Org" },
        { "iDrive", "RAM Reporting Org" },
        { "Executive", "RAM Reporting Org" }
    };

    // Description Data
    var dataReadyLogicDescription = "Data Ready Logic is a crucial component in digital systems, responsible for signaling when valid data is available for processing or transfer. It ensures data integrity and prevents race conditions by synchronizing data flow.";
}

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Field Mapping</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <div class="container">
        <h1>Field Mapping</h1>

        <!-- Division Selector -->
        <div class="selector-row">
            @Html.Label("SelectedDivision", "Division")
            <div class="select-wrapper">
                @Html.DropDownList("SelectedDivision", divisionList, new { @id = "divisionDropdown" })
            </div>
            <button type="button" class="btn btn-secondary">View Fields</button>
        </div>

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="lob-mapping">LOB Mapping</div>
        </div>

        <!-- LOB Mapping Tab Content -->
        <div id="lob-mapping-content" class="tab-content active">
            <!-- LOB Selector -->
            <div class="selector-row">
                @Html.Label("SelectedLOB", "LOB")
                <div class="select-wrapper">
                    @Html.DropDownList("SelectedLOB", lobList, new { @id = "lobDropdown" })
                </div>
            </div>

            <!-- Form Start -->
            @using (Html.BeginForm("Save", "FieldMapping", FormMethod.Post, new { id = "mappingForm" }))
            {
                @Html.AntiForgeryToken()

                <!-- Report Information Section -->
                <div class="section-header">
                    <span class="section-title">Report Information</span>
                    <div class="header-actions">
                        <button type="button" class="btn btn-outline" id="edit-details-btn">Edit Details</button>
                        <button type="submit" class="btn btn-save hidden" id="save-details-btn">Save Changes</button>
                        <button type="button" class="btn btn-cancel hidden" id="cancel-details-btn">Cancel</button>
                    </div>
                </div>

                <!-- Details Grid -->
                <div class="details-grid bordered" id="details-grid-container">
                    @foreach (var item in reportDetails)
                    {
                        <div class="detail-item">
                            <span class="detail-label">@item.Key</span>
                            <span class="detail-value">@item.Value</span>
                        </div>
                    }
                </div>

                <!-- Data Ready Logic Section -->
                <div class="description-section">
                    <div class="description-label">Data Ready Logic</div>
                    <p class="description-text" id="logic-description">
                        @dataReadyLogicDescription
                    </p>
                </div>
            }
            <!-- Form End -->
        </div>

    </div>

    <script>
        $(document).ready(function() {
            // Configuration for Dropdown Options (Client-side logic for Edit mode)
            const dropdownOptions = {
                "LOB Group": ["ETS Issues Report", "LOB Group A", "LOB Group B"],
                "Reporting Organisation": ["RAM Reporting Org", "Finance Org", "Operations Org"],
                "Reporting Manager": ["RAM Reporting Org", "Manager A", "Manager B"],
                "Reporting Lead": ["RAM Reporting Org", "Lead A", "Lead B"],
                "iDrive": ["RAM Reporting Org", "iDrive A", "iDrive B"],
                "Executive": ["RAM Reporting Org", "Executive A", "Executive B"]
            };

            // Store original state
            let originalValues = [];
            let originalDescription = "";

            // Cache selectors
            const $editBtn = $('#edit-details-btn');
            const $saveBtn = $('#save-details-btn');
            const $cancelBtn = $('#cancel-details-btn');
            const $gridContainer = $('#details-grid-container');
            const $descText = $('#logic-description');

            // ==========================================
            // TAB SWITCHING
            // ==========================================
            $('.tab').on('click', function() {
                $('.tab').removeClass('active');
                $(this).addClass('active');
                
                $('.tab-content').removeClass('active');
                var tabName = $(this).data('tab');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // EDIT MODE
            // ==========================================
            $editBtn.on('click', function() {
                // Toggle Buttons
                $editBtn.addClass('hidden');
                $saveBtn.removeClass('hidden');
                $cancelBtn.removeClass('hidden');

                // 1. Handle Grid Items (Convert to Select wrapped in Div)
                originalValues = [];
                
                $gridContainer.find('.detail-item').each(function() {
                    const $item = $(this);
                    const label = $item.find('.detail-label').text().trim();
                    const $valueEl = $item.find('.detail-value');
                    const currentText = $valueEl.text().trim();
                    
                    originalValues.push(currentText); // Save for cancel
                    
                    // Create Wrapper
                    const $wrapper = $('<div class="edit-select-wrapper"></div>');
                    const $select = $('<select></select>');
                    
                    // Get options or default
                    const options = dropdownOptions[label] || [currentText, "Option 1", "Option 2"];
                    
                    // Populate options
                    $.each(options, function(i, opt) {
                        const $option = $('<option></option>').val(opt).text(opt);
                        if(opt === currentText) $option.prop('selected', true);
                        $select.append($option);
                    });
                    
                    // Ensure current value is in list if custom
                    if ($.inArray(currentText, options) === -1 && currentText !== "") {
                         const $option = $('<option></option>').val(currentText).text(currentText).prop('selected', true);
                         $select.prepend($option);
                    }

                    $wrapper.append($select);
                    $valueEl.empty().append($wrapper);
                });

                // 2. Handle Description (Convert to Textarea)
                originalDescription = $descText.text().trim();
                const $textarea = $('<textarea class="edit-textarea"></textarea>').val(originalDescription);
                $descText.empty().append($textarea);
            });

            // ==========================================
            // SAVE CHANGES
            // ==========================================
            $saveBtn.on('click', function(e) {
                // Prevent form submission for this mock demo
                e.preventDefault(); 
                
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // 1. Save Grid Items
                $gridContainer.find('.detail-value').each(function() {
                    const $el = $(this);
                    const $select = $el.find('select');
                    if ($select.length) {
                        const newValue = $select.val();
                        $el.text(newValue);
                    }
                });

                // 2. Save Description
                const $textarea = $descText.find('textarea');
                if ($textarea.length) {
                    $descText.text($textarea.val());
                }
            });

            // ==========================================
            // CANCEL CHANGES
            // ==========================================
            $cancelBtn.on('click', function() {
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // 1. Restore Grid Items
                $gridContainer.find('.detail-value').each(function(index) {
                    $(this).text(originalValues[index]);
                });

                // 2. Restore Description
                $descText.text(originalDescription);
            });
        });
    </script>
</body>
</html>



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
/* Revert override for Resources (which is also layout-flex but had 40px) if strictly needed, 
   but 30px works generally. Sticking to individual overrides if specific files need it, 
   but here we will keep the general rule robust. */
   
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

/* Editing Dropdowns (Custom) */
.edit-select-wrapper {
    position: relative;
    width: 140px; 
}
/* Standard width for larger forms */
.details-grid .edit-select-wrapper {
    width: 100%;
}

.edit-select-wrapper select {
    width: 100%;
    padding: 6px 30px 6px 10px;
    font-size: 14px;
    border: 1px solid #ddd;
    border-radius: 4px;
    background: #fff;
    appearance: none;
    cursor: pointer;
    color: #333;
    outline: none;
    transition: border-color 0.2s;
}

/* Variant padding for Reporting Org / Field Mapping */
.details-grid .edit-select-wrapper select {
    padding: 8px 30px 8px 12px;
}

.edit-select-wrapper select:focus {
    border-color: #0049ac;
}

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
    border-top: 5px solid #666;
    pointer-events: none;
}

.edit-textarea {
    width: 100%;
    padding: 10px;
    font-size: 14px;
    border: 1px solid #0049ac;
    border-radius: 4px;
    font-family: inherit;
    min-height: 100px;
    resize: vertical;
    outline: none;
    line-height: 1.5;
}

/* Details Grid */
.details-grid {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 24px;
    padding: 20px 0;
}
/* Border only on Field Mapping */
.details-grid.bordered {
    border-bottom: 1px solid #eee;
}

.detail-item {
    display: flex;
    flex-direction: column;
    gap: 4px;
}

.detail-label {
    font-size: 12px;
    color: #666;
    font-weight: 500;
}

.detail-value {
    font-size: 14px;
    color: #1a1a1a;
    font-weight: 400;
    min-height: 24px;
    display: flex;
    align-items: center;
}

/* Description Section */
.description-section {
    padding: 20px 0;
}

.description-label {
    font-size: 12px;
    color: #666;
    font-weight: 500;
    margin-bottom: 8px;
}

.description-text {
    font-size: 14px;
    color: #333;
    line-height: 1.6;
}

/* Dropdown Menu (Field Values) */
.dropdown {
    position: relative;
    display: inline-block;
}

.dropdown-menu {
    display: none;
    position: absolute;
    right: 0;
    top: 100%;
    background: #fff;
    border: 1px solid #ddd;
    border-radius: 4px;
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
    min-width: 120px;
    z-index: 100;
}

.dropdown-menu.show {
    display: block;
}

.dropdown-item {
    display: block;
    padding: 10px 16px;
    font-size: 14px;
    color: #333;
    text-decoration: none;
    cursor: pointer;
    transition: background 0.2s;
}

.dropdown-item:hover {
    background: #f5f5f5;
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
@media (max-width: 1200px) {
    .details-grid {
        grid-template-columns: repeat(3, 1fr);
    }
}

@media (max-width: 1024px) {
    .data-table {
        display: block;
        overflow-x: auto;
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

    .details-grid {
        grid-template-columns: repeat(2, 1fr);
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
}

@media (max-width: 480px) {
    .details-grid {
        grid-template-columns: 1fr;
    }
}