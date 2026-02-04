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
    <title>Report Management Form</title>
    <style>
        :root {
            --primary-blue: #0078d4;
            --info-icon-bg: #0891b2;
            --border-color: #d1d5db;
            --input-bg: #ffffff;
            --form-bg: #ffffff;
            --label-color: #1f2937;
            --trigger-btn-bg: #fbbf24;
            --trigger-btn-hover: #f59e0b;
            --text-muted: #6b7280;
            --radio-selected: #0ea5e9;
            --radio-box-bg: #f3f4f6;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Segoe UI', -apple-system, BlinkMacSystemFont, sans-serif;
            background: linear-gradient(135deg, #e0f2fe 0%, #f0f9ff 50%, #e8f4fd 100%);
            min-height: 100vh;
            padding: 20px;
            color: var(--label-color);
        }

        .form-container {
            max-width: 1400px;
            margin: 0 auto;
            background: var(--form-bg);
            border-radius: 8px;
            padding: 24px 28px;
            box-shadow: 0 4px 20px rgba(0, 120, 212, 0.08);
        }

        .form-row {
            display: grid;
            gap: 24px;
            margin-bottom: 20px;
            align-items: start;
        }

        /* Row 1: Report Visibility | Retention Period | Security Scope */
        .row-1 {
            grid-template-columns: 1fr 1fr 2fr;
        }

        /* Row 2: 4 Yes/No radio groups */
        .row-2 {
            grid-template-columns: 1.2fr 1fr 1.2fr 1.2fr;
        }

        /* Row 3: Report Ranking - limited width */
        .row-3 {
            grid-template-columns: 2fr 1fr;
        }

        /* Row 4: 4 ID input fields */
        .row-4 {
            grid-template-columns: 1fr 1fr 1fr 1fr;
        }

        /* Row 5: Dates and Attestation */
        .row-5 {
            grid-template-columns: 1fr 1fr 1fr 1fr;
            align-items: end;
        }

        .form-group {
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .form-label {
            display: flex;
            align-items: center;
            gap: 6px;
            font-size: 13px;
            font-weight: 500;
            color: var(--label-color);
        }

        .form-label .required {
            color: var(--primary-blue);
            font-weight: 600;
        }

        .info-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 16px;
            height: 16px;
            background: var(--info-icon-bg);
            color: white;
            border-radius: 50%;
            font-size: 10px;
            font-weight: 600;
            font-style: italic;
            font-family: Georgia, serif;
            cursor: help;
            flex-shrink: 0;
        }

        .form-select,
        .form-input,
        .form-textarea {
            padding: 10px 12px;
            border: 1px solid var(--border-color);
            border-radius: 4px;
            font-size: 14px;
            background: var(--input-bg);
            color: var(--label-color);
            transition: border-color 0.2s, box-shadow 0.2s;
        }

        .form-select:focus,
        .form-input:focus,
        .form-textarea:focus {
            outline: none;
            border-color: var(--primary-blue);
            box-shadow: 0 0 0 3px rgba(0, 120, 212, 0.1);
        }

        .form-select {
            cursor: pointer;
            appearance: none;
            background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='12' height='12' viewBox='0 0 12 12'%3E%3Cpath fill='%236b7280' d='M2 4l4 4 4-4'/%3E%3C/svg%3E");
            background-repeat: no-repeat;
            background-position: right 12px center;
            padding-right: 32px;
        }

        .form-textarea {
            min-height: 100px;
            resize: vertical;
        }

        /* Radio button container with grey background */
        .radio-box {
            display: flex;
            align-items: center;
            gap: 20px;
            padding: 10px 16px;
            background: var(--radio-box-bg);
            border-radius: 4px;
            border: 1px solid var(--border-color);
        }

        .radio-box.wide {
            gap: 28px;
        }

        .radio-option {
            display: flex;
            align-items: center;
            gap: 6px;
            cursor: pointer;
        }

        .radio-option input[type="radio"] {
            appearance: none;
            width: 18px;
            height: 18px;
            border: 2px solid var(--border-color);
            border-radius: 50%;
            cursor: pointer;
            position: relative;
            transition: border-color 0.2s;
            background: white;
        }

        .radio-option input[type="radio"]:checked {
            border-color: var(--radio-selected);
        }

        .radio-option input[type="radio"]:checked::after {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            transform: translate(-50%, -50%);
            width: 10px;
            height: 10px;
            background: var(--radio-selected);
            border-radius: 50%;
        }

        .radio-option label {
            font-size: 14px;
            cursor: pointer;
            user-select: none;
        }

        .date-input-wrapper {
            position: relative;
            display: flex;
            align-items: center;
        }

        .date-input-wrapper .form-input {
            width: 100%;
            padding-right: 36px;
        }

        .date-input-wrapper .calendar-icon {
            position: absolute;
            right: 10px;
            color: var(--text-muted);
            pointer-events: none;
        }

        .info-text {
            font-size: 13px;
            color: var(--text-muted);
            font-style: italic;
        }

        .date-group {
            display: flex;
            flex-direction: column;
            gap: 6px;
        }

        .date-with-info {
            display: flex;
            align-items: center;
            gap: 16px;
        }

        .date-with-info .date-input-wrapper {
            flex: 1;
        }

        .attestation-info-inline {
            display: flex;
            flex-direction: column;
            gap: 2px;
            white-space: nowrap;
        }

        .trigger-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            padding: 10px 20px;
            background: var(--trigger-btn-bg);
            border: none;
            border-radius: 4px;
            font-size: 14px;
            font-weight: 500;
            color: var(--label-color);
            cursor: pointer;
            transition: background 0.2s, transform 0.1s;
            white-space: nowrap;
        }

        .trigger-btn:hover {
            background: var(--trigger-btn-hover);
        }

        .trigger-btn:active {
            transform: scale(0.98);
        }

        .trigger-btn svg {
            width: 16px;
            height: 16px;
        }

        .ranking-row {
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .ranking-row .form-select {
            flex: 1;
        }

        .checkmark-icon {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 24px;
            height: 24px;
            background: var(--radio-selected);
            border-radius: 50%;
            color: white;
            flex-shrink: 0;
        }

        /* Responsive adjustments */
        @media (max-width: 1200px) {
            .row-1, .row-2, .row-4 {
                grid-template-columns: repeat(2, 1fr);
            }
            .row-3 {
                grid-template-columns: 1fr;
            }
            .row-5 {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .row-1, .row-2, .row-3, .row-4, .row-5 {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- Row 1: Report Visibility, Retention Period, Security Scope -->
        <div class="form-row row-1">
            <div class="form-group">
                <label class="form-label">
                    Report Visibility <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="internal" selected>Internal</option>
                    <option value="external">External</option>
                    <option value="confidential">Confidential</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Retention Period <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="3">3 years</option>
                    <option value="5">5 years</option>
                    <option value="7" selected>7 years</option>
                    <option value="10">10 years</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Security Scope <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <div class="radio-box wide">
                    <div class="radio-option">
                        <input type="radio" id="non-nppi" name="security-scope" checked>
                        <label for="non-nppi">Non-NPPI</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="nppi" name="security-scope">
                        <label for="nppi">NPPI</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="hrci" name="security-scope">
                        <label for="hrci">HRCI</label>
                    </div>
                </div>
            </div>
        </div>

        <!-- Row 2: Non-Financial Regulatory, GRRP Report, Critical Use, High Risk -->
        <div class="form-row row-2">
            <div class="form-group">
                <label class="form-label">
                    Non-Financial Regulatory Report <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <div class="radio-box">
                    <div class="radio-option">
                        <input type="radio" id="nfr-yes" name="non-financial">
                        <label for="nfr-yes">Yes</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="nfr-no" name="non-financial" checked>
                        <label for="nfr-no">No</label>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">
                    GRRP Report <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <div class="radio-box">
                    <div class="radio-option">
                        <input type="radio" id="grrp-yes" name="grrp-report">
                        <label for="grrp-yes">Yes</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="grrp-no" name="grrp-report" checked>
                        <label for="grrp-no">No</label>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Critical Use Spreadsheet <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <div class="radio-box">
                    <div class="radio-option">
                        <input type="radio" id="cus-yes" name="critical-use">
                        <label for="cus-yes">Yes</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="cus-no" name="critical-use" checked>
                        <label for="cus-no">No</label>
                    </div>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Is Report Part of High Risk List <span class="required">*</span>
                </label>
                <div class="radio-box">
                    <div class="radio-option">
                        <input type="radio" id="hrl-yes" name="high-risk">
                        <label for="hrl-yes">Yes</label>
                    </div>
                    <div class="radio-option">
                        <input type="radio" id="hrl-no" name="high-risk" checked>
                        <label for="hrl-no">No</label>
                    </div>
                </div>
            </div>
        </div>

        <!-- Row 3: Report Ranking (limited width - 2/3 of row) -->
        <div class="form-row row-3">
            <div class="form-group">
                <label class="form-label">
                    Report Ranking <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <div class="ranking-row">
                    <select class="form-select">
                        <option value="low" selected>Low because the report is used by management for assessing the day-to-day operations of a business unit</option>
                        <option value="medium">Medium</option>
                        <option value="high">High</option>
                    </select>
                    <span class="checkmark-icon">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round" width="14" height="14">
                            <polyline points="20 6 9 17 4 12"></polyline>
                        </svg>
                    </span>
                </div>
            </div>
            <!-- Empty column to limit Report Ranking width -->
            <div></div>
        </div>

        <!-- Row 4: Record Code, PRC/RAP ID, SPI ID, Associated Audit ID -->
        <div class="form-row row-4">
            <div class="form-group">
                <label class="form-label">
                    Record Code
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    PRC/RAP ID
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    SPI ID
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Associated Audit ID
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>
        </div>

        <!-- Row 5: Dates and Attestation -->
        <div class="form-row row-5">
            <div class="date-group">
                <label class="form-label">AccessReviewDueDate</label>
                <div class="date-with-info">
                    <div class="date-input-wrapper">
                        <input type="text" class="form-input" placeholder="mm/dd/yyyy">
                        <span class="calendar-icon">
                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                                <line x1="16" y1="2" x2="16" y2="6"></line>
                                <line x1="8" y1="2" x2="8" y2="6"></line>
                                <line x1="3" y1="10" x2="21" y2="10"></line>
                            </svg>
                        </span>
                    </div>
                    <span class="info-text">Last Access Reviewed : N/A</span>
                </div>
            </div>

            <div class="date-group">
                <label class="form-label">AttestationDueDate</label>
                <div class="date-with-info">
                    <div class="date-input-wrapper">
                        <input type="text" class="form-input" placeholder="mm/dd/yyyy">
                        <span class="calendar-icon">
                            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                                <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                                <line x1="16" y1="2" x2="16" y2="6"></line>
                                <line x1="8" y1="2" x2="8" y2="6"></line>
                                <line x1="3" y1="10" x2="21" y2="10"></line>
                            </svg>
                        </span>
                    </div>
                    <div class="attestation-info-inline">
                        <span class="info-text">Last Attested On : N/A</span>
                        <span class="info-text">Last Attestation Sent : N/A</span>
                    </div>
                </div>
            </div>

            <div class="form-group" style="justify-content: flex-end;">
                <button class="trigger-btn">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
                        <path d="M12 19V5M5 12l7-7 7 7"/>
                    </svg>
                    Trigger Attestation
                </button>
            </div>
        </div>

        <!-- Row 6: Report Methodology -->
        <div class="form-row" style="margin-top: 8px;">
            <div class="form-group">
                <label class="form-label">
                    Report Methodology
                    <span class="info-icon">i</span>
                </label>
                <textarea class="form-textarea" placeholder=""></textarea>
            </div>
        </div>
    </div>
</body>
</html>
