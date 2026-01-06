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

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Field Values</title>
    <style>
        * { margin: 0; padding: 0; box-sizing: border-box; }
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, 'Helvetica Neue', Arial, sans-serif; background-color: #f5f5f5; color: #333; line-height: 1.5; padding: 20px; }
        .container { max-width: 1400px; margin: 0 auto; background: #fff; border-radius: 8px; box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1); padding: 24px 32px; }
        h1 { font-size: 20px; font-weight: 600; margin-bottom: 24px; color: #1a1a1a; }
        .selector-row { display: flex; align-items: center; gap: 16px; margin-bottom: 24px; }
        .selector-row label { font-size: 14px; color: #666; font-weight: 500; }
        .select-wrapper { position: relative; flex: 1; max-width: 500px; }
        .select-wrapper select { width: 100%; padding: 10px 40px 10px 14px; font-size: 14px; border: 1px solid #ddd; border-radius: 4px; background: #fff; appearance: none; cursor: pointer; color: #333; }
        .select-wrapper::after { content: ''; position: absolute; right: 14px; top: 50%; transform: translateY(-50%); width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; border-top: 5px solid #666; pointer-events: none; }
        .btn { padding: 10px 20px; font-size: 14px; border-radius: 4px; cursor: pointer; font-weight: 500; transition: all 0.2s; }
        .btn-secondary { background: #6b7280; color: #fff; border: none; }
        .btn-secondary:hover { background: #5a6170; }
        .btn-primary { background: #1e3a5f; color: #fff; border: none; display: inline-flex; align-items: center; gap: 6px; }
        .btn-primary:hover { background: #162d4a; }
        .tabs { display: flex; margin-bottom: 24px; position: relative; border-bottom: 1px solid #c5c5c5; flex-wrap: wrap; }
        .tab { padding: 14px 30px; font-size: 15px; color: #1e3a5f; cursor: pointer; transition: all 0.2s; position: relative; background: transparent; font-weight: 500; margin-bottom: -1px; }
        .tab:hover { color: #1e3a5f; }
        .tab.active { color: #1a1a1a; background: #fff; border-left: 1px solid #c5c5c5; border-right: 1px solid #c5c5c5; border-top: 1px solid #c5c5c5; border-bottom: 1px solid #fff; }
        .tab.active::before { content: ''; position: absolute; top: -1px; left: -1px; right: -1px; height: 3px; background: #1e3a5f; }
        .table-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }
        .search-box { position: relative; }
        .search-box input { padding: 8px 14px 8px 36px; font-size: 13px; border: 1px solid #ddd; border-radius: 4px; width: 240px; background: #fff; }
        .search-box input:focus { outline: none; border-color: #999; }
        .search-box::before { content: ''; position: absolute; left: 12px; top: 50%; transform: translateY(-50%); width: 14px; height: 14px; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23999'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z'/%3E%3C/svg%3E"); background-size: contain; }
        .data-table { width: 100%; border-collapse: collapse; }
        .data-table th { text-align: left; padding: 12px 16px; font-size: 13px; font-weight: 500; color: #666; border-bottom: 1px solid #e5e5e5; white-space: nowrap; }
        .data-table th .sort-icon { display: inline-flex; flex-direction: column; margin-left: 8px; vertical-align: middle; gap: 2px; }
        .data-table th .sort-icon span { display: block; width: 0; height: 0; border-left: 5px solid transparent; border-right: 5px solid transparent; }
        .data-table th .sort-icon .up { border-bottom: 6px solid #1e3a5f; }
        .data-table th .sort-icon .down { border-top: 6px solid #1e3a5f; }
        .data-table td { padding: 12px 16px; font-size: 14px; border-bottom: 1px solid #f0f0f0; color: #333; }
        .data-table tbody tr:nth-child(odd) { background: #f7f7f8; }
        .data-table tbody tr:nth-child(even) { background: #fff; }
        .data-table tbody tr:hover { background: #e6fcff; }
        .data-table tbody tr.editing { background: #fffde7; }
        .data-table tbody tr.editing:hover { background: #fffde7; }
        .status-active { color: #4a7c59; font-weight: 500; }
        .status-inactive { color: #c53030; font-weight: 500; }
        .action-buttons { display: flex; gap: 28px; justify-content: flex-end; align-items: center; }
        .icon-btn { background: none; border: none; cursor: pointer; color: #333; padding: 4px; display: flex; align-items: center; justify-content: center; transition: color 0.2s; }
        .icon-btn:hover { color: #1e3a5f; }
        .icon-btn svg { width: 20px; height: 20px; }
        .dropdown { position: relative; display: inline-block; }
        .dropdown-menu { display: none; position: absolute; right: 0; top: 100%; background: #fff; border: 1px solid #ddd; border-radius: 4px; box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15); min-width: 120px; z-index: 100; }
        .dropdown-menu.show { display: block; }
        .dropdown-item { display: block; padding: 10px 16px; font-size: 14px; color: #333; text-decoration: none; cursor: pointer; transition: background 0.2s; }
        .dropdown-item:hover { background: #f5f5f5; }
        .inline-input { padding: 6px 10px; font-size: 14px; border: 1px solid #ccc; border-radius: 4px; width: 100%; max-width: 120px; }
        .inline-input:focus { outline: none; border-color: #0049ac; box-shadow: 0 0 0 2px rgba(0, 73, 172, 0.2); }
        .inline-input.wide { max-width: 300px; }
        .pagination-wrapper { display: flex; justify-content: space-between; align-items: center; margin-top: 24px; padding-top: 16px; }
        .showing-text { font-size: 13px; color: #666; }
        .pagination { display: flex; align-items: center; gap: 4px; }
        .pagination button { width: 32px; height: 32px; border: none; background: transparent; color: #666; cursor: pointer; font-size: 13px; border-radius: 4px; display: flex; align-items: center; justify-content: center; }
        .pagination button:hover:not(.active):not(:disabled) { background: #f0f0f0; }
        .pagination button.active { background: #2563eb; color: #fff; }
        .pagination button:disabled { color: #ccc; cursor: not-allowed; }
        .pagination .dots { padding: 0 4px; color: #666; }
        .per-page { display: flex; align-items: center; gap: 8px; font-size: 13px; color: #666; }
        .per-page select { padding: 6px 28px 6px 10px; font-size: 13px; border: 1px solid #ddd; border-radius: 4px; background: #fff; appearance: none; cursor: pointer; background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke='%23666'%3E%3Cpath stroke-linecap='round' stroke-linejoin='round' stroke-width='2' d='M19 9l-7 7-7-7'/%3E%3C/svg%3E"); background-repeat: no-repeat; background-position: right 8px center; background-size: 14px; }
        .tab-content { display: none; }
        .tab-content.active { display: block; }
        @media (max-width: 1024px) { .data-table { display: block; overflow-x: auto; } }
        @media (max-width: 768px) { .container { padding: 16px; } .selector-row { flex-wrap: wrap; } .select-wrapper { max-width: 100%; width: 100%; } .table-header { flex-direction: column; gap: 12px; align-items: flex-start; } .pagination-wrapper { flex-direction: column; gap: 16px; } .tab { padding: 12px 16px; font-size: 13px; } }
    </style>
</head>
<body>
    <div class="container">
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

        <div id="lob-group-content" class="tab-content active">
            <div class="table-header">
                <button class="btn btn-primary" id="add-new-btn">
                    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg>
                    New
                </button>
                <div class="search-box">
                    <input type="text" id="lob-group-search" placeholder="Search by Report ID or Name">
                </div>
            </div>

            <table class="data-table" id="lob-group-table">
                <thead>
                    <tr>
                        <th>ID <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>CD <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Short Name <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Full Name <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Description <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Created on <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th>Status <span class="sort-icon"><span class="up"></span><span class="down"></span></span></th>
                        <th style="width: 80px;"></th>
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
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>A12C</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td>https://tableup.bankofamericaSetsVolumeHistory/Dashboard</td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr class="editing">
                        <td><input type="text" class="inline-input" value="123"></td>
                        <td><input type="text" class="inline-input" value="A12C"></td>
                        <td><input type="text" class="inline-input" value="IMP"></td>
                        <td><input type="text" class="inline-input" value="Jason Scott"></td>
                        <td><input type="text" class="inline-input wide" value="https://tableup.bankofamericaSetsVolumeHistory/Dashboard"></td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td><span class="status-inactive">Inactive</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>A12C</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td>https://tableup.bankofamericaSetsVolumeHistory/Dashboard</td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
                    </tr>
                    <tr>
                        <td>123</td>
                        <td>A12C</td>
                        <td>IMP</td>
                        <td>Jason Scott</td>
                        <td>https://tableup.bankofamericaSetsVolumeHistory/Dashboard</td>
                        <td>May 21, 2025 09:30 PM</td>
                        <td><span class="status-active">Active</span></td>
                        <td>
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
                        </td>
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
                    <span class="dots">...</span>
                    <button>5</button>
                    <button>6</button>
                    <button>›</button>
                    <button>»</button>
                </div>
                <div class="per-page">
                    <span>Show per page:</span>
                    <select><option>10</option><option>25</option><option>50</option><option>100</option></select>
                </div>
            </div>
        </div>

        <div id="lob-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table"><thead><tr><th>ID</th><th>CD</th><th>Short Name</th><th>Full Name</th><th>Description</th><th>Created on</th><th>Status</th><th></th></tr></thead><tbody><tr><td>456</td><td>B34D</td><td>LOB1</td><td>Line of Business 1</td><td>Primary LOB</td><td>May 20, 2025</td><td><span class="status-active">Active</span></td><td></td></tr></tbody></table>
            <div class="pagination-wrapper"><span class="showing-text">Showing 1-2 of 2</span><div class="pagination"><button class="active">1</button></div><div class="per-page"><span>Show per page:</span><select><option>10</option></select></div></div>
        </div>

        <div id="lob-detail-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table"><thead><tr><th>ID</th><th>CD</th><th>Short Name</th><th>Full Name</th><th>Description</th><th>Created on</th><th>Status</th><th></th></tr></thead><tbody><tr><td>789</td><td>D56F</td><td>DET1</td><td>Detail 1</td><td>LOB Detail</td><td>May 18, 2025</td><td><span class="status-active">Active</span></td><td></td></tr></tbody></table>
            <div class="pagination-wrapper"><span class="showing-text">Showing 1-6 of 6</span><div class="pagination"><button class="active">1</button></div><div class="per-page"><span>Show per page:</span><select><option>10</option></select></div></div>
        </div>

        <div id="report-category-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table"><thead><tr><th>ID</th><th>CD</th><th>Short Name</th><th>Full Name</th><th>Description</th><th>Created on</th><th>Status</th><th></th></tr></thead><tbody><tr><td>101</td><td>E78G</td><td>CAT1</td><td>Category 1</td><td>Report Category</td><td>May 17, 2025</td><td><span class="status-active">Active</span></td><td></td></tr></tbody></table>
            <div class="pagination-wrapper"><span class="showing-text">Showing 1-4 of 4</span><div class="pagination"><button class="active">1</button></div><div class="per-page"><span>Show per page:</span><select><option>10</option></select></div></div>
        </div>

        <div id="data-environment-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table"><thead><tr><th>ID</th><th>CD</th><th>Short Name</th><th>Full Name</th><th>Description</th><th>Created on</th><th>Status</th><th></th></tr></thead><tbody><tr><td>201</td><td>F89H</td><td>ENV1</td><td>Production</td><td>Data Environment</td><td>May 16, 2025</td><td><span class="status-active">Active</span></td><td></td></tr></tbody></table>
            <div class="pagination-wrapper"><span class="showing-text">Showing 1-1 of 1</span><div class="pagination"><button class="active">1</button></div><div class="per-page"><span>Show per page:</span><select><option>10</option></select></div></div>
        </div>

        <div id="app-data-source-content" class="tab-content">
            <div class="table-header">
                <button class="btn btn-primary"><svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M12 5v14M5 12h14"/></svg> New</button>
                <div class="search-box"><input type="text" placeholder="Search by Report ID or Name"></div>
            </div>
            <table class="data-table"><thead><tr><th>ID</th><th>CD</th><th>Short Name</th><th>Full Name</th><th>Description</th><th>Created on</th><th>Status</th><th></th></tr></thead><tbody><tr><td>301</td><td>G90I</td><td>SRC1</td><td>Data Source 1</td><td>App Data Source</td><td>May 15, 2025</td><td><span class="status-active">Active</span></td><td></td></tr></tbody></table>
            <div class="pagination-wrapper"><span class="showing-text">Showing 1-10 of 19</span><div class="pagination"><button class="active">1</button><button>2</button></div><div class="per-page"><span>Show per page:</span><select><option>10</option></select></div></div>
        </div>
    </div>

    <script>
        document.querySelectorAll('.tab').forEach(function(tab) {
            tab.addEventListener('click', function() {
                document.querySelectorAll('.tab').forEach(function(t) { t.classList.remove('active'); });
                this.classList.add('active');
                document.querySelectorAll('.tab-content').forEach(function(content) { content.classList.remove('active'); });
                document.getElementById(this.getAttribute('data-tab') + '-content').classList.add('active');
            });
        });

        document.querySelectorAll('.more-btn').forEach(function(btn) {
            btn.addEventListener('click', function(e) {
                e.stopPropagation();
                document.querySelectorAll('.dropdown-menu').forEach(function(menu) { if (menu !== btn.nextElementSibling) menu.classList.remove('show'); });
                this.nextElementSibling.classList.toggle('show');
            });
        });

        document.addEventListener('click', function() {
            document.querySelectorAll('.dropdown-menu').forEach(function(menu) { menu.classList.remove('show'); });
        });

        document.querySelectorAll('.pagination button:not(:disabled)').forEach(function(btn) {
            btn.addEventListener('click', function() {
                if (!this.classList.contains('active') && this.textContent.match(/\d/)) {
                    var pagination = this.closest('.pagination');
                    pagination.querySelectorAll('button').forEach(function(b) { b.classList.remove('active'); });
                    this.classList.add('active');
                }
            });
        });

        document.getElementById('lob-group-search').addEventListener('input', function() {
            var term = this.value.toLowerCase();
            document.querySelectorAll('#lob-group-table tbody tr').forEach(function(row) {
                row.style.display = row.textContent.toLowerCase().indexOf(term) > -1 ? '' : 'none';
            });
        });

        document.getElementById('add-new-btn').addEventListener('click', function() {
            alert('Add New clicked - implement add new row functionality');
        });
    </script>
</body>
</html>
