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



/* ============================================
   GLOBAL RESET & TYPOGRAPHY
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

h1 {
    font-size: 20px;
    font-weight: 600;
    margin-bottom: 24px;
    color: #1a1a1a;
}

.container {
    max-width: 1400px;
    margin: 0 auto;
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    padding: 24px 32px;
}

/* ============================================
   FORMS, SELECTORS & INPUTS
   ============================================ */
/* Unified class for selector rows (handles variations across files) */
.division-selector,
.selector-row {
    display: flex;
    align-items: center;
    gap: 16px;
    margin-bottom: 24px;
}

.division-selector label,
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

/* Helper for wider selects (used in Field Values) */
.select-wrapper.wide {
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

/* SVG Search Icon */
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

.inline-input.wide {
    max-width: 300px;
}

/* ============================================
   BUTTONS
   ============================================ */
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

/* Action Icon Buttons (Edit, More Options) */
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

/* Circle Action Buttons (Approve/Reject) */
.action-buttons {
    display: flex;
    gap: 24px;
    justify-content: flex-end;
    align-items: center;
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

/* ============================================
   TABS
   ============================================ */
.tabs {
    display: flex;
    margin-bottom: 24px;
    position: relative;
    border-bottom: 1px solid #c5c5c5;
    flex-wrap: wrap;
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

.tab-content {
    display: none;
}

.tab-content.active {
    display: block;
}

/* ============================================
   CARDS & SECTIONS
   ============================================ */
/* Unified styling for cards across different pages */
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

.division-actions {
    display: flex;
    gap: 8px;
}

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

/* ============================================
   GRIDS & DETAILS
   ============================================ */
.details-grid {
    display: grid;
    grid-template-columns: repeat(6, 1fr);
    gap: 24px;
    padding: 20px 0;
}

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
}

/* ============================================
   DATA TABLES
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

/* Sort Icons */
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
    border-left: 4px solid transparent;
    border-right: 4px solid transparent;
}

.data-table th .sort-icon .up {
    border-bottom: 5px solid #999;
}

.data-table th .sort-icon .down {
    border-top: 5px solid #999;
}

/* Table Body */
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

.data-table tbody tr.editing {
    background: #fffde7;
}

.data-table tbody tr.editing:hover {
    background: #fffde7;
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

/* ============================================
   STATUS INDICATORS
   ============================================ */
/* Simple Text Status */
.status-active {
    color: #4a7c59;
    font-weight: 500;
}

.status-inactive {
    color: #c53030;
    font-weight: 500;
}

/* Badge Status */
.status-badge {
    display: inline-block;
    padding: 4px 12px;
    font-size: 12px;
    font-weight: 500;
    border-radius: 4px;
}

/* Status colors within badges override text colors */
.status-badge.status-active,
.status-badge.status-approved {
    background: #4a7c59;
    color: #fff;
}

.status-badge.status-rejected {
    background: #c53030;
    color: #fff;
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
   DROPDOWN MENUS
   ============================================ */
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

    .division-selector,
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

    .details-grid {
        grid-template-columns: repeat(2, 1fr);
    }
    
    .section-header {
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