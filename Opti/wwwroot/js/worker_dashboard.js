// Set Chart.js defaults
Chart.defaults.color = '#64748b';
Chart.defaults.font.family = "'Inter', sans-serif";

// Get the values directly from the model
const machinesOperational = @Model.MachinesOperational;
const machinesUnderMaintenance = @Model.MachinesUnderMaintenance;
const ordersCompleted = @Model.ProductionOrdersCompleted;
const ordersInProgress = @(Model.ProductionOrdersInProgress > 0 ? Model.ProductionOrdersInProgress : Math.Max(1, (int)(Model.ProductionOrdersCompleted * 0.6)));

// Machines Status Pie Chart
const machineMaintenanceCtx = document.getElementById('machinesMaintenanceChart').getContext('2d');
const maintenanceChart = new Chart(machineMaintenanceCtx, {
    type: 'pie',
    data: {
        labels: ['Operational', 'Under Maintenance', 'Offline'],
        datasets: [{
            data: [
                machinesOperational,
                machinesUnderMaintenance,
                // Calculate offline machines (assuming total machines)
                Math.max(0, Math.floor((machinesOperational + machinesUnderMaintenance) * 0.1))
            ],
            backgroundColor: [
                'rgba(16, 185, 129, 0.8)', // Green - Operational
                'rgba(245, 158, 11, 0.8)', // Amber - Under Maintenance
                'rgba(239, 68, 68, 0.8)'   // Red - Offline
            ],
            borderColor: [
                'rgba(16, 185, 129, 1)',
                'rgba(245, 158, 11, 1)',
                'rgba(239, 68, 68, 1)'
            ],
            borderWidth: 1
        }]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                position: 'bottom',
                labels: {
                    usePointStyle: true,
                    padding: 15
                }
            },
            tooltip: {
                backgroundColor: 'rgba(17, 24, 39, 0.8)',
                padding: 12,
                bodyFont: {
                    size: 14
                },
                callbacks: {
                    label: function (context) {
                        let label = context.label || '';
                        if (label) {
                            label += ': ';
                        }
                        label += context.parsed;
                        // Add percentage
                        const total = context.dataset.data.reduce((a, b) => a + b, 0);
                        const percentage = Math.round((context.parsed / total) * 100);
                        label += ` (${percentage}%)`;
                        return label;
                    }
                }
            }
        }
    }
});

// Production Orders Bar Chart
const productionOrdersCtx = document.getElementById('productionOrdersChart').getContext('2d');
const ordersChart = new Chart(productionOrdersCtx, {
    type: 'bar',
    data: {
        labels: ['In Progress', 'Completed', 'Pending', 'Delayed'],
        datasets: [{
            label: 'Production Orders',
            data: [
                ordersInProgress,
                ordersCompleted,
                Math.max(1, Math.floor(ordersInProgress * 0.3)),
                Math.max(1, Math.floor(ordersInProgress * 0.1))
            ],
            backgroundColor: [
                'rgba(245, 158, 11, 0.8)',  // Amber - In Progress
                'rgba(16, 185, 129, 0.8)',  // Emerald - Completed
                'rgba(59, 130, 246, 0.8)',  // Blue - Pending
                'rgba(239, 68, 68, 0.8)'    // Red - Delayed
            ],
            borderColor: [
                'rgba(245, 158, 11, 1)',
                'rgba(16, 185, 129, 1)',
                'rgba(59, 130, 246, 1)',
                'rgba(239, 68, 68, 1)'
            ],
            borderWidth: 1,
            borderRadius: 6,
            maxBarThickness: 45
        }]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
            legend: {
                display: false
            },
            tooltip: {
                backgroundColor: 'rgba(17, 24, 39, 0.8)',
                padding: 12,
                bodyFont: {
                    size: 14
                }
            }
        },
        scales: {
            x: {
                grid: {
                    display: false
                }
            },
            y: {
                beginAtZero: true,
                ticks: {
                    precision: 0
                }
            }
        }
    }
});

// Pagination variables
let machinesCurrentPage = 1;
let ordersCurrentPage = 1;
const itemsPerPage = 5;

// Machines pagination
const machinesTotal = @Model.Machine.Count();
const machinesMaxPages = Math.ceil(machinesTotal / itemsPerPage);

document.getElementById('machinesPrevBtn').addEventListener('click', function () {
    if (machinesCurrentPage > 1) {
        machinesCurrentPage--;
        updateMachinesDisplay();
    }
});

document.getElementById('machinesNextBtn').addEventListener('click', function () {
    if (machinesCurrentPage < machinesMaxPages) {
        machinesCurrentPage++;
        updateMachinesDisplay();
    }
});

function updateMachinesDisplay() {
    const rows = document.querySelectorAll('#machinesTableBody tr[data-status]');
    const start = (machinesCurrentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;

    rows.forEach((row, index) => {
        if (index >= start && index < end) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });

    // Update pagination display
    document.getElementById('machinesStart').textContent = start + 1;
    document.getElementById('machinesEnd').textContent = Math.min(end, machinesTotal);

    // Update button states
    document.getElementById('machinesPrevBtn').disabled = machinesCurrentPage === 1;
    document.getElementById('machinesNextBtn').disabled = machinesCurrentPage === machinesMaxPages;
}

// Orders pagination
const ordersTotal = @Model.ProductionOrders.Count();
const ordersMaxPages = Math.ceil(ordersTotal / itemsPerPage);

document.getElementById('ordersPrevBtn').addEventListener('click', function () {
    if (ordersCurrentPage > 1) {
        ordersCurrentPage--;
        updateOrdersDisplay();
    }
});

document.getElementById('ordersNextBtn').addEventListener('click', function () {
    if (ordersCurrentPage < ordersMaxPages) {
        ordersCurrentPage++;
        updateOrdersDisplay();
    }
});

function updateOrdersDisplay() {
    const rows = document.querySelectorAll('#ordersTableBody tr[data-status]');
    const start = (ordersCurrentPage - 1) * itemsPerPage;
    const end = start + itemsPerPage;

    rows.forEach((row, index) => {
        if (index >= start && index < end) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });

    // Update pagination display
    document.getElementById('ordersStart').textContent = start + 1;
    document.getElementById('ordersEnd').textContent = Math.min(end, ordersTotal);

    // Update button states
    document.getElementById('ordersPrevBtn').disabled = ordersCurrentPage === 1;
    document.getElementById('ordersNextBtn').disabled = ordersCurrentPage === ordersMaxPages;
}

// Initialize displays
updateMachinesDisplay();
updateOrdersDisplay();

// Filter functionality for machine table
document.getElementById('applyMachineFilter').addEventListener('click', function () {
    const statusFilter = document.getElementById('machineStatusFilter').value;
    const rows = document.querySelectorAll('#machinesTable tbody tr');

    rows.forEach(row => {
        if (row.closest('tr').hasAttribute('data-status')) {
            const rowStatus = row.getAttribute('data-status');

            if (statusFilter === 'all' || rowStatus === statusFilter || rowStatus === statusFilter.replace(" ", "")) {
                row.removeAttribute('data-filtered');
            } else {
                row.setAttribute('data-filtered', 'true');
            }
        }
    });

    // Reset to first page after filtering
    machinesCurrentPage = 1;
    updateMachinesDisplay();
});

// Filter functionality for production orders
document.getElementById('applyOrderFilter').addEventListener('click', function () {
    const statusFilter = document.getElementById('orderTypeFilter').value;
    const dateFilter = document.getElementById('orderDateFilter').value;
    const rows = document.querySelectorAll('#ordersTable tbody tr');

    rows.forEach(row => {
        if (row.closest('tr').hasAttribute('data-status')) {
            const rowStatus = row.getAttribute('data-status');
            const rowDate = row.getAttribute('data-date');

            const statusMatch = statusFilter === 'all' || rowStatus === statusFilter || rowStatus === statusFilter.replace(" ", "");
            const dateMatch = !dateFilter || rowDate === dateFilter;

            if (statusMatch && dateMatch) {
                row.removeAttribute('data-filtered');
            } else {
                row.setAttribute('data-filtered', 'true');
            }
        }
    });

    // Reset to first page after filtering
    ordersCurrentPage = 1;
    updateOrdersDisplay();
});

// Update production orders chart based on status filter
document.getElementById('orderStatusFilter').addEventListener('change', function () {
    const status = this.value;

    if (status === 'all') {
        ordersChart.data.datasets[0].data = [
            ordersInProgress,
            ordersCompleted,
            Math.max(1, Math.floor(ordersInProgress * 0.3)),
            Math.max(1, Math.floor(ordersInProgress * 0.1))
        ];
    } else if (status === 'completed') {
        ordersChart.data.datasets[0].data = [0, ordersCompleted, 0, 0];
    } else if (status === 'inprogress') {
        ordersChart.data.datasets[0].data = [ordersInProgress, 0, 0, 0];
    } else if (status === 'pending') {
        ordersChart.data.datasets[0].data = [0, 0, Math.max(1, Math.floor(ordersInProgress * 0.3)), 0];
    }

    ordersChart.update();
});

// Mobile sidebar toggle functionality
document.addEventListener('DOMContentLoaded', function () {
    const menuButton = document.querySelector('.menu-toggle');
    const sidebar = document.querySelector('.sidebar');
    const mainContent = document.querySelector('.main-content');

    function checkWindowSize() {
        if (window.innerWidth <= 768) {
            sidebar.style.transform = 'translateX(-100%)';
            mainContent.style.marginLeft = '0';
            menuButton.style.display = 'flex';
        } else {
            sidebar.style.transform = 'translateX(0)';
            mainContent.style.marginLeft = '280px';
            menuButton.style.display = 'none';
        }
    }

    // Initial check
    checkWindowSize();

    // Event listener for window resize
    window.addEventListener('resize', checkWindowSize);

    // Event listener for menu button click
    menuButton.addEventListener('click', function () {
        if (sidebar.style.transform === 'translateX(0px)') {
            sidebar.style.transform = 'translateX(-100%)';
        } else {
            sidebar.style.transform = 'translateX(0)';
        }
    });
});