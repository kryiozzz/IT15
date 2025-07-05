// AdminMachine.js - JavaScript for Machine Management functionality

$(document).ready(function() {
    // Show success and error messages as toast notifications
    showNotifications();
    
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Status change listeners
    $('#Status, #editStatus').change(function() {
        var statusField = $(this);
        var maintenanceDateField = statusField.closest('form').find('[name="LastMaintenanceDate"]');
        var notesSection = statusField.closest('form').find('#maintenanceNotesSection');
        
        if (statusField.val() === 'Under Maintenance') {
            // If setting to maintenance and no date is set, set to today
            if (!maintenanceDateField.val()) {
                var today = new Date().toISOString().split('T')[0];
                maintenanceDateField.val(today);
            }
            
            // Show maintenance notes section
            notesSection.slideDown();
        } else if (statusField.val() === 'Operational') {
            // If moving from maintenance to operational, check if maintenance date is set
            if (!maintenanceDateField.val()) {
                var today = new Date().toISOString().split('T')[0];
                maintenanceDateField.val(today);
            }
            
            // Hide notes section
            notesSection.slideUp();
        } else {
            // For offline status
            notesSection.slideUp();
        }
    });
    
    // Edit Machine Modal Population
    $('.edit-machine').click(function() {
        var id = $(this).data('id');
        var name = $(this).data('name');
        var type = $(this).data('type');
        var status = $(this).data('status');
        var maintenance = $(this).data('maintenance');
        var description = $(this).data('description') || '';
        
        $('#editMachineId').val(id);
        $('#editMachineName').val(name);
        $('#editMachineType').val(type);
        $('#editStatus').val(status);
        $('#editMaintenanceDate').val(maintenance);
        $('#editDescription').val(description);
        
        // Show/hide maintenance notes section based on status
        if (status === 'Under Maintenance') {
            $('#maintenanceNotesSection').show();
        } else {
            $('#maintenanceNotesSection').hide();
        }
    });
    
    // Quick actions - Set to maintenance
    $('.quick-maintenance').click(function(e) {
        e.preventDefault();
        
        var machineId = $(this).data('id');
        var machineName = $(this).data('name');
        
        if (confirm('Set machine "' + machineName + '" to maintenance mode?')) {
            $.ajax({
                url: '/AdminMachine/QuickSetMaintenance',
                type: 'POST',
                data: {
                    machineId: machineId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
                },
                success: function(response) {
                    if (response.success) {
                        showToast('Machine set to maintenance mode.', 'success');
                        // Reload the page after a short delay
                        setTimeout(function() {
                            location.reload();
                        }, 1500);
                    } else {
                        showToast(response.message || 'Failed to update machine status.', 'error');
                    }
                },
                error: function() {
                    showToast('An error occurred while processing your request.', 'error');
                }
            });
        }
    });
    
    // Quick actions - Set to operational
    $('.quick-operational').click(function(e) {
        e.preventDefault();
        
        var machineId = $(this).data('id');
        var machineName = $(this).data('name');
        
        if (confirm('Set machine "' + machineName + '" to operational mode?')) {
            $.ajax({
                url: '/AdminMachine/QuickSetOperational',
                type: 'POST',
                data: {
                    machineId: machineId,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
                },
                success: function(response) {
                    if (response.success) {
                        showToast('Machine set to operational mode.', 'success');
                        // Reload the page after a short delay
                        setTimeout(function() {
                            location.reload();
                        }, 1500);
                    } else {
                        showToast(response.message || 'Failed to update machine status.', 'error');
                    }
                },
                error: function() {
                    showToast('An error occurred while processing your request.', 'error');
                }
            });
        }
    });
    
    // Search and filtering functionality
    $('#searchMachine').on('keyup', function() {
        var value = $(this).val().toLowerCase();
        $("#machinesTable tbody tr").filter(function() {
            $(this).toggle($(this).text().toLowerCase().indexOf(value) > -1)
        });
    });
    
    // Status filter
    $('#statusFilter').change(function() {
        filterTable();
    });
    
    // Type filter
    $('#typeFilter').change(function() {
        filterTable();
    });
    
    // Reset filters
    $('#resetFilters').click(function() {
        $('#statusFilter').val('');
        $('#typeFilter').val('');
        $('#searchMachine').val('');
        $('#machinesTable tbody tr').show();
    });
    
    // Combined filter function
    function filterTable() {
        var statusFilter = $('#statusFilter').val().toLowerCase();
        var typeFilter = $('#typeFilter').val().toLowerCase();
        
        $("#machinesTable tbody tr").filter(function() {
            var statusMatch = statusFilter === '' || $(this).find('td:eq(3)').text().toLowerCase().indexOf(statusFilter) > -1;
            var typeMatch = typeFilter === '' || $(this).find('td:eq(2)').text().toLowerCase() === typeFilter;
            
            $(this).toggle(statusMatch && typeMatch);
        });
    }
    
    // Notification functions
    function showNotifications() {
        // Check for success message in TempData
        var successMessage = $('#successMessage').text();
        if (successMessage) {
            showToast(successMessage, 'success');
        }
        
        // Check for error message in TempData
        var errorMessage = $('#errorMessage').text();
        if (errorMessage) {
            showToast(errorMessage, 'error');
        }
    }
    
    function showToast(message, type) {
        var toastClass = type === 'success' ? 'bg-success' : 'bg-danger';
        var toast = `
            <div class="toast ${toastClass}" role="alert" aria-live="assertive" aria-atomic="true" data-delay="5000">
                <div class="toast-header">
                    <strong class="mr-auto">Notification</strong>
                    <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">
                        <span aria-hidden="true">&times;</span>
                    </button>
                </div>
                <div class="toast-body text-white">
                    ${message}
                </div>
            </div>
        `;
        
        $('#toastContainer').append(toast);
        $('.toast').toast('show');
        
        // Remove toast after it's hidden
        $('.toast').on('hidden.bs.toast', function() {
            $(this).remove();
        });
    }
    
    // Machine status chart
    if ($('#machineStatusChart').length) {
        var ctx = document.getElementById('machineStatusChart').getContext('2d');
        var operational = parseInt($('#operationalCount').val());
        var maintenance = parseInt($('#maintenanceCount').val());
        var offline = parseInt($('#offlineCount').val());
        
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Operational', 'Under Maintenance', 'Offline'],
                datasets: [{
                    data: [operational, maintenance, offline],
                    backgroundColor: [
                        'rgba(40, 167, 69, 0.8)',  // Green
                        'rgba(255, 193, 7, 0.8)',  // Yellow
                        'rgba(220, 53, 69, 0.8)'   // Red
                    ],
                    borderColor: [
                        'rgba(40, 167, 69, 1)',
                        'rgba(255, 193, 7, 1)',
                        'rgba(220, 53, 69, 1)'
                    ],
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                legend: {
                    position: 'bottom'
                }
            }
        });
    }
});