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


//lob-contacts-form

<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>LOB & Contacts Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
		/* UNIQUE CSS FOR LOB CONTACTS */

        /* Row layouts specific to this form */
        .row-3-equal {
            grid-template-columns: repeat(3, 1fr);
        }

        .row-full {
            grid-template-columns: 1fr;
        }

        /* Contact row: ID field (smaller), Name field, Email field */
        .row-contact {
            grid-template-columns: 1fr 1.2fr 1.2fr;
        }

        /* Executives section - simple text display */
        .executives-section {
            margin-bottom: 20px;
        }

        .executives-label {
            font-size: 13px;
            font-weight: 600;
            color: var(--label-color);
            margin-bottom: 6px;
        }

        .executives-value {
            font-size: 14px;
            color: var(--label-color);
            padding: 10px 0;
            border-bottom: 1px solid var(--border-color);
        }

        /* Contact group - label spans the row, inputs are inline */
        .contact-group {
            display: flex;
            flex-direction: column;
            gap: 6px;
            margin-bottom: 20px;
        }

        .contact-inputs {
            display: grid;
            grid-template-columns: 1fr 1.2fr 1.2fr;
            gap: 24px;
        }

        /* RESPONSIVE */
        @media (max-width: 1200px) {
            .row-3-equal {
                grid-template-columns: repeat(2, 1fr);
            }
            .contact-inputs {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .row-3-equal,
            .contact-inputs {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- Row 1: LOB Group, LOB, LOB Detail -->
        <div class="form-row row-3-equal">
            <div class="form-group">
                <label class="form-label">
                    LOB Group <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="eco" selected>Enterprise Correspondence Operations</option>
                    <option value="other">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    LOB <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="ssi" selected>Statements Setup and Implementation</option>
                    <option value="other">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    LOB Detail <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="bss" selected>Business Support - Statements</option>
                    <option value="other">Other</option>
                </select>
            </div>
        </div>

        <!-- Executives Section -->
        <div class="executives-section">
            <div class="executives-label">Executives</div>
            <div class="executives-value">Weller, Jim</div>
        </div>

        <!-- Primary Customer Contact/Owner -->
        <div class="contact-group">
            <label class="form-label">
                Primary Customer Contact/Owner <span class="required">*</span>
                <span class="info-icon">i</span>
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" value="NBKKOLX">
                <input type="text" class="form-input readonly" value="Grant, Inna" readonly>
                <input type="text" class="form-input readonly" value="inna.grant@bofa.com" readonly>
            </div>
        </div>

        <!-- Reporting Team Owner -->
        <div class="contact-group">
            <label class="form-label">
                Reporting Team Owner <span class="required">*</span>
                <span class="info-icon">i</span>
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" value="NBKRQ1L">
                <input type="text" class="form-input readonly" value="Pal, Tom" readonly>
                <input type="text" class="form-input readonly" value="tom.pal@bofa.com" readonly>
            </div>
        </div>

        <!-- Primary Report Contact -->
        <div class="contact-group">
            <label class="form-label">
                Primary Report Contact <span class="required">*</span>
                <span class="info-icon">i</span>
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" value="NBK9UAI">
                <input type="text" class="form-input readonly" value="Ospina, Julio C" readonly>
                <input type="text" class="form-input readonly" value="julio.ospina@bofa.com" readonly>
            </div>
        </div>

        <!-- Backup Report Contact -->
        <div class="contact-group">
            <label class="form-label">
                Backup Report Contact <span class="required">*</span>
                <span class="info-icon">i</span>
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" value="NBKBQ8O">
                <input type="text" class="form-input readonly" value="Fujita, Chris" readonly>
                <input type="text" class="form-input readonly" value="christopher.fujita@bofa.com" readonly>
            </div>
        </div>

        <!-- Report Producer -->
        <div class="contact-group">
            <label class="form-label">
                Report Producer
                <span class="info-icon">i</span>
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" value="NBK9UAI">
                <input type="text" class="form-input readonly" value="Ospina, Julio C" readonly>
                <input type="text" class="form-input readonly" value="julio.ospina@bofa.com" readonly>
            </div>
        </div>

        <!-- Secondary Customer Contact/Owner -->
        <div class="contact-group">
            <label class="form-label">
                Secondary Customer Contact/Owner
            </label>
            <div class="contact-inputs">
                <input type="text" class="form-input" placeholder="Enter NBK or Email...">
                <input type="text" class="form-input readonly" value="" readonly>
                <input type="text" class="form-input readonly" value="" readonly>
            </div>
        </div>
    </div>
</body>
</html>



//report - details - form
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Report Details Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
		/* UNIQUE CSS FOR REPORT DETAILS */

        /* Row layouts specific to this form */
        .row-header {
            grid-template-columns: 2fr 1fr 1fr 1fr;
        }

        .row-full {
            grid-template-columns: 1fr;
        }

        .row-status {
            grid-template-columns: 1fr 1fr 1fr 1.2fr;
        }

        /* Smaller textarea for description fields */
        .form-textarea.medium {
            min-height: 80px;
        }

        /* Footer recorded date text */
        .form-footer {
            margin-top: 16px;
            padding-top: 8px;
        }

        .recorded-date {
            font-size: 13px;
            color: var(--text-muted);
            font-style: italic;
        }

		/* RESPONSIVE */

        @media (max-width: 1200px) {
            .row-header {
                grid-template-columns: repeat(2, 1fr);
            }
            .row-status {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .row-header,
            .row-status {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- Row 1: Report Name, Report Category, Legacy Report ID, ID -->
        <div class="form-row row-header">
            <div class="form-group">
                <label class="form-label">
                    Report Name <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="Statements Home Loans Dashboard">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Report Category <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="production" selected>Production</option>
                    <option value="development">Development</option>
                    <option value="testing">Testing</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Legacy Report ID
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="115">
            </div>

            <div class="form-group">
                <label class="form-label">ID</label>
                <input type="text" class="form-input readonly" value="100089" readonly>
            </div>
        </div>

        <!-- Row 2: Report Purpose (full width) -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Report Purpose <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <textarea class="form-textarea medium">Report provides statement related metrics to be used for monthly trending.</textarea>
            </div>
        </div>

        <!-- Row 3: Report Description (full width) -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Report Description <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <textarea class="form-textarea medium">Report compiles different statement metrics. Contributes to Statement dashboard.</textarea>
            </div>
        </div>

        <!-- Row 4: Report Status, Report Created Date, Retired Date, Retired Reason -->
        <div class="form-row row-status">
            <div class="form-group">
                <label class="form-label">
                    Report Status <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="active" selected>Active</option>
                    <option value="inactive">Inactive</option>
                    <option value="retired">Retired</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Report Created Date
                    <span class="info-icon">i</span>
                </label>
                <div class="date-input-wrapper">
                    <input type="text" class="form-input" value="05/21/2021">
                    <span class="calendar-icon">
                        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
                            <line x1="16" y1="2" x2="16" y2="6"></line>
                            <line x1="8" y1="2" x2="8" y2="6"></line>
                            <line x1="3" y1="10" x2="21" y2="10"></line>
                        </svg>
                    </span>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Retired Date
                    <span class="info-icon">i</span>
                </label>
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
            </div>

            <div class="form-group">
                <label class="form-label">
                    Retired Reason
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="" selected>--- Select Retired Reason ---</option>
                    <option value="deprecated">Deprecated</option>
                    <option value="replaced">Replaced</option>
                    <option value="obsolete">Obsolete</option>
                </select>
            </div>
        </div>

        <!-- Row 5: Retirement Comment (full width) -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Retirement Comment
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>
        </div>

        <!-- Footer: Recorded date -->
        <div class="form-footer">
            <span class="recorded-date">Recorded on 05/21/2021</span>
        </div>
    </div>
</body>
</html>


//report-form
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Report Management Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
		/* UNIQUE CSS FOR REPORT MANAGEMENT */
	

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

        /* RESPONSIVE */
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


//report - options - form
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Report Options Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
        /* UNIQUE CSS FOR REPORT OPTIONS */

        /* 
         * Responsive grid layout:
         * Large (default): 2 rows × 3 columns
         * Medium (≤1200px): 3 rows × 2 columns
         * Small (≤768px): 6 rows × 1 column
         */
        .options-grid {
            display: grid;
            gap: 24px;
            grid-template-columns: repeat(3, 1fr);
        }

        /* Medium screens: 2 columns */
        @media (max-width: 1200px) {
            .options-grid {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        /* Small screens: 1 column */
        @media (max-width: 768px) {
            .options-grid {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- All 6 fields in a single responsive grid -->
        <div class="options-grid">
            <!-- Item 1: Archive Only -->
            <div class="form-group">
                <label class="form-label">
                    Archive Only
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="N">
            </div>

            <!-- Item 2: Benefit Category -->
            <div class="form-group">
                <label class="form-label">
                    Benefit Category <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="customer-experience" selected>Customer Experience</option>
                    <option value="operational-efficiency">Operational Efficiency</option>
                    <option value="risk-management">Risk Management</option>
                    <option value="compliance">Compliance</option>
                </select>
            </div>

            <!-- Item 3: Exception Framework -->
            <div class="form-group">
                <label class="form-label">
                    Exception Framework (Y/N) <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="N">
            </div>

            <!-- Item 4: Non-Critical Use Spreadsheet -->
            <div class="form-group">
                <label class="form-label">
                    Non-Critical Use Spreadsheet
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="N">
            </div>

            <!-- Item 5: Report Tags -->
            <div class="form-group">
                <label class="form-label">
                    Report Tags
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <!-- Item 6: Supports a Model -->
            <div class="form-group">
                <label class="form-label">
                    Supports a Model
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input readonly" value="N" readonly>
            </div>
        </div>
    </div>
</body>
</html>


//report - schedule - form
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Report Schedule Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
        /* UNIQUE CSS FOR REPORT SCHEDULE */
		
        /* Row 1: 5 columns - Frequency, Level of Effort, Min/Max/Avg Time */
        .row-timing {
            grid-template-columns: 1fr 1fr 1fr 1fr 1fr;
        }

        /* Row 2: 4 columns - Delivery Method, SLA Time Zone, SLA Times */
        .row-delivery {
            grid-template-columns: 1fr 1fr 1fr 1fr;
        }

        /* Row 3: Link to Procedure, Calendar Days, Days of Week */
        .row-schedule {
            grid-template-columns: 1fr 1.2fr 1.5fr;
        }

        /* Checkbox group for days of week */
        .checkbox-group {
            display: flex;
            align-items: center;
            gap: 16px;
            padding: 10px 12px;
            background: var(--radio-box-bg);
            border: 1px solid var(--border-color);
            border-radius: 4px;
            flex-wrap: wrap;
        }

        .checkbox-option {
            display: flex;
            align-items: center;
            gap: 6px;
            cursor: pointer;
        }

        .checkbox-option input[type="checkbox"] {
            appearance: none;
            width: 16px;
            height: 16px;
            border: 2px solid var(--border-color);
            border-radius: 3px;
            cursor: pointer;
            position: relative;
            transition: border-color 0.2s, background-color 0.2s;
            background: white;
        }

        .checkbox-option input[type="checkbox"]:checked {
            border-color: var(--radio-selected);
            background-color: var(--radio-selected);
        }

        .checkbox-option input[type="checkbox"]:checked::after {
            content: '';
            position: absolute;
            top: 1px;
            left: 4px;
            width: 4px;
            height: 8px;
            border: solid white;
            border-width: 0 2px 2px 0;
            transform: rotate(45deg);
        }

        .checkbox-option label {
            font-size: 14px;
            cursor: pointer;
            user-select: none;
        }

        /* Smaller textarea for Link to Procedure */
        .form-textarea.small {
            min-height: 42px;
            resize: vertical;
        }

        /* RESPONSIVE */
        @media (max-width: 1200px) {
            .row-timing {
                grid-template-columns: repeat(3, 1fr);
            }
            .row-delivery {
                grid-template-columns: repeat(2, 1fr);
            }
            .row-schedule {
                grid-template-columns: 1fr 1fr;
            }
            .row-schedule .form-group:last-child {
                grid-column: 1 / -1;
            }
        }

        @media (max-width: 768px) {
            .row-timing,
            .row-delivery,
            .row-schedule {
                grid-template-columns: 1fr;
            }
            .row-schedule .form-group:last-child {
                grid-column: auto;
            }
            .checkbox-group {
                gap: 12px;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- Row 1: Report Frequency, Level of Effort, Min/Max/Avg Time -->
        <div class="form-row row-timing">
            <div class="form-group">
                <label class="form-label">
                    Report Frequency <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="daily">Daily</option>
                    <option value="weekly">Weekly</option>
                    <option value="monthly" selected>Monthly</option>
                    <option value="quarterly">Quarterly</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Level of Effort to Create
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="low" selected>Low</option>
                    <option value="medium">Medium</option>
                    <option value="high">High</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Min Time to Create Report (in mins)
                </label>
                <input type="text" class="form-input" value="30">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Max Time to Create Report (in mins)
                </label>
                <input type="text" class="form-input" value="60">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Avg Time to Create Report (in mins) <span class="required">*</span>
                </label>
                <input type="text" class="form-input" value="60">
            </div>
        </div>

        <!-- Row 2: Delivery Method, SLA Time Zone, Delivery Time SLA, Delivery Time SLA 2 -->
        <div class="form-row row-delivery">
            <div class="form-group">
                <label class="form-label">
                    Delivery Method <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="manual" selected>Manual Publish</option>
                    <option value="automated">Automated</option>
                    <option value="email">Email</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    SLA Time Zone <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="eastern" selected>Eastern</option>
                    <option value="central">Central</option>
                    <option value="mountain">Mountain</option>
                    <option value="pacific">Pacific</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Delivery Time SLA <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="11:00" selected>11:00</option>
                    <option value="12:00">12:00</option>
                    <option value="13:00">13:00</option>
                    <option value="14:00">14:00</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Delivery Time SLA 2
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="" selected>--- Select SLA Time 2 ---</option>
                    <option value="11:00">11:00</option>
                    <option value="12:00">12:00</option>
                    <option value="13:00">13:00</option>
                </select>
            </div>
        </div>

        <!-- Row 3: Link to Procedure, Calendar Days of Month, Days of Week -->
        <div class="form-row row-schedule">
			<div class="form-group">
                <label class="form-label">
                    Link to Procedure<span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Report Runs On (Calendar Days of Month)
                </label>
                <input type="text" class="form-input" placeholder="Days separated by comma: 1,3,15,24">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Report Runs On (Days of Week)
                </label>
                <div class="checkbox-group">
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-mon" name="days">
                        <label for="day-mon">Mon</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-tue" name="days">
                        <label for="day-tue">Tue</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-wed" name="days">
                        <label for="day-wed">Wed</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-thu" name="days">
                        <label for="day-thu">Thu</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-fri" name="days">
                        <label for="day-fri">Fri</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-sat" name="days">
                        <label for="day-sat">Sat</label>
                    </div>
                    <div class="checkbox-option">
                        <input type="checkbox" id="day-sun" name="days">
                        <label for="day-sun">Sun</label>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>



//report - technical - form
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Report Technical Details Form</title>
	<link rel="stylesheet" href="style.css">
    <style>
        /* UNIQUE CSS FOR TECHNICAL FORM */

        /* Row layouts specific to this form */
        .row-4-col {
            grid-template-columns: repeat(4, 1fr);
        }

        .row-2-col {
            grid-template-columns: repeat(2, 1fr);
        }

        .row-3-col {
            grid-template-columns: repeat(3, 1fr);
        }

        .row-full {
            grid-template-columns: 1fr;
        }

        /* Input with button (for Additional Data Platform, Additional App SOR) */
        .input-with-button {
            display: flex;
            gap: 8px;
        }

        .input-with-button .form-input {
            flex: 1;
        }

        .list-btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            width: 38px;
            height: 38px;
            background: var(--input-bg);
            border: 1px solid var(--info-icon-bg);
            border-radius: 4px;
            cursor: pointer;
            transition: background 0.2s;
            flex-shrink: 0;
        }

        .list-btn:hover {
            background: var(--radio-box-bg);
        }

        .list-btn svg {
            width: 18px;
            height: 18px;
            color: var(--info-icon-bg);
        }

        /* Contact row: ID field, Name field (readonly), Email field (readonly) */
        .contact-inputs {
            display: grid;
            grid-template-columns: repeat(3, 1fr);
            gap: 24px;
        }

        /* Associated section: input + disabled textarea below */
        .associated-section {
            display: flex;
            flex-direction: column;
            gap: 12px;
        }

        .form-textarea.disabled {
            background: var(--radio-box-bg);
            color: var(--text-muted);
            cursor: not-allowed;
            min-height: 80px;
        }

		/* RESPONSIVE */
        @media (max-width: 1200px) {
            .row-4-col {
                grid-template-columns: repeat(2, 1fr);
            }
            .row-3-col {
                grid-template-columns: repeat(2, 1fr);
            }
            .contact-inputs {
                grid-template-columns: repeat(2, 1fr);
            }
        }

        @media (max-width: 768px) {
            .row-4-col,
            .row-3-col,
            .row-2-col {
                grid-template-columns: 1fr;
            }
            .contact-inputs {
                grid-template-columns: 1fr;
            }
        }
    </style>
</head>
<body>
    <div class="form-container">
        <!-- Row 1: Report Type, Report Automation, Service ID, Additional Access Required -->
        <div class="form-row row-4-col">
            <div class="form-group">
                <label class="form-label">
                    Report Type <span class="required">*</span>
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="excel" selected>Excel</option>
                    <option value="pdf">PDF</option>
                    <option value="csv">CSV</option>
                    <option value="other">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Report Automation
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="manual" selected>Manual</option>
                    <option value="automated">Automated</option>
                    <option value="semi-automated">Semi-Automated</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Service ID
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Additional Access Required to View the Report
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>
        </div>

        <!-- Row 2: Primary Data Platform, Primary App SOR, Primary Database Name, Primary Schema Name -->
        <div class="form-row row-4-col">
            <div class="form-group">
                <label class="form-label">
                    Primary Data Platform
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="srp" selected>Strategic Reporting Platform - SRP - 20743</option>
                    <option value="other">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Primary App SOR
                    <span class="info-icon">i</span>
                </label>
                <select class="form-select">
                    <option value="srp" selected>Strategic Reporting Platform - SRP - 20743</option>
                    <option value="other">Other</option>
                </select>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Primary Database Name
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" value="Statement_Reporting">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Primary Schema Name
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>
        </div>

        <!-- Row 3: Additional Data Platform, Additional App SOR (with list buttons) -->
        <div class="form-row row-2-col">
            <div class="form-group">
                <label class="form-label">
                    Additional Data Platform
                </label>
                <div class="input-with-button">
                    <input type="text" class="form-input" placeholder="">
                    <button type="button" class="list-btn" title="View list">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <line x1="4" y1="6" x2="20" y2="6"></line>
                            <line x1="4" y1="12" x2="20" y2="12"></line>
                            <line x1="4" y1="18" x2="20" y2="18"></line>
                        </svg>
                    </button>
                </div>
            </div>

            <div class="form-group">
                <label class="form-label">
                    Additional App SOR
                </label>
                <div class="input-with-button">
                    <input type="text" class="form-input" placeholder="">
                    <button type="button" class="list-btn" title="View list">
                        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
                            <line x1="4" y1="6" x2="20" y2="6"></line>
                            <line x1="4" y1="12" x2="20" y2="12"></line>
                            <line x1="4" y1="18" x2="20" y2="18"></line>
                        </svg>
                    </button>
                </div>
            </div>
        </div>

        <!-- Row 4: Stored Procedure Name, Script Name, Job Name -->
        <div class="form-row row-3-col">
            <div class="form-group">
                <label class="form-label">
                    Stored Procedure Name
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Script Name
                    <span class="info-icon">i</span>
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>

            <div class="form-group">
                <label class="form-label">
                    Job Name
                </label>
                <input type="text" class="form-input" placeholder="">
            </div>
        </div>

        <!-- Row 5: Tech Support Contact (3 inputs in a row) -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Tech Support Contact
                    <span class="info-icon">i</span>
                </label>
                <div class="contact-inputs">
                    <input type="text" class="form-input" placeholder="Enter NBK or Email...">
                    <input type="text" class="form-input readonly" value="" readonly>
                    <input type="text" class="form-input readonly" value="" readonly>
                </div>
            </div>
        </div>

        <!-- Row 6: Associated Reports -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Associated Reports
                </label>
                <div class="associated-section">
                    <input type="text" class="form-input" placeholder="You can enter multiple Report ID separated by Space or Comma">
                    <textarea class="form-textarea disabled" disabled></textarea>
                </div>
            </div>
        </div>

        <!-- Row 7: Associated Processes -->
        <div class="form-row row-full">
            <div class="form-group">
                <label class="form-label">
                    Associated Processes
                </label>
                <div class="associated-section">
                    <input type="text" class="form-input" placeholder="You can enter multiple Process ID separated by Space or Comma">
                    <textarea class="form-textarea disabled" disabled></textarea>
                </div>
            </div>
        </div>
    </div>
</body>
</html>


//style
/* GLOBAL / SHARED VARIABLES */
:root {
    --primary-blue: #0078d4;
    --info-icon-bg: #0891b2;
    --border-color: #d1d5db;
    --input-bg: #ffffff;
    --form-bg: #ffffff;
    --label-color: #1f2937;
    --text-muted: #6b7280;
    --radio-selected: #0ea5e9;
    --radio-box-bg: #f3f4f6;
    --trigger-btn-bg: #fbbf24;
    --trigger-btn-hover: #f59e0b;
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

/* CONTAINER & LAYOUT */
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

.form-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
}

/* LABELS & ICONS */
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

.info-text {
    font-size: 13px;
    color: var(--text-muted);
    font-style: italic;
}

/* INPUTS, SELECTS, TEXTAREAS */
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
    width: 100%;
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

/* Read-only / Disabled States */
.form-input.readonly,
.form-textarea.readonly,
.form-textarea.disabled {
    background: var(--radio-box-bg);
    color: var(--label-color);
    cursor: default;
}

.form-textarea.disabled {
    color: var(--text-muted);
    cursor: not-allowed;
}

.form-input.readonly:focus,
.form-textarea.readonly:focus {
    border-color: var(--border-color);
    box-shadow: none;
}

/* SHARED UTILITIES (Date Inputs) */
.date-input-wrapper {
    position: relative;
    display: flex;
    align-items: center;
    width: 100%;
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