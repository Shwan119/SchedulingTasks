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