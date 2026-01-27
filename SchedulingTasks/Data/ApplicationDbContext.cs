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
    <title>Field Mapping</title>
    <!-- Add jQuery CDN -->
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="styles.css">
    <style>
        /* ============================================
           PAGE SPECIFIC STYLES (Field Mapping)
           ============================================ */

        /* Details Grid */
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

        /* Edit Select Wrapper */
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

        /* LOB Details Section */
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

        /* UPDATED FILTER INPUT STYLES */
        .lob-filter-wrapper {
            padding: 12px 16px;
            background: #f8f9fa; /* Light grey container */
            border-bottom: 1px solid #e5e7eb;
            position: relative;
        }

        .lob-filter-icon {
            position: absolute;
            left: 28px; /* 16px padding + 12px inside input */
            top: 50%;
            transform: translateY(-50%);
            width: 16px;
            height: 16px;
            color: #6b7280;
            pointer-events: none;
            z-index: 1;
        }

        .lob-filter-input {
            width: 100%;
            padding: 10px 12px 10px 40px; /* Space for icon */
            font-size: 14px;
            border: 1px solid #d1d5db;
            border-radius: 6px;
            background: #fff;
            color: #333;
            outline: none;
            transition: all 0.2s ease;
        }

        .lob-filter-input:focus {
            border-color: #2563eb;
            box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.1);
        }

        .lob-filter-input::placeholder {
            color: #9ca3af;
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

        .checkbox-item:nth-child(6n+1), .checkbox-item:nth-child(6n+2), .checkbox-item:nth-child(6n+3) { background: #fff; }
        .checkbox-item:nth-child(6n+4), .checkbox-item:nth-child(6n+5), .checkbox-item:nth-child(6n+6) { background: #f9fafb; }

        .checkbox-label {
            display: flex;
            align-items: flex-start;
            gap: 10px;
            cursor: pointer;
            font-size: 14px;
            color: #333;
            line-height: 1.4;
        }

        .checkbox-label input[type="checkbox"]:disabled + .checkbox-custom { opacity: 0.6; cursor: not-allowed; }
        .checkbox-label input[type="checkbox"]:disabled ~ .checkbox-text { opacity: 0.7; }

        .lob-details-section.edit-mode .checkbox-label input[type="checkbox"]:disabled + .checkbox-custom,
        .lob-details-section.edit-mode .checkbox-label input[type="checkbox"]:disabled ~ .checkbox-text { opacity: 1; }

        .checkbox-label input[type="checkbox"] { display: none; }

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

        .checkbox-label input[type="checkbox"]:checked + .checkbox-custom { background: #0049ac; border-color: #0049ac; }
        .checkbox-label input[type="checkbox"]:checked + .checkbox-custom::after {
            content: '';
            width: 5px;
            height: 9px;
            border: solid #fff;
            border-width: 0 2px 2px 0;
            transform: rotate(45deg);
            margin-bottom: 2px;
        }

        .checkbox-text { flex: 1; word-break: break-word; }

        .lob-checkbox-grid::-webkit-scrollbar { width: 8px; }
        .lob-checkbox-grid::-webkit-scrollbar-track { background: #f1f1f1; border-radius: 4px; }
        .lob-checkbox-grid::-webkit-scrollbar-thumb { background: #c1c1c1; border-radius: 4px; }
        .lob-checkbox-grid::-webkit-scrollbar-thumb:hover { background: #a1a1a1; }

        .hidden { display: none !important; }

        @media (max-width: 1400px) { .details-grid { grid-template-columns: repeat(3, 1fr); } }
        @media (max-width: 1200px) { .lob-checkbox-grid { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 900px) { .details-grid { grid-template-columns: repeat(2, 1fr); } }
        @media (max-width: 768px) { 
            .lob-checkbox-grid { grid-template-columns: 1fr; } 
            .details-grid { grid-template-columns: 1fr; }
        }
    </style>
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
            <div class="details-grid" id="details-grid-container">
                <div class="detail-item" data-field="lobGroup">
                    <span class="detail-label">LOB Group</span>
                    <div class="detail-value-box">
                        <span class="display-value">WM Operations</span>
                    </div>
                </div>
                <div class="detail-item" data-field="reportingOrg">
                    <span class="detail-label">
                        Reporting Org
                        <span class="info-icon" title="Information about Reporting Org">ⓘ</span>
                    </span>
                    <div class="detail-value-box">
                        <span class="display-value">ECOE (ECOE Reporting & Analytics)</span>
                    </div>
                </div>
                <div class="detail-item" data-field="reportManager">
                    <span class="detail-label">Report Manager</span>
                    <div class="detail-value-box">
                        <span class="display-value empty">(Please Select)</span>
                    </div>
                </div>
                <div class="detail-item" data-field="reportLead">
                    <span class="detail-label">Report Lead</span>
                    <div class="detail-value-box">
                        <span class="display-value empty">(Please Select)</span>
                    </div>
                </div>
                <div class="detail-item" data-field="iDrive">
                    <span class="detail-label">iDrive</span>
                    <div class="detail-value-box">
                        <span class="display-value empty">(Please Select)</span>
                    </div>
                </div>
                <div class="detail-item" data-field="executive">
                    <span class="detail-label">Executive</span>
                    <div class="detail-value-box">
                        <span class="display-value empty">(Please Select)</span>
                    </div>
                </div>
            </div>

            <!-- LOB Details Section -->
            <div class="lob-details-section">
                <label class="lob-details-label">LOB Details</label>
                <div class="lob-details-container">
                    <!-- Filter Input -->
                    <div class="lob-filter-wrapper">
                        <!-- Search Icon -->
                        <svg class="lob-filter-icon" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                        </svg>
                        <input type="text" 
                               id="lobDetailsFilter" 
                               class="lob-filter-input" 
                               placeholder="Filter LOB Details...">
                    </div>
                    
                    <!-- Checkbox Grid -->
                    <div class="lob-checkbox-grid" id="lobCheckboxGrid">
                        <!-- Checkboxes will be populated by JavaScript -->
                    </div>
                </div>
            </div>

        </div>

    </div>

    <script>
        $(document).ready(function() {
            // LOB Details Options Data
            const lobDetailsOptions = [
                { id: 'wm-operations', label: 'WM Operations', checked: true },
                { id: 'adv-client', label: 'ADVANCED CLIENT SOLUTIONS BUS CONTROL', checked: false },
                { id: 'assessments', label: 'ASSESSMENTS DEFECT MGMT COMPLAINTS AMOI...AA(*)', checked: false },
                { id: 'business-banking', label: 'Business Banking', checked: false },
                { id: 'business-controls', label: 'BUSINESS CONTROLS', checked: false },
                { id: 'business-controls-vnbga', label: 'BUSINESS CONTROLS VNBGA...AB(*)', checked: false },
                { id: 'business-support', label: 'BUSINESS SUPPORT', checked: false },
                { id: 'card-business', label: 'CARD BUSINESS ENABLEMENT AND CONTROLS', checked: false },
                { id: 'card-products', label: 'Card Products', checked: false },
                { id: 'cardholder-benefits', label: 'Cardholder Benefits Vendor(*)', checked: true },
                { id: 'claims-resolution', label: 'Claims Resolution & Recovery', checked: false },
                { id: 'client-care', label: 'CLIENT CARE BANKING', checked: false },
                { id: 'client-exp', label: 'CLIENT EXP SALES CONTROLS ADMIN', checked: false },
                { id: 'client-srvcs', label: 'CLIENT SRVCS AND CRDT ASSIST BUS CO', checked: false },
                { id: 'comm-govt', label: 'Comm & Govt Prepaid', checked: false },
                { id: 'complaint-program', label: 'Complaint Program Governance, Engagement and Reporting', checked: false },
                { id: 'complaint-review', label: 'Complaint Review Unit', checked: false },
                { id: 'complaints-control', label: 'Complaints Control & Oversight', checked: false },
                { id: 'consumer-banking', label: 'Consumer Banking Operations', checked: false },
                { id: 'corporate-services', label: 'Corporate Services', checked: false },
                { id: 'credit-risk', label: 'Credit Risk Management', checked: false },
                { id: 'digital-platforms', label: 'Digital Platforms', checked: false },
                { id: 'enterprise-data', label: 'Enterprise Data Services', checked: false },
                { id: 'financial-crimes', label: 'Financial Crimes Prevention', checked: false }
            ];

            // Configuration for Dropdown Options
            const dropdownOptions = {
                "lobGroup": ["WM Operations", "ETS Issues Report", "LOB Group A", "LOB Group B"],
                "reportingOrg": ["ECOE (ECOE Reporting & Analytics)", "RAM Reporting Org", "Finance Org", "Operations Org"],
                "reportManager": ["(Please Select)", "Manager A", "Manager B", "Manager C"],
                "reportLead": ["(Please Select)", "Lead A", "Lead B", "Lead C"],
                "iDrive": ["(Please Select)", "iDrive A", "iDrive B", "iDrive C"],
                "executive": ["(Please Select)", "Executive A", "Executive B", "Executive C"]
            };

            // Store original state
            let originalValues = {};
            let originalLobSelections = {};
            let isEditMode = false;

            // Cache selectors
            const $editBtn = $('#edit-details-btn');
            const $saveBtn = $('#save-details-btn');
            const $cancelBtn = $('#cancel-details-btn');
            const $gridContainer = $('#details-grid-container');
            const $lobCheckboxGrid = $('#lobCheckboxGrid');
            const $lobFilter = $('#lobDetailsFilter');
            const $lobDetailsSection = $('.lob-details-section');

            // ==========================================
            // INITIALIZE LOB DETAILS CHECKBOXES
            // ==========================================
            function initializeLobCheckboxes() {
                $lobCheckboxGrid.empty();
                
                lobDetailsOptions.forEach(function(option) {
                    const $checkboxItem = $(`
                        <div class="checkbox-item" data-label="${option.label.toLowerCase()}">
                            <label class="checkbox-label">
                                <input type="checkbox" 
                                       id="${option.id}" 
                                       value="${option.id}"
                                       ${option.checked ? 'checked' : ''}
                                       disabled>
                                <span class="checkbox-custom"></span>
                                <span class="checkbox-text">${option.label}</span>
                            </label>
                        </div>
                    `);
                    $lobCheckboxGrid.append($checkboxItem);
                });
            }

            // Initialize on page load
            initializeLobCheckboxes();

            // ==========================================
            // LOB DETAILS FILTER
            // ==========================================
            $lobFilter.on('input', function() {
                const filterText = $(this).val().toLowerCase();
                
                $('.checkbox-item').each(function() {
                    const labelText = $(this).data('label');
                    if (labelText.includes(filterText)) {
                        $(this).show();
                    } else {
                        $(this).hide();
                    }
                });
            });

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
                isEditMode = true;
                
                // Toggle Buttons
                $editBtn.addClass('hidden');
                $saveBtn.removeClass('hidden');
                $cancelBtn.removeClass('hidden');

                // Save original values
                originalValues = {};
                $gridContainer.find('.detail-item').each(function() {
                    const field = $(this).data('field');
                    const value = $(this).find('.display-value').text().trim();
                    originalValues[field] = value;
                });

                // Save original LOB selections
                originalLobSelections = {};
                $lobCheckboxGrid.find('input[type="checkbox"]').each(function() {
                    originalLobSelections[$(this).attr('id')] = $(this).is(':checked');
                });

                // Add dropdown overlays to each field
                $gridContainer.find('.detail-item').each(function() {
                    const $item = $(this);
                    const field = $item.data('field');
                    const $valueBox = $item.find('.detail-value-box');
                    const currentText = $valueBox.find('.display-value').text().trim();
                    
                    // Hide display value
                    $valueBox.find('.display-value').hide();
                    
                    // Create select wrapper
                    const $wrapper = $('<div class="edit-select-wrapper"></div>');
                    const $select = $('<select></select>');
                    
                    const options = dropdownOptions[field] || [currentText];
                    
                    $.each(options, function(i, opt) {
                        const $option = $('<option></option>').val(opt).text(opt);
                        if(opt === currentText) $option.prop('selected', true);
                        $select.append($option);
                    });
                    
                    // Ensure current value is in list
                    if ($.inArray(currentText, options) === -1 && currentText !== "") {
                        const $option = $('<option></option>').val(currentText).text(currentText).prop('selected', true);
                        $select.prepend($option);
                    }

                    $wrapper.append($select);
                    $valueBox.append($wrapper);
                });

                // Enable LOB Details checkboxes
                $lobCheckboxGrid.find('input[type="checkbox"]').prop('disabled', false);
                $lobDetailsSection.addClass('edit-mode');
            });

            // ==========================================
            // SAVE CHANGES
            // ==========================================
            $saveBtn.on('click', function() {
                isEditMode = false;
                
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // Save values and remove dropdowns
                $gridContainer.find('.detail-item').each(function() {
                    const $item = $(this);
                    const $valueBox = $item.find('.detail-value-box');
                    const $select = $valueBox.find('select');
                    const $displayValue = $valueBox.find('.display-value');
                    
                    if ($select.length) {
                        const newValue = $select.val();
                        const isEmpty = newValue === '(Please Select)' || newValue === '';
                        
                        // Update display value
                        $displayValue.text(newValue);
                        $displayValue.toggleClass('empty', isEmpty);
                        
                        // Remove select wrapper
                        $valueBox.find('.edit-select-wrapper').remove();
                        
                        // Show display value
                        $displayValue.show();
                    }
                });

                // Disable LOB Details checkboxes
                $lobCheckboxGrid.find('input[type="checkbox"]').prop('disabled', true);
                $lobDetailsSection.removeClass('edit-mode');

                // Log saved data
                const selectedLobs = [];
                $lobCheckboxGrid.find('input[type="checkbox"]:checked').each(function() {
                    selectedLobs.push($(this).closest('.checkbox-item').find('.checkbox-text').text());
                });
                console.log('Saved LOB Details:', selectedLobs);
            });

            // ==========================================
            // CANCEL CHANGES
            // ==========================================
            $cancelBtn.on('click', function() {
                isEditMode = false;
                
                // Toggle Buttons
                $editBtn.removeClass('hidden');
                $saveBtn.addClass('hidden');
                $cancelBtn.addClass('hidden');

                // Restore original values and remove dropdowns
                $gridContainer.find('.detail-item').each(function() {
                    const $item = $(this);
                    const field = $item.data('field');
                    const $valueBox = $item.find('.detail-value-box');
                    const $displayValue = $valueBox.find('.display-value');
                    const originalValue = originalValues[field];
                    const isEmpty = originalValue === '(Please Select)' || originalValue === '';
                    
                    // Restore display value
                    $displayValue.text(originalValue);
                    $displayValue.toggleClass('empty', isEmpty);
                    
                    // Remove select wrapper
                    $valueBox.find('.edit-select-wrapper').remove();
                    
                    // Show display value
                    $displayValue.show();
                });

                // Restore LOB selections and disable checkboxes
                $lobCheckboxGrid.find('input[type="checkbox"]').each(function() {
                    const id = $(this).attr('id');
                    $(this).prop('checked', originalLobSelections[id]);
                    $(this).prop('disabled', true);
                });
                $lobDetailsSection.removeClass('edit-mode');
            });
        });
    </script>
</body>
</html>