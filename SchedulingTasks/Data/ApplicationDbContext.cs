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
@model ManageDivisionViewModel

@{
    ViewBag.Title = "Manage Division";
}

<link href="~/css/styles.css" rel="stylesheet" />

@* ============================================
   PAGE SPECIFIC STYLES - ADD TO GLOBAL CSS
   ============================================ *@
<style>
    /* ============================================
       PAGE SPECIFIC STYLES (Manage Division)
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

    /* Pending Status Text */
    .pending-status {
        font-style: italic;
        color: #d97706;
        font-size: 13px;
        font-weight: 500;
        white-space: nowrap;
    }

    /* Action Cell Layout */
    .action-cell {
        display: flex;
        align-items: center;
        justify-content: flex-end;
        gap: 16px;
    }

    /* Responsive */
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

        .action-cell {
            flex-direction: column;
            align-items: flex-end;
            gap: 8px;
        }

        .pending-status {
            font-size: 12px;
        }
    }
</style>

@{
    AppUser _curUser = PageData["CurrentUser"] as AppUser;

    // Mock data for Approved History
    var approvedHistory = new List<dynamic>
    {
        new { NBK = "ZK20GX8", Username = "Jason Scott", Email = "jason.scott@bofa.com", ApprovedOn = new DateTime(2025, 5, 20, 14, 15, 0), Status = "Approved" },
        new { NBK = "AB45XY2", Username = "Sarah Johnson", Email = "sarah.johnson@bofa.com", ApprovedOn = new DateTime(2025, 5, 19, 11, 30, 0), Status = "Approved" },
        new { NBK = "CD78MN3", Username = "Mike Chen", Email = "mike.chen@bofa.com", ApprovedOn = new DateTime(2025, 5, 18, 16, 45, 0), Status = "Rejected" },
        new { NBK = "EF90PQ4", Username = "Emily Davis", Email = "emily.davis@bofa.com", ApprovedOn = new DateTime(2025, 5, 17, 9, 0, 0), Status = "Approved" },
        new { NBK = "GH12RS5", Username = "David Wilson", Email = "david.wilson@bofa.com", ApprovedOn = new DateTime(2025, 5, 16, 10, 30, 0), Status = "Approved" },
        new { NBK = "IJ34TU6", Username = "Lisa Brown", Email = "lisa.brown@bofa.com", ApprovedOn = new DateTime(2025, 5, 15, 15, 0, 0), Status = "Rejected" }
    };
}

<div class="container">
    @using (Html.BeginForm("ManageDivision", "Division", FormMethod.Post, new { @id = "frmManageDivision" }))
    {
        @Html.AntiForgeryToken()

        <h1>Manage Division</h1>

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
            @Html.HiddenFor(m => m.TheDivision.ID)
            @Html.HiddenFor(m => m.TheDivision.Code)
            @Html.HiddenFor(m => m.TheDivision.ActiveFlag)
            @Html.HiddenFor(m => m.ApproveUserId)

            <!-- Division Card -->
            <div class="division-card">
                <div class="division-info">
                    <span class="division-name" id="division-name-display">@Model.TheDivision.NameCode</span>
                    @Html.TextBoxFor(m => m.TheDivision.Name, new { @class = "division-name-input", @id = "division-name-input", @style = "display: none;" })
                    @if (Model.TheDivision.ActiveFlag == true)
                    {
                        <span class="status-badge status-active" id="division-status-badge">Active</span>
                    }
                    else
                    {
                        <span class="status-badge status-inactive" id="division-status-badge">Inactive</span>
                    }
                </div>
                <div class="division-actions">
                    <button type="button" class="btn btn-outline" id="edit-btn">Edit</button>
                    <button type="button" class="btn btn-save cgUpdateDivision" id="save-btn" style="display: none;">Save</button>
                    <button type="button" class="btn btn-cancel" id="cancel-btn" style="display: none;">Cancel</button>
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
                    <span class="total-count">Total (@(Model.PendingAdmins.Count + Model.ApprovedAdmins.Count))</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by NBK ID or Name">
                    </div>
                </div>

                <!-- Data Table -->
                <table class="data-table" id="tblAdminList">
                    <thead>
                        <tr>
                            <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Requested by <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Email Address <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th style="width: 280px;"></th>
                        </tr>
                    </thead>
                    <tbody>
                        @if (Model.PendingAdmins.Count > 0)
                        {
                            foreach (var pending in Model.PendingAdmins)
                            {
                                <tr>
                                    <td><a href="#" class="nbk-link">@pending.NBK</a></td>
                                    <td>@pending.Username</td>
                                    <td class="email-text">@pending.Email</td>
                                    <td>
                                        <div class="action-cell">
                                            <span class="pending-status">pending for approval</span>
                                            <div class="action-buttons">
                                                <button type="button" class="action-btn approve cgApproveAdmin" id="btnApproveAdmin_@pending.ID" title="Approve">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M5 13l4 4L19 7" /></svg>
                                                </button>
                                                <button type="button" class="action-btn reject cgRejectAdmin" id="btnRejectAdmin_@pending.ID" title="Reject">
                                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3"><path d="M6 18L18 6M6 6l12 12" /></svg>
                                                </button>
                                            </div>
                                        </div>
                                    </td>
                                </tr>
                            }
                        }

                        @if (Model.ApprovedAdmins.Count > 0)
                        {
                            foreach (var approved in Model.ApprovedAdmins)
                            {
                                <tr>
                                    <td><a href="#" class="nbk-link">@approved.NBK</a></td>
                                    <td>@approved.Username</td>
                                    <td class="email-text">@approved.Email</td>
                                    <td></td>
                                </tr>
                            }
                        }

                        @if (Model.ApprovedAdmins.Count == 0 && Model.PendingAdmins.Count == 0)
                        {
                            <tr>
                                <td colspan="4" style="text-align: center; padding: 32px; color: #666;">
                                    <h4>No administrator found.</h4>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>

                <!-- Pagination -->
                <div class="pagination-wrapper">
                    <span class="showing-text">Showing 1-@(Model.PendingAdmins.Count + Model.ApprovedAdmins.Count) of @(Model.PendingAdmins.Count + Model.ApprovedAdmins.Count)</span>

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

            <!-- Approved History Content -->
            <div id="approved-content" class="tab-content">
                <div class="table-header">
                    <span class="total-count">Total (@approvedHistory.Count)</span>
                    <div class="search-box">
                        <input type="text" class="table-search" placeholder="Search by NBK ID or Name">
                    </div>
                </div>

                <table class="data-table">
                    <thead>
                        <tr>
                            <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Requested by <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Email Address <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Approved on <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        @foreach (var item in approvedHistory)
                        {
                            <tr>
                                <td><a href="#" class="nbk-link">@item.NBK</a></td>
                                <td>@item.Username</td>
                                <td class="email-text">@item.Email</td>
                                <td>@item.ApprovedOn.ToString("MMM dd, yyyy hh:mm tt")</td>
                                <td>
                                    @if (item.Status == "Approved")
                                    {
                                        <span class="status-badge status-approved">Approved</span>
                                    }
                                    else
                                    {
                                        <span class="status-badge status-rejected">Rejected</span>
                                    }
                                </td>
                            </tr>
                        }

                        @if (approvedHistory.Count == 0)
                        {
                            <tr>
                                <td colspan="5" style="text-align: center; padding: 32px; color: #666;">
                                    <h4>No approval history found.</h4>
                                </td>
                            </tr>
                        }
                    </tbody>
                </table>

                <div class="pagination-wrapper">
                    <span class="showing-text">Showing 1-@approvedHistory.Count of @approvedHistory.Count</span>

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
                $('#frmManageDivision').submit();
            });

            // ==========================================
            // EDIT DIVISION FUNCTIONALITY
            // ==========================================
            let originalValue = '';
            let originalStatus = '';

            const $display = $('#division-name-display');
            const $input = $('#division-name-input');
            const $statusBadge = $('#division-status-badge');
            const $editBtn = $('#edit-btn');
            const $saveBtn = $('#save-btn');
            const $cancelBtn = $('#cancel-btn');

            $editBtn.on('click', function () {
                // Save original state
                originalValue = $display.text();
                originalStatus = $statusBadge.text().trim();
                $input.val(originalValue);

                // Toggle Visibility
                $display.hide();
                $input.show();
                $editBtn.hide();
                $saveBtn.css('display', 'inline-flex');
                $cancelBtn.css('display', 'inline-flex');

                // Create Status Dropdown
                const $statusWrapper = $('<div class="edit-select-wrapper" style="position: relative; display: inline-block;"></div>');
                const $select = $('<select style="padding: 4px 12px; font-size: 12px; border: 1px solid #0049ac; border-radius: 4px; outline: none;"></select>');

                ["Active", "Inactive"].forEach(function (opt) {
                    const $option = $('<option></option>').val(opt).text(opt);
                    if (opt === originalStatus) $option.prop('selected', true);
                    $select.append($option);
                });

                $statusWrapper.append($select);

                // Hide badge and insert dropdown
                $statusBadge.hide();
                $statusBadge.after($statusWrapper);

                // Focus Input
                $input.focus().select();
            });

            $saveBtn.on('click', function () {
                // Update Name Display
                $display.text($input.val());

                // Update Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if ($statusWrapper.length) {
                    const newStatus = $statusWrapper.find('select').val();
                    $statusBadge
                        .text(newStatus)
                        .removeClass('status-active status-inactive')
                        .addClass('status-badge status-' + newStatus.toLowerCase())
                        .show();
                    $statusWrapper.remove();
                }

                // Restore UI
                $display.show();
                $input.hide();
                $editBtn.css('display', 'inline-flex');
                $saveBtn.hide();
                $cancelBtn.hide();

                // Note: The actual save is handled by cgUpdateDivision class in manageDivision.js
            });

            $cancelBtn.on('click', function () {
                // Restore Status
                const $statusWrapper = $statusBadge.next('.edit-select-wrapper');
                if ($statusWrapper.length) {
                    $statusBadge
                        .text(originalStatus)
                        .removeClass('status-active status-inactive')
                        .addClass('status-badge status-' + originalStatus.toLowerCase())
                        .show();
                    $statusWrapper.remove();
                }

                // Restore UI
                $display.show();
                $input.hide();
                $editBtn.css('display', 'inline-flex');
                $saveBtn.hide();
                $cancelBtn.hide();
            });

            // Handle Keys in Input
            $input.on('keydown', function (e) {
                if (e.key === 'Enter') $saveBtn.click();
                if (e.key === 'Escape') $cancelBtn.click();
            });

            // ==========================================
            // TABS
            // ==========================================
            $('.tab').on('click', function () {
                $('.tab').removeClass('active');
                $(this).addClass('active');

                const tabName = $(this).data('tab');

                $('.tab-content').removeClass('active');
                $('#' + tabName + '-content').addClass('active');
            });

            // ==========================================
            // PAGINATION
            // ==========================================
            $(document).on('click', '.pagination button:not(:disabled)', function () {
                if (!$(this).hasClass('active') && /\d/.test($(this).text())) {
                    $(this).closest('.pagination').find('button').removeClass('active');
                    $(this).addClass('active');
                }
            });

            // ==========================================
            // SEARCH (FILTER)
            // ==========================================
            $('.table-search').on('keyup', function () {
                const searchTerm = $(this).val().toLowerCase();
                const $tabContent = $(this).closest('.tab-content');
                const $rows = $tabContent.find('.data-table tbody tr');
                let visibleCount = 0;

                $rows.each(function () {
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
    <script src="~/_js2/manageDivision.js"></script>
}
