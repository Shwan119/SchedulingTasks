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

//manage division
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Manage Division</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <div class="container">
        <h1>Manage Division</h1>

        <!-- Division Selector -->
        <div class="division-selector">
            <label>Division</label>
            <div class="select-wrapper">
                <select>
                    <option>DWO (ETS)</option>
                    <option>Division 2</option>
                    <option>Division 3</option>
                </select>
            </div>
            <button class="btn btn-secondary">View Fields</button>
        </div>

        <!-- Division Card -->
        <div class="division-card">
            <div class="division-info">
                <span class="division-name" id="division-name-display">ETS (DWO)</span>
                <input type="text" class="division-name-input" id="division-name-input" value="ETS (DWO)" style="display: none;">
                <span class="status-badge status-active" id="division-status-badge">Active</span>
            </div>
            <div class="division-actions">
                <button class="btn btn-outline" id="edit-btn">Edit</button>
                <button class="btn btn-save" id="save-btn" style="display: none;">Save</button>
                <button class="btn btn-cancel" id="cancel-btn" style="display: none;">Cancel</button>
            </div>
        </div>

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="pending">Pending Request</div>
            <div class="tab" data-tab="approved">Approved History</div>
        </div>

        <!-- Pending Request Content -->
        <div id="pending-content" class="tab-content active">
            <!-- Table Header -->
            <div class="table-header">
                <span class="total-count">Total (4)</span>
                <div class="search-box">
                    <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <!-- Data Table -->
            <table class="data-table">
                <thead>
                    <tr>
                        <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Requested by <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Email Address <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 200px;"></th>
                        <th>Requested on <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><a href="#" class="nbk-link">ZK20GX8</a></td>
                        <td>Jason Scott</td>
                        <td class="email-text">jason.scott@bofa.com</td>
                        <td></td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td>
                            <div class="action-buttons">
                                <button class="action-btn approve" title="Approve">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7"/></svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">ZK20GX8</a></td>
                        <td>Jason Scott</td>
                        <td class="email-text">jason.scott@bofa.com</td>
                        <td></td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td>
                            <div class="action-buttons">
                                <button class="action-btn approve" title="Approve">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7"/></svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">ZK20GX8</a></td>
                        <td>Jason Scott</td>
                        <td class="email-text">jason.scott@bofa.com</td>
                        <td></td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td>
                            <div class="action-buttons">
                                <button class="action-btn approve" title="Approve">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7"/></svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">ZK20GX8</a></td>
                        <td>Jason Scott</td>
                        <td class="email-text">jason.scott@bofa.com</td>
                        <td></td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td>
                            <div class="action-buttons">
                                <button class="action-btn approve" title="Approve">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7"/></svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12"/></svg>
                                </button>
                            </div>
                        </td>
                    </tr>
                </tbody>
            </table>

            <!-- Pagination -->
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-4 of 4</span>
                
                <div class="pagination">
                    <button disabled>«</button>
                    <button disabled>‹</button>
                    <button>1</button>
                    <button class="active">2</button>
                    <button>3</button>
                    <span class="dots">...</span>
                    <button>5</button>
                    <button>6</button>
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

        <!-- Approved History Content -->
        <div id="approved-content" class="tab-content" style="display: none;">
            <div class="table-header">
                <span class="total-count">Total (12)</span>
                <div class="search-box">
                    <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Requested by <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Email Address <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 200px;"></th>
                        <th>Approved on <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td><a href="#" class="nbk-link">ZK20GX8</a></td>
                        <td>Jason Scott</td>
                        <td class="email-text">jason.scott@bofa.com</td>
                        <td></td>
                        <td>May 20, 2025 02:15 PM</td>
                        <td><span class="status-badge status-approved">Approved</span></td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">AB45XY2</a></td>
                        <td>Sarah Johnson</td>
                        <td class="email-text">sarah.johnson@bofa.com</td>
                        <td></td>
                        <td>May 19, 2025 11:30 AM</td>
                        <td><span class="status-badge status-approved">Approved</span></td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">CD78MN3</a></td>
                        <td>Mike Chen</td>
                        <td class="email-text">mike.chen@bofa.com</td>
                        <td></td>
                        <td>May 18, 2025 04:45 PM</td>
                        <td><span class="status-badge status-rejected">Rejected</span></td>
                    </tr>
                    <tr>
                        <td><a href="#" class="nbk-link">EF90PQ4</a></td>
                        <td>Emily Davis</td>
                        <td class="email-text">emily.davis@bofa.com</td>
                        <td></td>
                        <td>May 17, 2025 09:00 AM</td>
                        <td><span class="status-badge status-approved">Approved</span></td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-4 of 12</span>
                
                <div class="pagination">
                    <button disabled>«</button>
                    <button disabled>‹</button>
                    <button class="active">1</button>
                    <button>2</button>
                    <button>3</button>
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

    </div>

    <script>
        $(document).ready(function() {
            // ==========================================
            // EDIT DIVISION FUNCTIONALITY
            // ==========================================
            let originalValue = '';
            let originalStatus = '';

            // Cache selectors
            const $display = $('#division-name-display');
            const $input = $('#division-name-input');
            const $statusBadge = $('#division-status-badge');
            const $editBtn = $('#edit-btn');
            const $saveBtn = $('#save-btn');
            const $cancelBtn = $('#cancel-btn');

            $editBtn.on('click', function() {
                // Save original state
                originalValue = $display.text();
                originalStatus = $statusBadge.text().trim();
                $input.val(originalValue);

                // Toggle Visibility
                $display.hide();
                $input.show();
                $editBtn.hide();
                $saveBtn.css('display', 'inline-block'); // jQuery show() might set display:block which breaks alignment
                $cancelBtn.css('display', 'inline-block');

                // Create Status Dropdown
                const $statusWrapper = $('<div class="edit-select-wrapper"></div>');
                const $select = $('<select></select>');
                
                ["Active", "Inactive"].forEach(opt => {
                    const $option = $('<option></option>').val(opt).text(opt);
                    if(opt === originalStatus) $option.prop('selected', true);
                    $select.append($option);
                });

                $statusWrapper.append($select);
                
                // Hide badge and insert dropdown
                $statusBadge.hide();
                $statusBadge.after($statusWrapper);

                // Focus Input
                $input.focus().select();
            });

            $saveBtn.on('click', function() {
                // Update Name
                $display.text($input.val());

                // Update Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if($statusWrapper.length) {
                    const newStatus = $statusWrapper.find('select').val();
                    $statusBadge
                        .text(newStatus)
                        .removeClass()
                        .addClass('status-badge status-' + newStatus.toLowerCase())
                        .show();
                    $statusWrapper.remove();
                }

                // Restore UI
                $display.show();
                $input.hide();
                $editBtn.css('display', 'inline-block');
                $saveBtn.hide();
                $cancelBtn.hide();
            });

            $cancelBtn.on('click', function() {
                // Restore Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if($statusWrapper.length) {
                    $statusBadge
                        .text(originalStatus)
                        .removeClass()
                        .addClass('status-badge status-' + originalStatus.toLowerCase())
                        .show();
                    $statusWrapper.remove();
                }

                // Restore UI
                $display.show();
                $input.hide();
                $editBtn.css('display', 'inline-block');
                $saveBtn.hide();
                $cancelBtn.hide();
            });

            // Handle Keys in Input
            $input.on('keydown', function(e) {
                if (e.key === 'Enter') $saveBtn.click();
                if (e.key === 'Escape') $cancelBtn.click();
            });

            // ==========================================
            // TABS
            // ==========================================
            $('.tab').on('click', function() {
                $('.tab').removeClass('active');
                $(this).addClass('active');
                
                const tabName = $(this).data('tab');
                
                if (tabName === 'pending') {
                    $('#pending-content').show();
                    $('#approved-content').hide();
                } else {
                    $('#pending-content').hide();
                    $('#approved-content').show();
                }
            });

            // ==========================================
            // PAGINATION
            // ==========================================
            $(document).on('click', '.pagination button:not(:disabled)', function() {
                if (!$(this).hasClass('active') && /\d/.test($(this).text())) {
                    $(this).closest('.pagination').find('button').removeClass('active');
                    $(this).addClass('active');
                }
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function() {
                const searchTerm = $(this).val().toLowerCase();
                const $tabContent = $(this).closest('.tab-content');
                const $rows = $tabContent.find('.data-table tbody tr');
                let visibleCount = 0;

                $rows.each(function() {
                    const $row = $(this);
                    const nbkId = $row.find('.nbk-link').text().toLowerCase();
                    const name = $row.find('td:eq(1)').text().toLowerCase();
                    const email = $row.find('td:eq(2)').text().toLowerCase();

                    if (nbkId.includes(searchTerm) || name.includes(searchTerm) || email.includes(searchTerm)) {
                        $row.show();
                        visibleCount++;
                    } else {
                        $row.hide();
                    }
                });

                // Update Count Text
                const $totalCount = $tabContent.find('.total-count');
                if (searchTerm === '') {
                    $totalCount.text('Total (' + $rows.length + ')');
                } else {
                    $totalCount.text('Showing ' + visibleCount + ' of ' + $rows.length);
                }
            });
        });
    </script>
</body>
</html>







// reporting org
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Reporting Organisation</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <div class="container">
        <h1>Reporting Organisation</h1>

        <!-- Division Selector -->
        <div class="selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                <select id="divisionDropdown">
                    <option>DWO (ETS)</option>
                    <option>Division 2</option>
                    <option>Division 3</option>
                </select>
            </div>
            <button class="btn btn-secondary">View Fields</button>
        </div>

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="general">General</div>
            <div class="tab" data-tab="communication">Communication</div>
            <div class="tab" data-tab="report-custom">Report Custom Fields</div>
            <div class="tab" data-tab="process-custom">Process Custom Fields</div>
        </div>

        <!-- General Tab Content -->
        <div id="general-content" class="tab-content active">
            <!-- Reporting Organisation Selector -->
            <div class="selector-row">
                <label>Reporting Organisation</label>
                <div class="select-wrapper">
                    <select id="reportingOrgDropdown">
                        <option>RAM (RAM Reporting Org)</option>
                        <option>Organisation 2</option>
                        <option>Organisation 3</option>
                    </select>
                </div>
            </div>

            <!-- Organisation Info Card -->
            <div class="info-card">
                <div class="info-card-left">
                    <span class="info-card-title" id="org-name-display">RAM Reporting Org (RAM)</span>
                    <span class="status-badge status-active" id="status-badge">Active</span>
                </div>
                <div class="header-actions">
                    <button class="btn btn-outline" id="edit-details-btn">Edit Details</button>
                    <button class="btn btn-save hidden" id="save-details-btn">Save Changes</button>
                    <button class="btn btn-cancel hidden" id="cancel-details-btn">Cancel</button>
                </div>
            </div>

            <!-- Details Grid -->
            <div class="details-grid" id="details-grid-container">
                <div class="detail-item">
                    <span class="detail-label">Name</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Reporting Storage Locations</span>
                    <span class="detail-value">..\Temp\ReportHubData\RAM</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Auto Publish Source Path</span>
                    <span class="detail-value">..\Temp\AutoPublisherData\RAM</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Auto Publish Flag</span>
                    <span class="detail-value">Enabled</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Report Expiration Flag</span>
                    <span class="detail-value">Enabled</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Auto Upload Retirement</span>
                    <span class="detail-value">Enabled</span>
                </div>
            </div>
        </div>

        <!-- Communication Tab Content -->
        <div id="communication-content" class="tab-content">
            <div class="selector-row">
                <label>Reporting Organisation</label>
                <div class="select-wrapper">
                    <select>
                        <option>RAM (RAM Reporting Org)</option>
                        <option>Organisation 2</option>
                        <option>Organisation 3</option>
                    </select>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-left">
                    <span class="info-card-title">Communication Settings</span>
                    <span class="status-badge status-active">Active</span>
                </div>
                <button class="btn btn-outline">Edit Details</button>
            </div>

            <div class="details-grid">
                <div class="detail-item">
                    <span class="detail-label">Email Server</span>
                    <span class="detail-value">smtp.company.com</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Email Port</span>
                    <span class="detail-value">587</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Default From Address</span>
                    <span class="detail-value">reports@company.com</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Email Notifications</span>
                    <span class="detail-value">Enabled</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">SMS Notifications</span>
                    <span class="detail-value">Disabled</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Retry Attempts</span>
                    <span class="detail-value">3</span>
                </div>
            </div>
        </div>

        <!-- Report Custom Fields Tab Content -->
        <div id="report-custom-content" class="tab-content">
            <div class="selector-row">
                <label>Reporting Organisation</label>
                <div class="select-wrapper">
                    <select>
                        <option>RAM (RAM Reporting Org)</option>
                        <option>Organisation 2</option>
                        <option>Organisation 3</option>
                    </select>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-left">
                    <span class="info-card-title">Report Custom Fields</span>
                    <span class="status-badge status-active">Active</span>
                </div>
                <button class="btn btn-outline">Edit Details</button>
            </div>

            <div class="details-grid">
                <div class="detail-item">
                    <span class="detail-label">Custom Field 1</span>
                    <span class="detail-value">Region Code</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Custom Field 2</span>
                    <span class="detail-value">Department ID</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Custom Field 3</span>
                    <span class="detail-value">Cost Center</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Custom Field 4</span>
                    <span class="detail-value">Project Code</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Custom Field 5</span>
                    <span class="detail-value">Not Configured</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Custom Field 6</span>
                    <span class="detail-value">Not Configured</span>
                </div>
            </div>
        </div>

        <!-- Process Custom Fields Tab Content -->
        <div id="process-custom-content" class="tab-content">
            <div class="selector-row">
                <label>Reporting Organisation</label>
                <div class="select-wrapper">
                    <select>
                        <option>RAM (RAM Reporting Org)</option>
                        <option>Organisation 2</option>
                        <option>Organisation 3</option>
                    </select>
                </div>
            </div>

            <div class="info-card">
                <div class="info-card-left">
                    <span class="info-card-title">Process Custom Fields</span>
                    <span class="status-badge status-active">Active</span>
                </div>
                <button class="btn btn-outline">Edit Details</button>
            </div>

            <div class="details-grid">
                <div class="detail-item">
                    <span class="detail-label">Process Field 1</span>
                    <span class="detail-value">Workflow ID</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Process Field 2</span>
                    <span class="detail-value">Approval Level</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Process Field 3</span>
                    <span class="detail-value">Priority Flag</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Process Field 4</span>
                    <span class="detail-value">SLA Category</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Process Field 5</span>
                    <span class="detail-value">Not Configured</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Process Field 6</span>
                    <span class="detail-value">Not Configured</span>
                </div>
            </div>
        </div>

    </div>

    <script>
        $(document).ready(function() {
            // ==========================================
            // TABS
            // ==========================================
            $('.tab').on('click', function() {
                // Remove active class from all tabs
                $('.tab').removeClass('active');
                // Add active class to clicked tab
                $(this).addClass('active');

                // Hide all tab contents
                $('.tab-content').removeClass('active');

                // Show corresponding content
                var tabName = $(this).data('tab');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // EDIT DETAILS FUNCTIONALITY
            // ==========================================
            
            const $editBtn = $('#edit-details-btn');
            const $saveBtn = $('#save-details-btn');
            const $cancelBtn = $('#cancel-details-btn');
            
            const $gridContainer = $('#details-grid-container');
            const $statusBadge = $('#status-badge');

            let originalValues = [];
            let originalStatus = "";
            
            // Config options for dropdowns
            const dropdownOptions = {
                "Name": ["RAM Reporting Org", "Org 2", "Org 3"],
                "Reporting Storage Locations": ["..\\Temp\\ReportHubData\\RAM", "..\\Temp\\NewLocation"],
                "Auto Publish Source Path": ["..\\Temp\\AutoPublisherData\\RAM", "..\\Temp\\NewPath"],
                "Auto Publish Flag": ["Enabled", "Disabled"],
                "Report Expiration Flag": ["Enabled", "Disabled"],
                "Auto Upload Retirement": ["Enabled", "Disabled"]
            };

            // Switch to Edit Mode
            $editBtn.on('click', function() {
                // Toggle Buttons
                $editBtn.addClass('hidden');
                $saveBtn.removeClass('hidden');
                $cancelBtn.removeClass('hidden');

                // 1. Handle Grid Items
                originalValues = []; 
                const $detailItems = $gridContainer.find('.detail-item');
                
                $detailItems.each(function() {
                    const $item = $(this);
                    const label = $item.find('.detail-label').text().trim();
                    const $valueEl = $item.find('.detail-value');
                    const currentText = $valueEl.text().trim();
                    
                    originalValues.push(currentText);
                    
                    // Create Dropdown Wrapper
                    const $wrapper = $('<div class="edit-select-wrapper"></div>');

                    const $select = $('<select></select>');
                    const options = dropdownOptions[label] || [currentText, "Option 1", "Option 2"];
                    
                    $.each(options, function(index, opt) {
                        const $optionEl = $('<option></option>').val(opt).text(opt);
                        if(opt === currentText) $optionEl.prop('selected', true);
                        $select.append($optionEl);
                    });
                    
                    // Ensure current value is in list
                    if ($.inArray(currentText, options) === -1 && currentText !== "") {
                         const $optionEl = $('<option></option>').val(currentText).text(currentText).prop('selected', true);
                         $select.prepend($optionEl);
                    }

                    $wrapper.append($select);
                    $valueEl.empty().append($wrapper);
                });
                
                // 2. Handle Status Badge
                originalStatus = $statusBadge.text().trim();
                const $statusWrapper = $('<div class="edit-select-wrapper" style="width: 120px;"></div>');
                
                const $statusSelect = $('<select></select>');
                const statusOptions = ["Active", "Inactive"];

                $.each(statusOptions, function(index, opt) {
                    const $o = $('<option></option>').val(opt).text(opt);
                    if(opt === originalStatus) $o.prop('selected', true);
                    $statusSelect.append($o);
                });
                
                $statusWrapper.append($statusSelect);
                
                // Replace badge with dropdown temporarily
                $statusBadge.hide();
                $statusBadge.after($statusWrapper);
            });

            // Save Changes
            $saveBtn.on('click', function() {
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // 1. Save Grid Items
                $gridContainer.find('.detail-value').each(function() {
                    const $el = $(this);
                    const $select = $el.find('select');
                    if ($select.length) {
                        $el.text($select.val());
                    }
                });
                
                // 2. Save Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if($statusWrapper.length) {
                    const newStatus = $statusWrapper.find('select').val();
                    $statusBadge.text(newStatus);
                    $statusBadge.removeClass().addClass('status-badge status-' + newStatus.toLowerCase());
                    $statusBadge.show();
                    $statusWrapper.remove();
                }
            });

            // Cancel Changes
            $cancelBtn.on('click', function() {
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // 1. Restore Grid Items
                $gridContainer.find('.detail-value').each(function(index) {
                    $(this).text(originalValues[index]);
                });
                
                // 2. Restore Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if($statusWrapper.length) {
                    $statusBadge.text(originalStatus);
                    $statusBadge.removeClass().addClass('status-badge status-' + originalStatus.toLowerCase());
                    $statusBadge.show();
                    $statusWrapper.remove();
                }
            });
        });
    </script>
</body>
</html>









// field mapping
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
            <label>Division</label>
            <div class="select-wrapper">
                <select id="divisionDropdown">
                    <option>DWO (ETS)</option>
                    <option>Division 2</option>
                    <option>Division 3</option>
                </select>
            </div>
            <button class="btn btn-secondary">View Fields</button>
        </div>

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="lob-mapping">LOB Mapping</div>
        </div>

        <!-- LOB Mapping Tab Content -->
        <div id="lob-mapping-content" class="tab-content active">
            <!-- LOB Selector -->
            <div class="selector-row">
                <label>LOB</label>
                <div class="select-wrapper">
                    <select id="lobDropdown">
                        <option>All DSRA Access Request</option>
                        <option>LOB Option 2</option>
                        <option>LOB Option 3</option>
                    </select>
                </div>
            </div>

            <!-- Report Information Section -->
            <div class="section-header">
                <span class="section-title">Report Information</span>
                <div class="header-actions">
                    <button class="btn btn-outline" id="edit-details-btn">Edit Details</button>
                    <button class="btn btn-save hidden" id="save-details-btn">Save Changes</button>
                    <button class="btn btn-cancel hidden" id="cancel-details-btn">Cancel</button>
                </div>
            </div>

            <!-- Details Grid -->
            <div class="details-grid bordered" id="details-grid-container">
                <div class="detail-item">
                    <span class="detail-label">LOB Group</span>
                    <span class="detail-value">ETS Issues Report</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Reporting Organisation</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Reporting Manager</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Reporting Lead</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">iDrive</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
                <div class="detail-item">
                    <span class="detail-label">Executive</span>
                    <span class="detail-value">RAM Reporting Org</span>
                </div>
            </div>

            <!-- Data Ready Logic Section -->
            <div class="description-section">
                <div class="description-label">Data Ready Logic</div>
                <p class="description-text" id="logic-description">
                    Data Ready Logic is a crucial component in digital systems, responsible for signaling when valid data is available for processing or transfer. It ensures data integrity and prevents race conditions by synchronizing data flow.
                </p>
            </div>
        </div>

    </div>

    <script>
        $(document).ready(function() {
            // Configuration for Dropdown Options
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
            $saveBtn.on('click', function() {
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






// resources
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Resources</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <!-- Added layout-flex class to maintain specific layout properties for this page -->
    <div class="container layout-flex">
        <h1>Resources</h1>

        <!-- Division Selector -->
        <div class="selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                <select id="divisionDropdown">
                    <option>DWO (ETS)</option>
                    <option>Division 2</option>
                    <option>Division 3</option>
                </select>
            </div>
            <button class="btn btn-secondary">View Fields</button>
        </div>

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="executives">Executives (20)</div>
            <div class="tab" data-tab="report-leads">Report Leads (10)</div>
            <div class="tab" data-tab="report-manager">Report Manager (10)</div>
            <div class="tab" data-tab="idrives">iDrives (5)</div>
        </div>

        <!-- Executives Tab Content -->
        <div id="executives-content" class="tab-content active">
            <!-- Table Header -->
            <div class="table-header">
                <!-- Added generic class 'add-new-btn' -->
                <button class="btn btn-primary add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search by Report ID or Name" class="table-search">
                </div>
            </div>

            <!-- Data Table -->
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 10%">ID</th>
                        <th style="width: 15%">NBK ID</th>
                        <th style="width: 25%">Full Name</th>
                        <th style="width: 20%"></th> <!-- Spacer -->
                        <th style="width: 15%">Status</th>
                        <th style="width: 15%"></th> <!-- Actions -->
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>123</td>
                        <td>ZKA6IED</td>
                        <td>Jason Scott</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td><!-- Actions handled by JS --></td>
                    </tr>
                    <tr>
                        <td>124</td>
                        <td>IMP</td>
                        <td>Maria Rodriguez</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>125</td>
                        <td>IMP</td>
                        <td>Sam Williams</td>
                        <td></td>
                        <td><span class="status-inactive">Inactive</span></td>
                        <td></td>
                    </tr>
                </tbody>
            </table>

            <!-- Pagination -->
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-3 of 20</span>
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
                    <select><option>10</option><option>25</option></select>
                </div>
            </div>
        </div>

        <!-- Report Leads Tab Content -->
        <div id="report-leads-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search Report Leads" class="table-search">
                </div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 10%">ID</th>
                        <th style="width: 15%">NBK ID</th>
                        <th style="width: 25%">Full Name</th>
                        <th style="width: 20%"></th>
                        <th style="width: 15%">Status</th>
                        <th style="width: 15%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>201</td>
                        <td>RL001</td>
                        <td>Sarah Connor</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td></td>
                    </tr>
                    <tr>
                        <td>202</td>
                        <td>RL002</td>
                        <td>John Doe</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td></td>
                    </tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-2 of 10</span>
                <div class="pagination">
                    <button class="active">1</button>
                    <button>2</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option></select>
                </div>
            </div>
        </div>

        <!-- Report Manager Tab Content -->
        <div id="report-manager-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search Report Managers" class="table-search">
                </div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 10%">ID</th>
                        <th style="width: 15%">NBK ID</th>
                        <th style="width: 25%">Full Name</th>
                        <th style="width: 20%"></th>
                        <th style="width: 15%">Status</th>
                        <th style="width: 15%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>301</td>
                        <td>RM101</td>
                        <td>Alice Wonderland</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td></td>
                    </tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 10</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option></select>
                </div>
            </div>
        </div>

        <!-- iDrives Tab Content -->
        <div id="idrives-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search iDrives" class="table-search">
                </div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 10%">ID</th>
                        <th style="width: 15%">NBK ID</th>
                        <th style="width: 25%">Full Name</th>
                        <th style="width: 20%"></th>
                        <th style="width: 15%">Status</th>
                        <th style="width: 15%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>401</td>
                        <td>ID001</td>
                        <td>Driver One</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td></td>
                    </tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 5</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option></select>
                </div>
            </div>
        </div>

    </div>

    <script>
        $(document).ready(function() {
            // ==========================================
            // INITIALIZATION
            // ==========================================
            
            // Generate action buttons for all existing rows on load
            $('.data-table tbody tr').each(function() {
                // Ensure the last cell has the action buttons
                var $cell = $(this).find('td:last');
                if($cell.is(':empty') || $cell.html().trim() === '') {
                    $cell.html(getActionButtonsHTML(false));
                }
            });

            // ==========================================
            // TABS
            // ==========================================
            $('.tab').on('click', function() {
                // Toggle active class on tab buttons
                $('.tab').removeClass('active');
                $(this).addClass('active');

                // Toggle active class on content divs
                var tabName = $(this).data('tab');
                $('.tab-content').removeClass('active');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // PAGINATION
            // ==========================================
            // Event delegation used in case pagination is rebuilt dynamically later
            $(document).on('click', '.pagination button:not(:disabled)', function() {
                var $btn = $(this);
                
                // Only act if it's a number button
                if (/\d/.test($btn.text())) {
                    // Remove active from siblings in this specific pagination container
                    $btn.closest('.pagination').find('button').removeClass('active');
                    $btn.addClass('active');
                }
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function() {
                var term = $(this).val().toLowerCase();
                // Find the table within the same tab content
                var $table = $(this).closest('.tab-content').find('.data-table');
                
                $table.find('tbody tr').each(function() {
                    var $row = $(this);
                    // Skip filtering if row is currently in edit mode
                    if ($row.hasClass('editing')) return;

                    var id = $row.find('td:eq(0)').text().toLowerCase();
                    var nbk = $row.find('td:eq(1)').text().toLowerCase();
                    var name = $row.find('td:eq(2)').text().toLowerCase();

                    if (id.indexOf(term) > -1 || nbk.indexOf(term) > -1 || name.indexOf(term) > -1) {
                        $row.show();
                    } else {
                        $row.hide();
                    }
                });
            });

            // ==========================================
            // HELPER FUNCTIONS
            // ==========================================

            function getActionButtonsHTML(isEditing) {
                if (isEditing) {
                    return `
                        <div class="action-buttons">
                            <button class="action-btn approve" title="Save">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                    <path d="M5 13l4 4L19 7"/>
                                </svg>
                            </button>
                            <button class="action-btn reject" title="Cancel">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                    <path d="M6 18L18 6M6 6l12 12"/>
                                </svg>
                            </button>
                        </div>
                    `;
                } else {
                    return `
                        <div class="action-buttons">
                            <button class="icon-btn edit-btn" title="Edit">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                                    <path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"/>
                                </svg>
                            </button>
                            <button class="icon-btn more-btn" title="More options">
                                <svg viewBox="0 0 24 24" fill="currentColor">
                                    <circle cx="12" cy="5" r="2"/>
                                    <circle cx="12" cy="12" r="2"/>
                                    <circle cx="12" cy="19" r="2"/>
                                </svg>
                            </button>
                        </div>
                    `;
                }
            }

            function makeRowEditable($row, isNew) {
                $row.addClass('editing');
                
                // Retrieve current values
                var id = "", nbk = "", name = "", status = "Active";

                if (!isNew) {
                    id = $row.find('td:eq(0)').text().trim();
                    nbk = $row.find('td:eq(1)').text().trim();
                    name = $row.find('td:eq(2)').text().trim();
                    status = $row.find('td:eq(4)').text().trim();

                    // Save original data to restore if cancelled
                    $row.data('original', { id: id, nbk: nbk, name: name, status: status });
                } else {
                    $row.data('is-new', true);
                }

                // Replace cells with inputs
                $row.find('td:eq(0)').html(`<input type="text" class="inline-input id-input" value="${id}" placeholder="ID">`);
                $row.find('td:eq(1)').html(`<input type="text" class="inline-input nbk-input" value="${nbk}" placeholder="NBK ID">`);
                $row.find('td:eq(2)').html(`<input type="text" class="inline-input name-input" value="${name}" placeholder="Full Name">`);
                
                // Status Dropdown
                var isInactive = (status === 'Inactive');
                var selectHtml = `
                    <select class="inline-input status-select">
                        <option value="Active" ${!isInactive ? 'selected' : ''}>Active</option>
                        <option value="Inactive" ${isInactive ? 'selected' : ''}>Inactive</option>
                    </select>
                `;
                $row.find('td:eq(4)').html(selectHtml);

                // Action Buttons
                $row.find('td:last').html(getActionButtonsHTML(true));
            }

            // ==========================================
            // ACTION BUTTON LISTENERS (Event Delegation)
            // ==========================================

            // 1. ADD NEW ROW
            $('.add-new-btn').on('click', function() {
                // Find the table body within the current tab
                var $tbody = $(this).closest('.tab-content').find('.data-table tbody');
                
                // Create new row
                var $newRow = $('<tr>');
                for(var i=0; i<6; i++) {
                    $newRow.append('<td></td>');
                }
                
                $tbody.prepend($newRow);
                makeRowEditable($newRow, true);
            });

            // 2. EDIT BUTTON
            $(document).on('click', '.edit-btn', function() {
                var $row = $(this).closest('tr');
                makeRowEditable($row, false);
            });

            // 3. CANCEL (REJECT) BUTTON
            $(document).on('click', '.reject', function() {
                var $row = $(this).closest('tr');
                
                if ($row.data('is-new')) {
                    $row.remove();
                } else {
                    // Restore original data
                    var original = $row.data('original');
                    
                    $row.removeClass('editing');
                    $row.find('td:eq(0)').text(original.id);
                    $row.find('td:eq(1)').text(original.nbk);
                    $row.find('td:eq(2)').text(original.name);
                    
                    var statusClass = (original.status === 'Active') ? 'status-active' : 'status-inactive';
                    $row.find('td:eq(4)').html(`<span class="${statusClass}">${original.status}</span>`);
                    
                    $row.find('td:last').html(getActionButtonsHTML(false));
                }
            });

            // 4. SAVE (APPROVE) BUTTON
            $(document).on('click', '.approve', function() {
                var $row = $(this).closest('tr');
                
                var newId = $row.find('.id-input').val();
                var newNbk = $row.find('.nbk-input').val();
                var newName = $row.find('.name-input').val();
                var newStatus = $row.find('.status-select').val();

                if(!newId || !newName) {
                    alert("ID and Name are required");
                    return;
                }

                $row.removeClass('editing');
                $row.removeData('is-new'); // No longer a new row
                $row.removeData('original'); // Clear backup

                $row.find('td:eq(0)').text(newId);
                $row.find('td:eq(1)').text(newNbk);
                $row.find('td:eq(2)').text(newName);
                
                var statusClass = (newStatus === 'Active') ? 'status-active' : 'status-inactive';
                $row.find('td:eq(4)').html(`<span class="${statusClass}">${newStatus}</span>`);
                
                $row.find('td:last').html(getActionButtonsHTML(false));
            });
        });
    </script>
</body>
</html>










// field values
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Field Values</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
</head>
<body>
    <!-- Added layout-flex class to maintain specific layout properties for this page -->
    <div class="container layout-flex">
        <h1>Field Values</h1>

        <div class="selector-row">
            <label>Division</label>
            <div class="select-wrapper">
                <select id="divisionDropdown">
                    <option>ECOE (Enterprise Complaints Operations and Enablement Reporting & Analytics)</option>
                    <option>DWO (ETS)</option>
                    <option>Division 3</option>
                </select>
            </div>
            <button class="btn btn-secondary">View Fields</button>
        </div>

        <div class="tabs">
            <div class="tab active" data-tab="lob-group">LOB Group (5)</div>
            <div class="tab" data-tab="lob">LOB (2)</div>
            <div class="tab" data-tab="lob-detail">LOB Detail (6)</div>
            <div class="tab" data-tab="report-category">Report Category (4)</div>
            <div class="tab" data-tab="data-environment">Data Environment (1)</div>
            <div class="tab" data-tab="app-data-source">App Data Source (19)</div>
        </div>

        <!-- LOB Group Tab -->
        <div id="lob-group-content" class="tab-content active">
            <div class="table-header">
                <button class="btn btn-primary add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" class="table-search" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>123</td>
                        <td>A12C</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td>https://tableup.bankofamericaSetsVolumeHistory/Dashboard</td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td><span class="status-active">Active</span></td>
                        <td><!-- Actions handled by JS --></td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-5 of 5</span>
                <div class="pagination">
                    <button disabled>«</button>
                    <button disabled>‹</button>
                    <button>1</button>
                    <button class="active">2</button>
                    <button>3</button>
                    <button>›</button>
                    <button>»</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>

        <!-- LOB Tab -->
        <div id="lob-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary add-new-btn"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" class="table-search" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td>456</td><td>B34D</td><td>LOB1</td><td>Line of Business 1</td><td>Primary LOB</td><td>May 20, 2025 10:00 AM</td><td><span class="status-active">Active</span></td>
                    <td></td></tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-2 of 2</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>

        <!-- LOB Detail Tab -->
        <div id="lob-detail-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" class="table-search" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td>789</td><td>D56F</td><td>DET1</td><td>Detail 1</td><td>LOB Detail</td><td>May 18, 2025 02:15 PM</td><td><span class="status-active">Active</span></td>
                    <td></td></tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 6</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>

        <!-- Report Category Tab -->
        <div id="report-category-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" class="table-search" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td>101</td><td>E78G</td><td>CAT1</td><td>Category 1</td><td>Report Category</td><td>May 17, 2025 11:45 AM</td><td><span class="status-active">Active</span></td>
                     <td></td></tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 4</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>

        <!-- Data Environment Tab -->
        <div id="data-environment-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" class="table-search" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td>201</td><td>F89H</td><td>ENV1</td><td>Production</td><td>Data Environment</td><td>May 16, 2025 09:10 AM</td><td><span class="status-active">Active</span></td>
                     <td></td></tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 1</span>
                <div class="pagination">
                    <button class="active">1</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>

        <!-- App Data Source Tab -->
        <div id="app-data-source-content" class="tab-content">
             <div class="table-header">
                <button class="btn btn-primary add-new-btn"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" class="table-search" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table">
                <thead>
                    <tr>
                        <th style="width: 8%">ID</th>
                        <th style="width: 8%">CD</th>
                        <th style="width: 10%">Short Name</th>
                        <th style="width: 15%">Full Name</th>
                        <th style="width: 25%">Description</th>
                        <th style="width: 15%">Created on</th>
                        <th style="width: 10%">Status</th>
                        <th style="width: 9%"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr><td>301</td><td>G90I</td><td>SRC1</td><td>Data Source 1</td><td>App Data Source</td><td>May 15, 2025 03:20 PM</td><td><span class="status-active">Active</span></td>
                     <td></td></tr>
                </tbody>
            </table>
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 19</span>
                <div class="pagination">
                    <button class="active">1</button>
                    <button>2</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option></select>
                </div>
            </div>
        </div>
    </div>

    <script>
        $(document).ready(function() {
            // ==========================================
            // INITIALIZATION
            // ==========================================
            
            // Generate action buttons for all existing rows on load
            $('.data-table tbody tr').each(function() {
                var $cell = $(this).find('td:last');
                if($cell.is(':empty') || $cell.html().trim() === '') {
                    $cell.html(getActionButtonsHTML(false));
                }
            });

            // ==========================================
            // TABS
            // ==========================================
            $('.tab').on('click', function() {
                $('.tab').removeClass('active');
                $(this).addClass('active');

                var tabName = $(this).data('tab');
                $('.tab-content').removeClass('active');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // PAGINATION (Visual)
            // ==========================================
            $(document).on('click', '.pagination button:not(:disabled)', function() {
                var $btn = $(this);
                if (/\d/.test($btn.text())) {
                    $btn.closest('.pagination').find('button').removeClass('active');
                    $btn.addClass('active');
                }
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function() {
                var term = $(this).val().toLowerCase();
                var $table = $(this).closest('.tab-content').find('.data-table');
                
                $table.find('tbody tr').each(function() {
                    var $row = $(this);
                    if ($row.hasClass('editing')) return;

                    // Search across all text in the row
                    if ($row.text().toLowerCase().indexOf(term) > -1) {
                        $row.show();
                    } else {
                        $row.hide();
                    }
                });
            });

            // ==========================================
            // DROPDOWNS
            // ==========================================
            $(document).on('click', '.more-btn', function(e) {
                e.stopPropagation();
                // Close others
                $('.dropdown-menu').not($(this).next('.dropdown-menu')).removeClass('show');
                // Toggle current
                $(this).next('.dropdown-menu').toggleClass('show');
            });

            $(document).on('click', function() {
                $('.dropdown-menu').removeClass('show');
            });

            // ==========================================
            // HELPER FUNCTIONS
            // ==========================================
            function getActionButtonsHTML(isEditing) {
                if (isEditing) {
                    return `
                        <div class="action-buttons">
                            <button class="action-btn approve" title="Save">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7"/></svg>
                            </button>
                            <button class="action-btn reject" title="Cancel">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12"/></svg>
                            </button>
                        </div>
                    `;
                } else {
                    return `
                        <div class="action-buttons">
                            <button class="icon-btn edit-btn" title="Edit">
                                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M17 3a2.85 2.85 0 1 1 4 4L7.5 20.5 2 22l1.5-5.5L17 3z"/></svg>
                            </button>
                            <div class="dropdown">
                                <button class="icon-btn more-btn" title="More options">
                                    <svg viewBox="0 0 24 24" fill="currentColor"><circle cx="12" cy="5" r="2"/><circle cx="12" cy="12" r="2"/><circle cx="12" cy="19" r="2"/></svg>
                                </button>
                                <div class="dropdown-menu">
                                    <a class="dropdown-item" href="#">Edit</a>
                                    <a class="dropdown-item" href="#">Inactive</a>
                                    <a class="dropdown-item" href="#">Delete</a>
                                </div>
                            </div>
                        </div>
                    `;
                }
            }

            function getCurrentDateString() {
                const options = { year: 'numeric', month: 'long', day: 'numeric', hour: '2-digit', minute:'2-digit' };
                return new Date().toLocaleDateString('en-US', options);
            }

            function makeRowEditable($row, isNew) {
                $row.addClass('editing');
                
                var id = "", cd = "", short = "", full = "", desc = "", created = "", status = "Active";

                if (!isNew) {
                    id = $row.find('td:eq(0)').text().trim();
                    cd = $row.find('td:eq(1)').text().trim();
                    short = $row.find('td:eq(2)').text().trim();
                    full = $row.find('td:eq(3)').text().trim();
                    desc = $row.find('td:eq(4)').text().trim();
                    created = $row.find('td:eq(5)').text().trim();
                    status = $row.find('td:eq(6)').text().trim();

                    $row.data('original', { id, cd, short, full, desc, created, status });
                } else {
                    $row.data('is-new', true);
                    created = getCurrentDateString();
                }

                $row.find('td:eq(0)').html(`<input type="text" class="inline-input id-input" value="${id}" placeholder="ID">`);
                $row.find('td:eq(1)').html(`<input type="text" class="inline-input cd-input" value="${cd}" placeholder="CD">`);
                $row.find('td:eq(2)').html(`<input type="text" class="inline-input short-input" value="${short}" placeholder="Short Name">`);
                $row.find('td:eq(3)').html(`<input type="text" class="inline-input full-input" value="${full}" placeholder="Full Name">`);
                $row.find('td:eq(4)').html(`<input type="text" class="inline-input wide desc-input" value="${desc}" placeholder="Description">`);
                
                $row.find('td:eq(5)').text(created);

                var isInactive = (status === 'Inactive');
                $row.find('td:eq(6)').html(`
                    <select class="inline-input status-select">
                        <option value="Active" ${!isInactive ? 'selected' : ''}>Active</option>
                        <option value="Inactive" ${isInactive ? 'selected' : ''}>Inactive</option>
                    </select>
                `);

                $row.find('td:last').html(getActionButtonsHTML(true));
            }

            // ==========================================
            // ACTION BUTTON LISTENERS
            // ==========================================

            // 1. ADD NEW ROW
            $('.add-new-btn').on('click', function() {
                var $tbody = $(this).closest('.tab-content').find('.data-table tbody');
                var $newRow = $('<tr>');
                for(var i=0; i<8; i++) $newRow.append('<td></td>');
                $tbody.prepend($newRow);
                makeRowEditable($newRow, true);
            });

            // 2. EDIT BUTTON
            $(document).on('click', '.edit-btn', function() {
                var $row = $(this).closest('tr');
                makeRowEditable($row, false);
            });

            // 3. CANCEL (REJECT)
            $(document).on('click', '.reject', function() {
                var $row = $(this).closest('tr');
                if ($row.data('is-new')) {
                    $row.remove();
                } else {
                    var org = $row.data('original');
                    $row.removeClass('editing');
                    $row.find('td:eq(0)').text(org.id);
                    $row.find('td:eq(1)').text(org.cd);
                    $row.find('td:eq(2)').text(org.short);
                    $row.find('td:eq(3)').text(org.full);
                    $row.find('td:eq(4)').text(org.desc);
                    $row.find('td:eq(5)').text(org.created);
                    
                    var statusClass = (org.status === 'Active') ? 'status-active' : 'status-inactive';
                    $row.find('td:eq(6)').html(`<span class="${statusClass}">${org.status}</span>`);
                    
                    $row.find('td:last').html(getActionButtonsHTML(false));
                }
            });

            // 4. SAVE (APPROVE)
            $(document).on('click', '.approve', function() {
                var $row = $(this).closest('tr');
                
                var newId = $row.find('.id-input').val();
                var newCd = $row.find('.cd-input').val();
                var newShort = $row.find('.short-input').val();
                var newFull = $row.find('.full-input').val();
                var newDesc = $row.find('.desc-input').val();
                var newStatus = $row.find('.status-select').val();

                if(!newId) { alert("ID is required"); return; }

                $row.removeClass('editing');
                $row.removeData('is-new');
                $row.removeData('original');

                $row.find('td:eq(0)').text(newId);
                $row.find('td:eq(1)').text(newCd);
                $row.find('td:eq(2)').text(newShort);
                $row.find('td:eq(3)').text(newFull);
                $row.find('td:eq(4)').text(newDesc);
                // col 5 (created) is already set
                
                var statusClass = (newStatus === 'Active') ? 'status-active' : 'status-inactive';
                $row.find('td:eq(6)').html(`<span class="${statusClass}">${newStatus}</span>`);
                
                $row.find('td:last').html(getActionButtonsHTML(false));
            });
        });
    </script>
</body>
</html>







//css styles
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
    max-width: 400px;
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
	/* 10px 20px */
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
    padding: 14px 40px;
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
	font-weight: 500;
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
	font-weight: 500;
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