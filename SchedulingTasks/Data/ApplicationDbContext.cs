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

// first
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Manage Division</title>
    <style>
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

        .container {
            max-width: 1400px;
            margin: 0 auto;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            padding: 24px 32px;
        }

        h1 {
            font-size: 20px;
            font-weight: 600;
            margin-bottom: 24px;
            color: #1a1a1a;
        }

        /* Division Selector Row */
        .division-selector {
            display: flex;
            align-items: center;
            gap: 16px;
            margin-bottom: 24px;
        }

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

        .btn {
            padding: 10px 20px;
            font-size: 14px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: all 0.2s;
        }

        .btn-secondary {
            background: #6b7280;
            color: #fff;
            border: none;
        }

        .btn-secondary:hover {
            background: #5a6170;
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
            background: #0049ac;
            color: #fff;
            border: 1px solid #0049ac;
        }

        .btn-save:hover {
            background: #003d91;
        }

        .btn-cancel {
            background: #fff;
            color: #666;
            border: 1px solid #ccc;
        }

        .btn-cancel:hover {
            background: #f5f5f5;
        }

        /* Division Card */
        .division-card {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 16px 20px;
            background: #fafafa;
            border-radius: 6px;
            margin-bottom: 24px;
        }

        .division-info {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .division-name {
            font-size: 15px;
            font-weight: 600;
            color: #1a1a1a;
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

        .division-actions {
            display: flex;
            gap: 8px;
        }

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

        .status-approved {
            background: #4a7c59;
            color: #fff;
        }

        .status-rejected {
            background: #c53030;
            color: #fff;
        }

        /* Tabs */
        .tabs {
            display: flex;
            margin-bottom: 24px;
            position: relative;
            border-bottom: 1px solid #c5c5c5;
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

        /* Table Header Row */
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

        /* Data Table */
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

        .data-table th .sort-icon .up {
            border-bottom: 4px solid #999;
        }

        .data-table th .sort-icon .down {
            border-top: 4px solid #999;
        }

        .data-table td {
            padding: 16px;
            font-size: 14px;
            border-bottom: 1px solid #f0f0f0;
            color: #333;
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

        /* Action Buttons */
        .action-buttons {
            display: flex;
            gap: 24px;
            justify-content: flex-end;
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

        /* Pagination */
        .pagination-wrapper {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-top: 24px;
            padding-top: 16px;
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
            border-radius: 4px;
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

        /* Responsive adjustments */
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
        }
    </style>
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
                <span class="status-badge status-active">Active</span>
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
        <div id="pending-content" class="tab-content">
            <!-- Table Header -->
            <div class="table-header">
                <span class="total-count">Total (4)</span>
                <div class="search-box">
                    <input type="text" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <!-- Data Table -->
            <table class="data-table">
                <thead>
                    <tr>
                        <th>
                            NBK ID 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Requested by 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Email Address 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 200px;"></th>
                        <th>
                            Requested on 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
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
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M5 13l4 4L19 7"/>
                                    </svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M6 18L18 6M6 6l12 12"/>
                                    </svg>
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
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M5 13l4 4L19 7"/>
                                    </svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M6 18L18 6M6 6l12 12"/>
                                    </svg>
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
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M5 13l4 4L19 7"/>
                                    </svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M6 18L18 6M6 6l12 12"/>
                                    </svg>
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
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M5 13l4 4L19 7"/>
                                    </svg>
                                </button>
                                <button class="action-btn reject" title="Reject">
                                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                        <path d="M6 18L18 6M6 6l12 12"/>
                                    </svg>
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
                    <input type="text" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th>
                            NBK ID 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Requested by 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Email Address 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 200px;"></th>
                        <th>
                            Approved on 
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
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
        // Edit functionality
        let originalValue = '';
        const display = document.getElementById('division-name-display');
        const input = document.getElementById('division-name-input');
        const editBtn = document.getElementById('edit-btn');
        const saveBtn = document.getElementById('save-btn');
        const cancelBtn = document.getElementById('cancel-btn');

        editBtn.addEventListener('click', function() {
            originalValue = display.textContent;
            input.value = originalValue;

            display.style.display = 'none';
            input.style.display = 'block';
            editBtn.style.display = 'none';
            saveBtn.style.display = 'inline-block';
            cancelBtn.style.display = 'inline-block';

            input.focus();
            input.select();
        });

        saveBtn.addEventListener('click', function() {
            display.textContent = input.value;

            display.style.display = 'inline';
            input.style.display = 'none';
            editBtn.style.display = 'inline-block';
            saveBtn.style.display = 'none';
            cancelBtn.style.display = 'none';
        });

        cancelBtn.addEventListener('click', function() {
            input.value = originalValue;

            display.style.display = 'inline';
            input.style.display = 'none';
            editBtn.style.display = 'inline-block';
            saveBtn.style.display = 'none';
            cancelBtn.style.display = 'none';
        });

        // Handle Enter key to save and Escape to cancel
        input.addEventListener('keydown', function(e) {
            if (e.key === 'Enter') {
                saveBtn.click();
            } else if (e.key === 'Escape') {
                cancelBtn.click();
            }
        });

        // Tab switching functionality
        document.querySelectorAll('.tab').forEach(function(tab) {
            tab.addEventListener('click', function() {
                // Remove active class from all tabs
                document.querySelectorAll('.tab').forEach(function(t) {
                    t.classList.remove('active');
                });
                // Add active class to clicked tab
                this.classList.add('active');
                
                // Show/hide corresponding content
                const pendingContent = document.getElementById('pending-content');
                const approvedContent = document.getElementById('approved-content');
                
                if (this.getAttribute('data-tab') === 'pending') {
                    pendingContent.style.display = 'block';
                    approvedContent.style.display = 'none';
                } else {
                    pendingContent.style.display = 'none';
                    approvedContent.style.display = 'block';
                }
            });
        });

        // Pagination functionality
        document.querySelectorAll('.pagination button:not(:disabled)').forEach(function(btn) {
            btn.addEventListener('click', function() {
                if (!this.classList.contains('active') && this.textContent.match(/\d/)) {
                    const pagination = this.closest('.pagination');
                    pagination.querySelectorAll('button').forEach(function(b) {
                        b.classList.remove('active');
                    });
                    this.classList.add('active');
                }
            });
        });

        // Search functionality
        document.querySelectorAll('.search-box input').forEach(function(searchInput) {
            searchInput.addEventListener('input', function() {
                const searchTerm = this.value.toLowerCase();
                const tabContent = this.closest('.tab-content');
                const table = tabContent.querySelector('.data-table');
                const rows = table.querySelectorAll('tbody tr');
                let visibleCount = 0;

                rows.forEach(function(row) {
                    const nbkId = row.querySelector('.nbk-link') ? row.querySelector('.nbk-link').textContent.toLowerCase() : '';
                    const name = row.cells[1] ? row.cells[1].textContent.toLowerCase() : '';
                    const email = row.cells[2] ? row.cells[2].textContent.toLowerCase() : '';

                    if (nbkId.includes(searchTerm) || name.includes(searchTerm) || email.includes(searchTerm)) {
                        row.style.display = '';
                        visibleCount++;
                    } else {
                        row.style.display = 'none';
                    }
                });

                // Update total count
                const totalCount = tabContent.querySelector('.total-count');
                if (searchTerm === '') {
                    totalCount.textContent = 'Total (' + rows.length + ')';
                } else {
                    totalCount.textContent = 'Showing ' + visibleCount + ' of ' + rows.length;
                }
            });
        });
    </script>
</body>
</html>



// second
@model YourNamespace.Models.ManageDivisionViewModel

@{
    ViewBag.Title = "Manage Division";
    Layout = "~/Views/Shared/_Layout.cshtml";
}

<style>
    * {
        margin: 0;
        padding: 0;
        box-sizing: border-box;
    }

    .manage-division-container {
        font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif;
        background-color: #f5f5f5;
        color: #333;
        line-height: 1.5;
        padding: 20px;
    }

    .container {
        max-width: 1400px;
        margin: 0 auto;
        background: #fff;
        border-radius: 8px;
        box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
        padding: 24px 32px;
    }

    h1 {
        font-size: 20px;
        font-weight: 600;
        margin-bottom: 24px;
        color: #1a1a1a;
    }

    /* Division Selector Row */
    .division-selector {
        display: flex;
        align-items: center;
        gap: 16px;
        margin-bottom: 24px;
    }

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

    .btn {
        padding: 10px 20px;
        font-size: 14px;
        border-radius: 4px;
        cursor: pointer;
        font-weight: 500;
        transition: all 0.2s;
    }

    .btn-secondary {
        background: #6b7280;
        color: #fff;
        border: none;
    }

    .btn-secondary:hover {
        background: #5a6170;
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
        background: #0049ac;
        color: #fff;
        border: 1px solid #0049ac;
    }

    .btn-save:hover {
        background: #003d91;
    }

    .btn-cancel {
        background: #fff;
        color: #666;
        border: 1px solid #ccc;
    }

    .btn-cancel:hover {
        background: #f5f5f5;
    }

    /* Division Card */
    .division-card {
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding: 16px 20px;
        background: #fafafa;
        border-radius: 6px;
        margin-bottom: 24px;
    }

    .division-info {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .division-name {
        font-size: 15px;
        font-weight: 600;
        color: #1a1a1a;
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

    .division-actions {
        display: flex;
        gap: 8px;
    }

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

    .status-approved {
        background: #4a7c59;
        color: #fff;
    }

    .status-rejected {
        background: #c53030;
        color: #fff;
    }

    /* Tabs */
    .tabs {
        display: flex;
        margin-bottom: 24px;
        position: relative;
        border-bottom: 1px solid #c5c5c5;
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

    /* Table Header Row */
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

    /* Data Table */
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

    .data-table th .sort-icon .up {
        border-bottom: 4px solid #999;
    }

    .data-table th .sort-icon .down {
        border-top: 4px solid #999;
    }

    .data-table td {
        padding: 16px;
        font-size: 14px;
        border-bottom: 1px solid #f0f0f0;
        color: #333;
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

    /* Action Buttons */
    .action-buttons {
        display: flex;
        gap: 24px;
        justify-content: flex-end;
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

    /* Pagination */
    .pagination-wrapper {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 24px;
        padding-top: 16px;
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
        border-radius: 4px;
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

    /* Responsive adjustments */
    @@media (max-width: 1024px) {
        .data-table {
            display: block;
            overflow-x: auto;
        }
    }

    @@media (max-width: 768px) {
        .container {
            padding: 16px;
        }

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
    }
</style>

<div class="manage-division-container">
    <div class="container">
        <h1>Manage Division</h1>

        @using (Html.BeginForm("Index", "ManageDivision", FormMethod.Post, new { id = "divisionForm" }))
        {
            @Html.AntiForgeryToken()

            <!-- Division Selector -->
            <div class="division-selector">
                @Html.LabelFor(m => m.SelectedDivisionId, "Division", new { @class = "division-label" })
                <div class="select-wrapper">
                    @Html.DropDownListFor(m => m.SelectedDivisionId, Model.DivisionList, new { @class = "form-control", id = "divisionDropdown" })
                </div>
                <button type="button" class="btn btn-secondary" id="viewFieldsBtn">View Fields</button>
            </div>

            <!-- Division Card -->
            <div class="division-card">
                <div class="division-info">
                    <span class="division-name" id="division-name-display">@Model.DivisionName</span>
                    @Html.TextBoxFor(m => m.DivisionName, new { @class = "division-name-input", id = "division-name-input", style = "display: none;" })
                    <span class="status-badge @(Model.IsActive ? "status-active" : "")">@(Model.IsActive ? "Active" : "Inactive")</span>
                </div>
                <div class="division-actions">
                    <button type="button" class="btn btn-outline" id="edit-btn">Edit</button>
                    <button type="submit" class="btn btn-save" id="save-btn" style="display: none;">Save</button>
                    <button type="button" class="btn btn-cancel" id="cancel-btn" style="display: none;">Cancel</button>
                </div>
            </div>
        }

        <!-- Tabs -->
        <div class="tabs">
            <div class="tab active" data-tab="pending">Pending Request</div>
            <div class="tab" data-tab="approved">Approved History</div>
        </div>

        <!-- Pending Request Content -->
        <div id="pending-content" class="tab-content">
            <!-- Table Header -->
            <div class="table-header">
                <span class="total-count" id="pending-total-count">Total (@Model.PendingRequests.Count())</span>
                <div class="search-box">
                    @Html.TextBox("PendingSearchTerm", "", new { @class = "form-control", id = "pending-search", placeholder = "Search by Report ID or Name" })
                </div>
            </div>

            <!-- Data Table -->
            <table class="data-table" id="pending-table">
                <thead>
                    <tr>
                        <th>
                            NBK ID
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Requested by
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Email Address
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 200px;"></th>
                        <th>
                            Requested on
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var request in Model.PendingRequests)
                    {
                        <tr data-request-id="@request.Id">
                            <td><a href="@Url.Action("Details", "Request", new { id = request.NbkId })" class="nbk-link">@request.NbkId</a></td>
                            <td>@request.RequestedBy</td>
                            <td class="email-text">@request.EmailAddress</td>
                            <td></td>
                            <td>@request.RequestedOn.ToString("MMM dd, yyyy hh:mm tt")</td>
                            <td>
                                <div class="action-buttons">
                                    <button type="button" class="action-btn approve" title="Approve" data-id="@request.Id" onclick="approveRequest(@request.Id)">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                            <path d="M5 13l4 4L19 7" />
                                        </svg>
                                    </button>
                                    <button type="button" class="action-btn reject" title="Reject" data-id="@request.Id" onclick="rejectRequest(@request.Id)">
                                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3">
                                            <path d="M6 18L18 6M6 6l12 12" />
                                        </svg>
                                    </button>
                                </div>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>

            <!-- Pagination -->
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-@Model.PendingRequests.Count() of @Model.PendingRequestsTotalCount</span>

                <div class="pagination">
                    <button type="button" disabled>«</button>
                    <button type="button" disabled>‹</button>
                    @for (int i = 1; i <= Model.PendingTotalPages; i++)
                    {
                        <button type="button" class="@(i == Model.PendingCurrentPage ? "active" : "")" onclick="goToPage(@i, 'pending')">@i</button>
                    }
                    <button type="button">›</button>
                    <button type="button">»</button>
                </div>

                <div class="per-page">
                    @Html.Label("Show per page:")
                    @Html.DropDownList("PendingPageSize", new SelectList(new[] { 10, 25, 50, 100 }, Model.PageSize), new { @class = "form-control", id = "pending-page-size", onchange = "changePageSize(this.value, 'pending')" })
                </div>
            </div>
        </div>

        <!-- Approved History Content -->
        <div id="approved-content" class="tab-content" style="display: none;">
            <div class="table-header">
                <span class="total-count" id="approved-total-count">Total (@Model.ApprovedHistory.Count())</span>
                <div class="search-box">
                    @Html.TextBox("ApprovedSearchTerm", "", new { @class = "form-control", id = "approved-search", placeholder = "Search by Report ID or Name" })
                </div>
            </div>

            <table class="data-table" id="approved-table">
                <thead>
                    <tr>
                        <th>
                            NBK ID
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Requested by
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Email Address
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 200px;"></th>
                        <th>
                            Approved on
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>Status</th>
                    </tr>
                </thead>
                <tbody>
                    @foreach (var history in Model.ApprovedHistory)
                    {
                        <tr>
                            <td><a href="@Url.Action("Details", "Request", new { id = history.NbkId })" class="nbk-link">@history.NbkId</a></td>
                            <td>@history.RequestedBy</td>
                            <td class="email-text">@history.EmailAddress</td>
                            <td></td>
                            <td>@history.ApprovedOn.ToString("MMM dd, yyyy hh:mm tt")</td>
                            <td>
                                <span class="status-badge @(history.Status == "Approved" ? "status-approved" : "status-rejected")">@history.Status</span>
                            </td>
                        </tr>
                    }
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-@Model.ApprovedHistory.Count() of @Model.ApprovedHistoryTotalCount</span>

                <div class="pagination">
                    <button type="button" disabled>«</button>
                    <button type="button" disabled>‹</button>
                    @for (int i = 1; i <= Model.ApprovedTotalPages; i++)
                    {
                        <button type="button" class="@(i == Model.ApprovedCurrentPage ? "active" : "")" onclick="goToPage(@i, 'approved')">@i</button>
                    }
                    <button type="button">›</button>
                    <button type="button">»</button>
                </div>

                <div class="per-page">
                    @Html.Label("Show per page:")
                    @Html.DropDownList("ApprovedPageSize", new SelectList(new[] { 10, 25, 50, 100 }, Model.PageSize), new { @class = "form-control", id = "approved-page-size", onchange = "changePageSize(this.value, 'approved')" })
                </div>
            </div>
        </div>

    </div>
</div>

@section Scripts {
    <script>
        // Edit functionality
        var originalValue = '';
        var display = document.getElementById('division-name-display');
        var input = document.getElementById('division-name-input');
        var editBtn = document.getElementById('edit-btn');
        var saveBtn = document.getElementById('save-btn');
        var cancelBtn = document.getElementById('cancel-btn');

        editBtn.addEventListener('click', function () {
            originalValue = display.textContent;
            input.value = originalValue;

            display.style.display = 'none';
            input.style.display = 'block';
            editBtn.style.display = 'none';
            saveBtn.style.display = 'inline-block';
            cancelBtn.style.display = 'inline-block';

            input.focus();
            input.select();
        });

        cancelBtn.addEventListener('click', function () {
            input.value = originalValue;

            display.style.display = 'inline';
            input.style.display = 'none';
            editBtn.style.display = 'inline-block';
            saveBtn.style.display = 'none';
            cancelBtn.style.display = 'none';
        });

        // Handle Enter key to save and Escape to cancel
        input.addEventListener('keydown', function (e) {
            if (e.key === 'Enter') {
                document.getElementById('divisionForm').submit();
            } else if (e.key === 'Escape') {
                cancelBtn.click();
            }
        });

        // Tab switching functionality
        document.querySelectorAll('.tab').forEach(function (tab) {
            tab.addEventListener('click', function () {
                // Remove active class from all tabs
                document.querySelectorAll('.tab').forEach(function (t) {
                    t.classList.remove('active');
                });
                // Add active class to clicked tab
                this.classList.add('active');

                // Show/hide corresponding content
                var pendingContent = document.getElementById('pending-content');
                var approvedContent = document.getElementById('approved-content');

                if (this.getAttribute('data-tab') === 'pending') {
                    pendingContent.style.display = 'block';
                    approvedContent.style.display = 'none';
                } else {
                    pendingContent.style.display = 'none';
                    approvedContent.style.display = 'block';
                }
            });
        });

        // Pagination functionality
        document.querySelectorAll('.pagination button:not(:disabled)').forEach(function (btn) {
            btn.addEventListener('click', function () {
                if (!this.classList.contains('active') && this.textContent.match(/\d/)) {
                    var pagination = this.closest('.pagination');
                    pagination.querySelectorAll('button').forEach(function (b) {
                        b.classList.remove('active');
                    });
                    this.classList.add('active');
                }
            });
        });

        // Search functionality for Pending Requests
        document.getElementById('pending-search').addEventListener('input', function () {
            filterTable(this.value, 'pending-table', 'pending-total-count');
        });

        // Search functionality for Approved History
        document.getElementById('approved-search').addEventListener('input', function () {
            filterTable(this.value, 'approved-table', 'approved-total-count');
        });

        function filterTable(searchTerm, tableId, totalCountId) {
            var term = searchTerm.toLowerCase();
            var table = document.getElementById(tableId);
            var rows = table.querySelectorAll('tbody tr');
            var visibleCount = 0;

            rows.forEach(function (row) {
                var nbkId = row.querySelector('.nbk-link') ? row.querySelector('.nbk-link').textContent.toLowerCase() : '';
                var name = row.cells[1] ? row.cells[1].textContent.toLowerCase() : '';
                var email = row.cells[2] ? row.cells[2].textContent.toLowerCase() : '';

                if (nbkId.indexOf(term) > -1 || name.indexOf(term) > -1 || email.indexOf(term) > -1) {
                    row.style.display = '';
                    visibleCount++;
                } else {
                    row.style.display = 'none';
                }
            });

            // Update total count
            var totalCount = document.getElementById(totalCountId);
            if (term === '') {
                totalCount.textContent = 'Total (' + rows.length + ')';
            } else {
                totalCount.textContent = 'Showing ' + visibleCount + ' of ' + rows.length;
            }
        }

        // Approve request
        function approveRequest(requestId) {
            if (confirm('Are you sure you want to approve this request?')) {
                $.ajax({
                    url: '@Url.Action("Approve", "ManageDivision")',
                    type: 'POST',
                    data: {
                        id: requestId,
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            location.reload();
                        } else {
                            alert('Error: ' + response.message);
                        }
                    },
                    error: function () {
                        alert('An error occurred while processing your request.');
                    }
                });
            }
        }

        // Reject request
        function rejectRequest(requestId) {
            if (confirm('Are you sure you want to reject this request?')) {
                $.ajax({
                    url: '@Url.Action("Reject", "ManageDivision")',
                    type: 'POST',
                    data: {
                        id: requestId,
                        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
                    },
                    success: function (response) {
                        if (response.success) {
                            location.reload();
                        } else {
                            alert('Error: ' + response.message);
                        }
                    },
                    error: function () {
                        alert('An error occurred while processing your request.');
                    }
                });
            }
        }

        // Pagination
        function goToPage(page, type) {
            var url = '@Url.Action("Index", "ManageDivision")';
            window.location.href = url + '?page=' + page + '&type=' + type;
        }

        // Change page size
        function changePageSize(size, type) {
            var url = '@Url.Action("Index", "ManageDivision")';
            window.location.href = url + '?pageSize=' + size + '&type=' + type;
        }

        // View Fields button
        document.getElementById('viewFieldsBtn').addEventListener('click', function () {
            var divisionId = document.getElementById('divisionDropdown').value;
            window.location.href = '@Url.Action("ViewFields", "ManageDivision")' + '?divisionId=' + divisionId;
        });
    </script>
}



// third
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using YourNamespace.Models;

namespace YourNamespace.Controllers
{
    public class ManageDivisionController : Controller
    {
        // GET: ManageDivision
        public ActionResult Index(int? page, int? pageSize, string type)
        {
            var model = new ManageDivisionViewModel
            {
                // Division dropdown
                SelectedDivisionId = 1,
                DivisionList = GetDivisionList(),
                DivisionName = "ETS (DWO)",
                IsActive = true,

                // Pagination settings
                PageSize = pageSize ?? 10,
                PendingCurrentPage = (type == "pending" ? page : 1) ?? 1,
                ApprovedCurrentPage = (type == "approved" ? page : 1) ?? 1,

                // Load data
                PendingRequests = GetPendingRequests(),
                PendingRequestsTotalCount = 4,
                PendingTotalPages = 6,

                ApprovedHistory = GetApprovedHistory(),
                ApprovedHistoryTotalCount = 12,
                ApprovedTotalPages = 3
            };

            return View(model);
        }

        // POST: ManageDivision (Save Division Name)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(ManageDivisionViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Save division name to database
                // _divisionService.UpdateDivisionName(model.SelectedDivisionId, model.DivisionName);

                TempData["SuccessMessage"] = "Division name updated successfully.";
                return RedirectToAction("Index");
            }

            // Reload dropdown and data if validation fails
            model.DivisionList = GetDivisionList();
            model.PendingRequests = GetPendingRequests();
            model.ApprovedHistory = GetApprovedHistory();

            return View(model);
        }

        // POST: Approve Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            try
            {
                // Approve the request in database
                // _requestService.ApproveRequest(id);

                return Json(new { success = true, message = "Request approved successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Reject Request
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            try
            {
                // Reject the request in database
                // _requestService.RejectRequest(id);

                return Json(new { success = true, message = "Request rejected successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: View Fields
        public ActionResult ViewFields(int divisionId)
        {
            // Redirect to View Fields page
            return RedirectToAction("Index", "Fields", new { divisionId = divisionId });
        }

        #region Helper Methods

        private IEnumerable<SelectListItem> GetDivisionList()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "DWO (ETS)" },
                new SelectListItem { Value = "2", Text = "Division 2" },
                new SelectListItem { Value = "3", Text = "Division 3" }
            };
        }

        private IEnumerable<PendingRequestViewModel> GetPendingRequests()
        {
            // Replace with actual database call
            return new List<PendingRequestViewModel>
            {
                new PendingRequestViewModel
                {
                    Id = 1,
                    NbkId = "ZK20GX8",
                    RequestedBy = "Jason Scott",
                    EmailAddress = "jason.scott@bofa.com",
                    RequestedOn = new DateTime(2025, 5, 21, 21, 30, 0)
                },
                new PendingRequestViewModel
                {
                    Id = 2,
                    NbkId = "ZK20GX8",
                    RequestedBy = "Jason Scott",
                    EmailAddress = "jason.scott@bofa.com",
                    RequestedOn = new DateTime(2025, 5, 21, 21, 30, 0)
                },
                new PendingRequestViewModel
                {
                    Id = 3,
                    NbkId = "ZK20GX8",
                    RequestedBy = "Jason Scott",
                    EmailAddress = "jason.scott@bofa.com",
                    RequestedOn = new DateTime(2025, 5, 21, 21, 30, 0)
                },
                new PendingRequestViewModel
                {
                    Id = 4,
                    NbkId = "ZK20GX8",
                    RequestedBy = "Jason Scott",
                    EmailAddress = "jason.scott@bofa.com",
                    RequestedOn = new DateTime(2025, 5, 21, 21, 30, 0)
                }
            };
        }

        private IEnumerable<ApprovedHistoryViewModel> GetApprovedHistory()
        {
            // Replace with actual database call
            return new List<ApprovedHistoryViewModel>
            {
                new ApprovedHistoryViewModel
                {
                    Id = 1,
                    NbkId = "ZK20GX8",
                    RequestedBy = "Jason Scott",
                    EmailAddress = "jason.scott@bofa.com",
                    ApprovedOn = new DateTime(2025, 5, 20, 14, 15, 0),
                    Status = "Approved"
                },
                new ApprovedHistoryViewModel
                {
                    Id = 2,
                    NbkId = "AB45XY2",
                    RequestedBy = "Sarah Johnson",
                    EmailAddress = "sarah.johnson@bofa.com",
                    ApprovedOn = new DateTime(2025, 5, 19, 11, 30, 0),
                    Status = "Approved"
                },
                new ApprovedHistoryViewModel
                {
                    Id = 3,
                    NbkId = "CD78MN3",
                    RequestedBy = "Mike Chen",
                    EmailAddress = "mike.chen@bofa.com",
                    ApprovedOn = new DateTime(2025, 5, 18, 16, 45, 0),
                    Status = "Rejected"
                },
                new ApprovedHistoryViewModel
                {
                    Id = 4,
                    NbkId = "EF90PQ4",
                    RequestedBy = "Emily Davis",
                    EmailAddress = "emily.davis@bofa.com",
                    ApprovedOn = new DateTime(2025, 5, 17, 9, 0, 0),
                    Status = "Approved"
                }
            };
        }

        #endregion
    }
}



// fourth
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace YourNamespace.Models
{
    public class ManageDivisionViewModel
    {
        // Division Selection
        [Display(Name = "Division")]
        public int SelectedDivisionId { get; set; }
        public IEnumerable<SelectListItem> DivisionList { get; set; }

        // Division Details
        [Display(Name = "Division Name")]
        [Required(ErrorMessage = "Division name is required")]
        [StringLength(100, ErrorMessage = "Division name cannot exceed 100 characters")]
        public string DivisionName { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }

        // Pending Requests
        public IEnumerable<PendingRequestViewModel> PendingRequests { get; set; }
        public int PendingRequestsTotalCount { get; set; }
        public int PendingCurrentPage { get; set; }
        public int PendingTotalPages { get; set; }

        // Approved History
        public IEnumerable<ApprovedHistoryViewModel> ApprovedHistory { get; set; }
        public int ApprovedHistoryTotalCount { get; set; }
        public int ApprovedCurrentPage { get; set; }
        public int ApprovedTotalPages { get; set; }

        // Pagination
        public int PageSize { get; set; }

        public ManageDivisionViewModel()
        {
            DivisionList = new List<SelectListItem>();
            PendingRequests = new List<PendingRequestViewModel>();
            ApprovedHistory = new List<ApprovedHistoryViewModel>();
            PageSize = 10;
            PendingCurrentPage = 1;
            ApprovedCurrentPage = 1;
        }
    }

    public class PendingRequestViewModel
    {
        public int Id { get; set; }

        [Display(Name = "NBK ID")]
        public string NbkId { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Display(Name = "Requested On")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy hh:mm tt}")]
        public DateTime RequestedOn { get; set; }
    }

    public class ApprovedHistoryViewModel
    {
        public int Id { get; set; }

        [Display(Name = "NBK ID")]
        public string NbkId { get; set; }

        [Display(Name = "Requested By")]
        public string RequestedBy { get; set; }

        [Display(Name = "Email Address")]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Display(Name = "Approved On")]
        [DisplayFormat(DataFormatString = "{0:MMM dd, yyyy hh:mm tt}")]
        public DateTime ApprovedOn { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } // "Approved" or "Rejected"
    }
}




<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Reporting Organisation</title>
    <!-- Link to shared styles - in production, extract common CSS to a shared file -->
    <style>
        /* ============================================
           SHARED STYLES (reused from manage-division)
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

        .container {
            max-width: 1400px;
            margin: 0 auto;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            padding: 24px 32px;
        }

        h1 {
            font-size: 20px;
            font-weight: 600;
            margin-bottom: 24px;
            color: #1a1a1a;
        }

        /* Division/Organisation Selector Row */
        .selector-row {
            display: flex;
            align-items: center;
            gap: 16px;
            margin-bottom: 24px;
        }

        .selector-row label {
            font-size: 14px;
            color: #666;
            font-weight: 500;
        }

        .select-wrapper {
            position: relative;
            flex: 1;
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

        /* Buttons */
        .btn {
            padding: 10px 20px;
            font-size: 14px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: all 0.2s;
        }

        .btn-secondary {
            background: #6b7280;
            color: #fff;
            border: none;
        }

        .btn-secondary:hover {
            background: #5a6170;
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
            background: #0049ac;
            color: #fff;
            border: 1px solid #0049ac;
        }

        .btn-save:hover {
            background: #003d91;
        }

        .btn-cancel {
            background: #fff;
            color: #666;
            border: 1px solid #ccc;
        }

        .btn-cancel:hover {
            background: #f5f5f5;
        }

        /* Status Badge */
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

        /* Tabs */
        .tabs {
            display: flex;
            margin-bottom: 24px;
            position: relative;
            border-bottom: 1px solid #c5c5c5;
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

        /* ============================================
           PAGE-SPECIFIC STYLES (Reporting Organisation)
           ============================================ */

        /* Info Card */
        .info-card {
            display: flex;
            align-items: center;
            justify-content: space-between;
            padding: 16px 20px;
            background: #fafafa;
            border-radius: 6px;
            margin-bottom: 24px;
        }

        .info-card-left {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .info-card-title {
            font-size: 15px;
            font-weight: 600;
            color: #1a1a1a;
        }

        /* Details Grid */
        .details-grid {
            display: grid;
            grid-template-columns: repeat(6, 1fr);
            gap: 24px;
            padding: 20px 0;
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
        }

        /* Tab Content */
        .tab-content {
            display: none;
        }

        .tab-content.active {
            display: block;
        }

        /* Responsive adjustments */
        @media (max-width: 1200px) {
            .details-grid {
                grid-template-columns: repeat(3, 1fr);
            }
        }

        @media (max-width: 768px) {
            .container {
                padding: 16px;
            }

            .selector-row {
                flex-wrap: wrap;
            }

            .select-wrapper {
                max-width: 100%;
                width: 100%;
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
        }

        @media (max-width: 480px) {
            .details-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>
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
                    <span class="status-badge status-active">Active</span>
                </div>
                <button class="btn btn-outline" id="edit-details-btn">Edit Details</button>
            </div>

            <!-- Details Grid -->
            <div class="details-grid">
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
        // Tab switching functionality
        document.querySelectorAll('.tab').forEach(function(tab) {
            tab.addEventListener('click', function() {
                // Remove active class from all tabs
                document.querySelectorAll('.tab').forEach(function(t) {
                    t.classList.remove('active');
                });
                // Add active class to clicked tab
                this.classList.add('active');

                // Hide all tab contents
                document.querySelectorAll('.tab-content').forEach(function(content) {
                    content.classList.remove('active');
                });

                // Show corresponding content
                var tabName = this.getAttribute('data-tab');
                document.getElementById(tabName + '-content').classList.add('active');
            });
        });

        // Edit Details button functionality
        document.getElementById('edit-details-btn').addEventListener('click', function() {
            // Redirect to edit page or open modal
            alert('Edit Details clicked - implement your edit functionality here');
        });
    </script>
</body>
</html>





// resource
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Resources</title>
    <style>
        /* ============================================
           SHARED STYLES (reused from manage-division)
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

        .container {
            max-width: 1400px;
            margin: 0 auto;
            background: #fff;
            border-radius: 8px;
            box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
            padding: 24px 32px;
        }

        h1 {
            font-size: 20px;
            font-weight: 600;
            margin-bottom: 24px;
            color: #1a1a1a;
        }

        /* Selector Row */
        .selector-row {
            display: flex;
            align-items: center;
            gap: 16px;
            margin-bottom: 24px;
        }

        .selector-row label {
            font-size: 14px;
            color: #666;
            font-weight: 500;
        }

        .select-wrapper {
            position: relative;
            flex: 1;
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

        /* Buttons */
        .btn {
            padding: 10px 20px;
            font-size: 14px;
            border-radius: 4px;
            cursor: pointer;
            font-weight: 500;
            transition: all 0.2s;
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
            display: inline-flex;
            align-items: center;
            gap: 6px;
        }

        .btn-primary:hover {
            background: #162d4a;
        }

        /* Tabs */
        .tabs {
            display: flex;
            margin-bottom: 24px;
            position: relative;
            border-bottom: 1px solid #c5c5c5;
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

        /* Table Header Row */
        .table-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 16px;
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

        /* Data Table */
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

        .data-table th .sort-icon {
            display: inline-flex;
            flex-direction: column;
            margin-left: 8px;
            vertical-align: middle;
            gap: 2px;
        }

        .data-table th .sort-icon span {
            display: block;
            width: 0;
            height: 0;
            border-left: 5px solid transparent;
            border-right: 5px solid transparent;
        }

        .data-table th .sort-icon .up {
            border-bottom: 6px solid #1e3a5f;
        }

        .data-table th .sort-icon .down {
            border-top: 6px solid #1e3a5f;
        }

        .data-table td {
            padding: 16px;
            font-size: 14px;
            border-bottom: 1px solid #f0f0f0;
            color: #333;
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

        /* Editing Row */
        .data-table tbody tr.editing {
            background: #fffde7;
        }

        .data-table tbody tr.editing:hover {
            background: #fffde7;
        }

        /* Pagination */
        .pagination-wrapper {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-top: 24px;
            padding-top: 16px;
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
            border-radius: 4px;
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
           PAGE-SPECIFIC STYLES (Resources)
           ============================================ */

        /* Status Text */
        .status-active {
            color: #4a7c59;
            font-weight: 500;
        }

        .status-inactive {
            color: #c53030;
            font-weight: 500;
        }

        /* Action Buttons */
        .action-buttons {
            display: flex;
            gap: 28px;
            justify-content: flex-end;
            align-items: center;
        }

        .action-btn {
            width: 24px;
            height: 24px;
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
            width: 12px;
            height: 12px;
            stroke-width: 3;
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

        /* Inline Edit Input */
        .inline-input {
            padding: 6px 10px;
            font-size: 14px;
            border: 1px solid #ccc;
            border-radius: 4px;
            width: 100%;
            max-width: 120px;
        }

        .inline-input:focus {
            outline: none;
            border-color: #0049ac;
            box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.2);
        }

        /* Tab Content */
        .tab-content {
            display: none;
        }

        .tab-content.active {
            display: block;
        }

        /* Responsive adjustments */
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

            .selector-row {
                flex-wrap: wrap;
            }

            .select-wrapper {
                max-width: 100%;
                width: 100%;
            }

            .table-header {
                flex-direction: column;
                gap: 12px;
                align-items: flex-start;
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
        }
    </style>
</head>
<body>
    <div class="container">
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
                <button class="btn btn-primary" id="add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" id="executives-search" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <!-- Data Table -->
            <table class="data-table" id="executives-table">
                <thead>
                    <tr>
                        <th>
                            ID
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            NBK ID
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th>
                            Full Name
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 300px;"></th>
                        <th>
                            Status
                            <span class="sort-icon">
                                <span class="up"></span>
                                <span class="down"></span>
                            </span>
                        </th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>123</td>
                        <td>ZKA6IED</td>
                        <td>Jason Scott</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr class="editing">
                        <td><input type="text" class="inline-input" value="123"></td>
                        <td><input type="text" class="inline-input" value="IMP"></td>
                        <td><input type="text" class="inline-input" value="Jason Scott"></td>
                        <td></td>
                        <td><span class="status-inactive">Inactive</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                </tbody>
            </table>

            <!-- Pagination -->
            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-5 of 5</span>

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

        <!-- Report Leads Tab Content -->
        <div id="report-leads-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th>ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Full Name <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 300px;"></th>
                        <th>Status <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>456</td>
                        <td>RL001</td>
                        <td>Sarah Johnson</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>457</td>
                        <td>RL002</td>
                        <td>Mike Chen</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-2 of 10</span>
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
                    <select><option>10</option><option>25</option><option>50</option><option>100</option></select>
                </div>
            </div>
        </div>

        <!-- Report Manager Tab Content -->
        <div id="report-manager-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th>ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Full Name <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 300px;"></th>
                        <th>Status <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>789</td>
                        <td>RM001</td>
                        <td>Emily Davis</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 10</span>
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
                    <select><option>10</option><option>25</option><option>50</option><option>100</option></select>
                </div>
            </div>
        </div>

        <!-- iDrives Tab Content -->
        <div id="idrives-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                        <path d="M12 5v14M5 12h14"/>
                    </svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table">
                <thead>
                    <tr>
                        <th>ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>NBK ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Full Name <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 300px;"></th>
                        <th>Status <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 100px;"></th>
                    </tr>
                </thead>
                <tbody>
                    <tr>
                        <td>101</td>
                        <td>ID001</td>
                        <td>Alex Thompson</td>
                        <td></td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                </tbody>
            </table>

            <div class="pagination-wrapper">
                <span class="showing-text">Showing 1-1 of 5</span>
                <div class="pagination">
                    <button disabled>«</button>
                    <button disabled>‹</button>
                    <button class="active">1</button>
                    <button>2</button>
                    <button>›</button>
                    <button>»</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option><option>100</option></select>
                </div>
            </div>
        </div>

    </div>

    <script>
        // Tab switching functionality
        document.querySelectorAll('.tab').forEach(function(tab) {
            tab.addEventListener('click', function() {
                // Remove active class from all tabs
                document.querySelectorAll('.tab').forEach(function(t) {
                    t.classList.remove('active');
                });
                // Add active class to clicked tab
                this.classList.add('active');

                // Hide all tab contents
                document.querySelectorAll('.tab-content').forEach(function(content) {
                    content.classList.remove('active');
                });

                // Show corresponding content
                var tabName = this.getAttribute('data-tab');
                document.getElementById(tabName + '-content').classList.add('active');
            });
        });

        // Pagination functionality
        document.querySelectorAll('.pagination button:not(:disabled)').forEach(function(btn) {
            btn.addEventListener('click', function() {
                if (!this.classList.contains('active') && this.textContent.match(/\d/)) {
                    var pagination = this.closest('.pagination');
                    pagination.querySelectorAll('button').forEach(function(b) {
                        b.classList.remove('active');
                    });
                    this.classList.add('active');
                }
            });
        });

        // Search functionality
        document.getElementById('executives-search').addEventListener('input', function() {
            filterTable(this.value, 'executives-table');
        });

        function filterTable(searchTerm, tableId) {
            var term = searchTerm.toLowerCase();
            var table = document.getElementById(tableId);
            var rows = table.querySelectorAll('tbody tr');

            rows.forEach(function(row) {
                var id = row.cells[0] ? row.cells[0].textContent.toLowerCase() : '';
                var nbkId = row.cells[1] ? row.cells[1].textContent.toLowerCase() : '';
                var name = row.cells[2] ? row.cells[2].textContent.toLowerCase() : '';

                if (id.indexOf(term) > -1 || nbkId.indexOf(term) > -1 || name.indexOf(term) > -1) {
                    row.style.display = '';
                } else {
                    row.style.display = 'none';
                }
            });
        }

        // Edit button functionality
        document.querySelectorAll('.edit-btn').forEach(function(btn) {
            btn.addEventListener('click', function() {
                var row = this.closest('tr');
                // Toggle editing class and convert cells to inputs
                alert('Edit row clicked - implement inline editing functionality');
            });
        });

        // Add New button functionality
        document.getElementById('add-new-btn').addEventListener('click', function() {
            alert('Add New clicked - implement add new row functionality');
        });
    </script>
</body>
</html>
